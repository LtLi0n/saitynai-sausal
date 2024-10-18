using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Core;
using Saitynai.Backend.DataAccess;

namespace Saitynai.Backend.Services;

public interface ICrudService
{
	Task<ResultError?> DeleteAsync<T>(Guid callerId, Guid entityId)
		where T : class, IOwnableEntity;
}

public class CrudService : ICrudService
{
	private readonly SaitynaiDbContext _dbContext;
	private readonly IUserService _userService;

	public CrudService(
		SaitynaiDbContext dbContext,
		IUserService userService)
	{
		_dbContext = dbContext;
		_userService = userService;
	}

	public async Task<ResultError?> DeleteAsync<T>(Guid callerId, Guid entityId)
		where T : class, IOwnableEntity
	{
		var entity = await _dbContext.Set<T>().FindAsync(entityId);

		if (entity == null)
			return new ResultError(404, "Entity not found.");

		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return error;

		if (entity.OwnerId != callerId && !caller.IsAdmin)
		{
			return new ResultError(403, "Entity belongs to another user.");
		}

		_dbContext.Remove(entity);
		await _dbContext.SaveChangesAsync();

		return null;
	}
}
