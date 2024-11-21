using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Extensions;
using Saitynai.Backend.Services;
using System.ComponentModel.DataAnnotations;
using static Saitynai.Backend.Controllers.NotesController;

namespace Saitynai.Backend.Controllers;

[Authorize]
[ApiController]
[Route("tag-groups")]
public class TagGroupsController : ControllerBase
{
	private readonly ITagsService _tagsService;

	public TagGroupsController(ITagsService tagsService)
	{
		_tagsService = tagsService;
	}

	public class RequestCreateTagGroupDto
	{
		public Guid? ParentGroupId { get; set; }
		[MinLength(1), MaxLength(100)]
		public required string Name { get; set; }
	}

	[HttpPost]
	public async Task<ActionResult<ResponseGetTagGroupDto>> CreateGroupAsync([FromBody] RequestCreateTagGroupDto requestDto)
	{
		var userId = User.GetUserId();
		var (tagGroup, error) = await _tagsService.CreateGroupAsync(userId, requestDto);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		var responseDto = ToFullResponseDto(tagGroup);

		return Created($"tag-groups/{tagGroup.Id}", responseDto);
	}

	public class RequestCreateTagDto
	{
		[MinLength(1), MaxLength(100)]
		public required string Name { get; set; }
	}
	[HttpPost("{tagGroupId:guid}/tags")]
	public async Task<ActionResult<Tag>> CreateTagAsync([FromRoute] Guid tagGroupId, [FromBody] RequestCreateTagDto requestDto)
	{
		var userId = User.GetUserId();
		var (tag, error) = await _tagsService.CreateTagAsync(userId, tagGroupId, requestDto.Name);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok(tag);
	}

	public class ResponseGetTagGroupDto
	{
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public ResponseGetTagGroupDto Parent { get; set; }
		public List<ResponseGetTagGroupDto> Children { get; set; } = [];
	}
	[HttpGet("{tagGroupId:guid}")]
	public async Task<ActionResult<ResponseGetTagGroupDto>> GetGroup([FromRoute] Guid tagGroupId)
	{
		var userId = User.GetUserId();
		var (tagGroup, error) = await _tagsService.GetGroupAsync(userId, tagGroupId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		var responseDto = ToFullResponseDto(tagGroup);

		return Ok(responseDto);
	}

	public class RequestListTagsDto
	{
		public bool? IncludePublicTags { get; set; }
	}
	public class ResponseListTagDto
	{
		public required Guid Id { get; set; }
		public required Guid GroupId { get; set; }
		public required string Name { get; set; }
		public required Guid? OwnerId { get; set; }
	}
	[HttpGet("{tagGroupId:guid}/tags")]
	public async Task<ActionResult<List<Tag>>> ListAsync([FromRoute] Guid tagGroupId, [FromQuery] RequestListTagsDto requestDto)
	{
		var userId = User.GetUserId();

		var (tags, error) = await _tagsService.ListTagsAsync(userId, tagGroupId, requestDto);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		var toReturn = tags.Select(x => new ResponseListTagDto()
		{
			Id = x.Id,
			GroupId = x.GroupId,
			Name = x.Name,
			OwnerId = x.OwnerId,
		}).ToList();

		return Ok(toReturn);
	}

	public class RequestEditTagDto
	{
		public string? Name { get; set; }
		public Guid? TagGroupId { get; set; }
	}
	[HttpPatch("tags/{tagId:guid}")]
	public async Task<IActionResult> EditTagAsync([FromRoute] Guid tagId, [FromBody] RequestEditTagDto requestDto)
	{
		var userId = User.GetUserId();

		var error = await _tagsService.EditTagAsync(userId, tagId, requestDto);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	public class RequestEditTagGroupDto
	{
		public string? Name { get; set; }
		public Guid? ParentTagGroupId { get; set; }
	}
	[HttpPatch("{tagGroupId:guid}")]
	public async Task<IActionResult> EditGroupAsync([FromRoute] Guid tagGroupId, [FromBody] RequestEditTagGroupDto requestDto)
	{
		var userId = User.GetUserId();

		var error = await _tagsService.EditGroupAsync(userId, tagGroupId, requestDto);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	[HttpDelete("{tagGroupId:guid}")]
	public async Task<IActionResult> DeleteGroupAsync([FromRoute] Guid tagGroupId)
	{
		var userId = User.GetUserId();

		var error = await _tagsService.DeleteGroupAsync(userId, tagGroupId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	[HttpDelete("tags/{tagId:guid}")]
	public async Task<IActionResult> DeleteTagAsync([FromRoute] Guid tagId)
	{
		var userId = User.GetUserId();

		var error = await _tagsService.DeleteTagAsync(userId, tagId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	private static ResponseGetTagGroupDto ToFullResponseDto(TagGroup? tagGroup)
	{
		var responseDto = new ResponseGetTagGroupDto()
		{
			Id = tagGroup.Id,
			Name = tagGroup.Name,
		};

		if (tagGroup.ChildrenGroups != null)
		{
			var children = tagGroup.ChildrenGroups.Select(ToResponseDto).ToList();
			responseDto.Children = children;
		}

		if (tagGroup.Parent != null)
			responseDto.Parent = ToResponseDto(tagGroup.Parent);

		return responseDto;
	}

	private static ResponseGetTagGroupDto ToResponseDto(TagGroup? tagGroup)
	{
		var responseDto = new ResponseGetTagGroupDto()
		{
			Id = tagGroup.Id,
			Name = tagGroup.Name,
		};

		return responseDto;
	}
}
