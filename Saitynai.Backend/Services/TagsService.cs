using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Core;
using static Saitynai.Backend.Controllers.TagGroupsController;

namespace Saitynai.Backend.Services;

public interface ITagsService
{
	Task<(TagGroup?, ResultError?)> CreateGroupAsync(Guid callerId, RequestCreateTagGroupDto requestDto);
	Task<(Tag?, ResultError?)> CreateTagAsync(Guid callerId, Guid tagGroupId, string name);

	Task<(TagGroup?, ResultError?)> GetGroupAsync(Guid callerId, Guid tagGroupId);

	Task<(List<Tag>, ResultError?)> ListTagsAsync(Guid callerId, RequestListTagsDto requestDto);

	Task<ResultError?> EditTagAsync(Guid callerId, Guid tagId, RequestEditTagDto requestDto);
	Task<ResultError?> EditGroupAsync(Guid callerId, Guid tagGroupid, RequestEditTagGroupDto requestDto);

	Task<ResultError?> DeleteGroupAsync(Guid callerId, Guid tagGroupId);
	Task<ResultError?> DeleteTagAsync(Guid callerId, Guid tagId);
}

public class TagsService : ITagsService
{
	private readonly SaitynaiDbContext _dbContext;
	private readonly ICrudService _crudService;
	private readonly IUserService _userService;

	public TagsService(
		SaitynaiDbContext dbContext,
		ICrudService crudService,
		IUserService userService)
	{
		_dbContext = dbContext;
		_crudService = crudService;
		_userService = userService;
	}

	public async Task<(TagGroup?, ResultError?)> CreateGroupAsync(Guid callerId, RequestCreateTagGroupDto requestDto)
	{
		try
		{
			var tagGroup = new TagGroup
			{
				OwnerId = callerId,
				Name = requestDto.Name,
				ParentId = requestDto.ParentGroupId,
			};

			_dbContext.Add(tagGroup);
			await _dbContext.SaveChangesAsync();

			return (tagGroup, null);
		}
		catch (Exception ex)
		{
			return (null, new ResultError(500, ex.Message));
		}
	}

	public async Task<(Tag?, ResultError?)> CreateTagAsync(Guid callerId, Guid tagGroupId, string name)
	{
		var tag = new Tag
		{
			Name = name,
			GroupId = tagGroupId,
			OwnerId = callerId,
		};

		_dbContext.Add(tag);
		await _dbContext.SaveChangesAsync();

		return (tag, null);
	}

	public async Task<(TagGroup?, ResultError?)> GetGroupAsync(Guid callerId, Guid tagGroupId)
	{
		var tagGroup = await _dbContext.Set<TagGroup>()
			.Include(x => x.Parent)
			.Include(x => x.ChildrenGroups)
			.Where(x => x.Id == tagGroupId)
			.FirstOrDefaultAsync();

		if (tagGroup == null)
			return (null, new ResultError(404, "Tag group not found."));

		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return (null, error);

		if (tagGroup.OwnerId != callerId && !caller.IsAdmin)
		{
			return (null, new ResultError(403, "Tag group belongs to another user."));
		}

		return (tagGroup, null);
	}

	public async Task<(List<Tag>?, ResultError?)> ListTagsAsync(Guid callerId, RequestListTagsDto requestDto)
	{
		var query = _dbContext.Set<Tag>().AsQueryable();

		if (requestDto.TagGroupId != null)
		{
			var tagGroup = await _dbContext.Set<TagGroup>().FindAsync(requestDto.TagGroupId);
			if (tagGroup == null)
				return (null, new ResultError(404, "Tag Group was not found."));

			var (caller, error) = await _userService.GetUserAsync(callerId);
			if (error != null)
				return (null, error);

			if (tagGroup.OwnerId != callerId && !caller!.IsAdmin)
				return (null, new ResultError(403, "Tag Group is owned by another user. Edit forbidden."));

			query = query.Where(x => x.GroupId == requestDto.TagGroupId);
		}

		var callerTags = await query.Where(x => x.OwnerId == callerId).ToListAsync();
		var publicTags = await query.Where(x => x.OwnerId == null).ToListAsync();

		return ([.. callerTags, .. publicTags], null);
	}

	public async Task<ResultError?> EditTagAsync(Guid callerId, Guid tagId, RequestEditTagDto requestDto)
	{
		var transaction = _dbContext.Database.BeginTransaction();
		try
		{
			var existingEntity = await _dbContext.Set<Tag>().FindAsync(tagId);
			if (existingEntity == null)
				return new ResultError(404, "Tag not found.");

			var (caller, error) = await _userService.GetUserAsync(callerId);
			if (error != null)
				return error;

			if (existingEntity.OwnerId != callerId && !caller.IsAdmin)
				return new ResultError(403, "Tag is owned by another user. Edit forbidden.");

			if (requestDto.Name != null)
				existingEntity.Name = requestDto.Name;

			if (requestDto.TagGroupId != null)
			{
				var tagGroup = await _dbContext.Set<TagGroup>().FindAsync(requestDto.TagGroupId);

				if (tagGroup == null)
					return new ResultError(404, "Tag group not found.");

				if (tagGroup.OwnerId != callerId && !caller.IsAdmin)
					return new ResultError(403, "Cannot change tag's group into a group that belongs to another user.");

				existingEntity.GroupId = tagGroup.Id;
			}

			_dbContext.Update(existingEntity);
			await _dbContext.SaveChangesAsync();

			await transaction.CommitAsync();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return new ResultError(500, ex.Message);
		}

		return null;
	}

	public async Task<ResultError?> EditGroupAsync(Guid callerId, Guid tagGroupid, RequestEditTagGroupDto requestDto)
	{
		var transaction = _dbContext.Database.BeginTransaction();
		try
		{
			var existingEntity = await _dbContext.Set<TagGroup>().FindAsync(tagGroupid);
			if (existingEntity == null)
				return new ResultError(404, "Tag Group not found.");

			var (caller, error) = await _userService.GetUserAsync(callerId);
			if (error != null)
				return error;

			if (existingEntity.OwnerId != callerId && !caller.IsAdmin)
				return new ResultError(403, "Tag Group is owned by another user. Edit forbidden.");

			if (requestDto.Name != null)
				existingEntity.Name = requestDto.Name;

			if (requestDto.ParentTagGroupId != null)
			{
				var parentTagGroup = await _dbContext.Set<TagGroup>().FindAsync(requestDto.ParentTagGroupId);

				if (parentTagGroup == null)
					return new ResultError(404, "Parent Tag Group not found.");

				if (parentTagGroup.OwnerId != callerId && !caller.IsAdmin)
					return new ResultError(403, "Cannot change tag's parent group into a group that belongs to another user.");

				existingEntity.ParentId = parentTagGroup.Id;
			}

			_dbContext.Update(existingEntity);
			await _dbContext.SaveChangesAsync();

			await transaction.CommitAsync();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return new ResultError(500, ex.Message);
		}

		return null;
	}

	public async Task<ResultError?> DeleteGroupAsync(Guid callerId, Guid tagGroupId)
	{
		return await _crudService.DeleteAsync<TagGroup>(callerId, tagGroupId);
	}

	public async Task<ResultError?> DeleteTagAsync(Guid callerId, Guid tagId)
	{
		return await _crudService.DeleteAsync<Tag>(callerId, tagId);
	}
}
