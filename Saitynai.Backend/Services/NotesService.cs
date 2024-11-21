using Microsoft.EntityFrameworkCore;
using OpenAI.Embeddings;
using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Core;
using static Saitynai.Backend.Controllers.NotesController;

namespace Saitynai.Backend.Services;

public interface INotesService
{
	Task<Note> CreateAsync(Guid callerId, string content);
	Task<(Note?, ResultError?)> GetAsync(Guid callerId, Guid noteId);
	Task<(Note?, ResultError?)> ListNoteTagsAsync(Guid callerId, Guid noteId);
	Task<List<ResponseListNoteDto>> ListPersonalNotesAsync(Guid callerId);
	Task<ResultError?> ComputeEmbeddingsAsync(Guid callerId, Guid noteId);
	Task<ResultError?> EditContentAsync(Guid callerId, Guid noteId, string newContent);
	Task<ResultError?> DeleteAsync(Guid callerId, Guid noteId);
}

public class NotesService : INotesService
{
	private readonly SaitynaiDbContext _dbContext;
	private readonly IConfiguration _configuration;
	private readonly ICrudService _crudService;
	private readonly IUserService _userService;

	public NotesService(
		SaitynaiDbContext dbContext,
		IConfiguration configuration,
		ICrudService crudService,
		IUserService userService)
	{
		_dbContext = dbContext;
		_configuration = configuration;
		_crudService = crudService;
		_userService = userService;
	}

	public async Task<Note> CreateAsync(Guid callerId, string content)
	{
		var note = new Note()
		{
			OwnerId = callerId,
			Content = content,
		};
		_dbContext.Add(note);

		await _dbContext.SaveChangesAsync();

		return note;
	}

	public async Task<(Note?, ResultError?)> GetAsync(Guid callerId, Guid noteId)
	{
		var note = await _dbContext.Set<Note>()
			.Include(x => x.Embedding)
			.Where(x => x.Id == noteId)
			.FirstOrDefaultAsync();
		if (note == null)
			return (null, new ResultError(404, "Note not found."));

		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return (null, error);

		if (note.OwnerId != callerId && !caller.IsAdmin)
			return (null, new ResultError(403, "Note belongs to another user."));

		return (note, null);
	}

	public async Task<(Note?, ResultError?)> ListNoteTagsAsync(Guid callerId, Guid noteId)
	{
		var note = await _dbContext.Set<Note>()
			.Include(x => x.Tags)
				.ThenInclude(x => x.Tag)
			.FirstOrDefaultAsync(x => x.Id == noteId);
		if (note == null)
			return (null, new ResultError(404, "Note not found."));

		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return (null, error);

		if (note.OwnerId != callerId && !caller.IsAdmin)
			return (null, new ResultError(403, "Note belongs to another user."));

		return (note, null);
	}

	public async Task<List<ResponseListNoteDto>> ListPersonalNotesAsync(Guid callerId)
	{
		var notes = await _dbContext.Set<Note>()
			.Where(x => x.OwnerId == callerId)
			.Select(x => new ResponseListNoteDto
			{
				Id = x.Id,
				Content = x.Content
			})
			.ToListAsync();

		return notes;
	}

	public async Task<ResultError?> ComputeEmbeddingsAsync(Guid callerId, Guid noteId)
	{
		var transaction = _dbContext.Database.BeginTransaction();
		try
		{
			var note = await _dbContext.Set<Note>().FindAsync(noteId);
			if (note == null)
			{
				return new ResultError(404, "Note not found.");
			}

			var (caller, error) = await _userService.GetUserAsync(callerId);
			if (error != null)
				return error;

			if (note.OwnerId != callerId && !caller.IsAdmin)
			{
				return new ResultError(403, "Note belongs to another user.");
			}

			var embeddingClient = new EmbeddingClient("text-embedding-3-small", _configuration["OpenAI"]);
			var result = await embeddingClient.GenerateEmbeddingAsync(note.Content);
			var embeddingFloats = result.Value.ToFloats();

			var embedding = new Embedding()
			{
				Value = new Pgvector.Vector(embeddingFloats),
			};
			_dbContext.Add(embedding);
			await _dbContext.SaveChangesAsync();

			note.EmbeddingId = embedding.Id;
			_dbContext.Update(note);
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

	public async Task<ResultError?> EditContentAsync(Guid callerId, Guid noteId, string newContent)
	{
		var transaction = _dbContext.Database.BeginTransaction();
		try
		{
			var note = await _dbContext.Set<Note>()
				.Include(x => x.Embedding)
				.FirstOrDefaultAsync(x => x.Id == noteId);
			if (note == null)
				return new ResultError(404, "Note not found.");

			var (caller, error) = await _userService.GetUserAsync(callerId);
			if (error != null)
				return error;

			if (note.OwnerId != callerId && !caller.IsAdmin)
				return new ResultError(403, "Note belongs to another user.");

			note.Content = newContent;
			if (note.Embedding != null)
			{
				note.EmbeddingId = null;
				_dbContext.Remove(note.Embedding);
			}

			_dbContext.Update(note);
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

	public async Task<ResultError?> DeleteAsync(Guid callerId, Guid noteId)
	{
		return await _crudService.DeleteAsync<Note>(callerId, noteId);
	}
}
