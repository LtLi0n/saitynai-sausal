using Microsoft.EntityFrameworkCore;
using OpenAI.Embeddings;
using Pgvector.EntityFrameworkCore;
using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Core;
using System.Collections.Concurrent;
using static Saitynai.Backend.Controllers.NotesController;

namespace Saitynai.Backend.Services;

public interface INotesService
{
	Task<Note> CreateAsync(Guid callerId, string content);
	Task<(Note?, ResultError?)> GetAsync(Guid callerId, Guid noteId);
	Task<(Note?, ResultError?)> ListNoteTagsAsync(Guid callerId, Guid noteId);
	Task<List<ResponseListNoteDto>> ListPersonalNotesAsync(Guid callerId, string? search);
	Task<ResultError?> ComputeEmbeddingsAsync(Guid callerId, Guid noteId);
	Task<ResultError?> EditContentAsync(Guid callerId, Guid noteId, string newContent);
	Task<ResultError?> DeleteAsync(Guid callerId, Guid noteId);
	Task<ResultError?> AddTagAsync(Guid callerId, Guid noteId, Guid tagId);
	Task<ResultError?> RemoveTagAsync(Guid callerId, Guid noteId, Guid tagId);
	Task<ResultError?> SyncTagsAsync(Guid callerId);
	Task<(List<Tag>?, ResultError?)> SuggestTagsAsync(Guid callerId, Guid noteId);
}

public class NotesService : INotesService
{
	private static readonly ConcurrentDictionary<string, Pgvector.Vector> _cache = [];

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
			CreatedAt = DateTime.UtcNow,
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

	public async Task<List<ResponseListNoteDto>> ListPersonalNotesAsync(Guid callerId, string? search)
	{
		var query = _dbContext.Set<Note>()
			.Include(x => x.Tags)
			.OrderByDescending(x => x.CreatedAt)
			.Where(x => x.OwnerId == callerId);

		if (search != null && search.Trim().Length > 0)
		{
			var embedding = await GetEmbeddingAsync(search);
			query = query
				.OrderBy(x => x.Embedding.Value!.CosineDistance(embedding));
		}

		var notes = await query
			.Select(x => new ResponseListNoteDto
			{
				Id = x.Id,
				Content = x.Content,
				Tags = x.Tags.Select(x => x.Tag.Name).ToList(),
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
				return new ResultError(404, "Note not found.");

			var (caller, error) = await _userService.GetUserAsync(callerId);
			if (error != null)
				return error;

			if (note.OwnerId != callerId && !caller.IsAdmin)
				return new ResultError(403, "Note belongs to another user.");

			var embeddingValue = await GetEmbeddingAsync(note.Content);
			var embedding = new Embedding()
			{
				Value = embeddingValue,
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

	private async Task<Pgvector.Vector> GetEmbeddingAsync(string text)
	{
		if (!_cache.TryGetValue(text, out var embedding))
		{
			var embeddingClient = new EmbeddingClient("text-embedding-3-small", _configuration["OpenAI"]);
			var result = await embeddingClient.GenerateEmbeddingAsync(text);
			var embeddingFloats = result.Value.ToFloats();

			embedding = new Pgvector.Vector(embeddingFloats);
			_cache[text] = embedding;
		}

		return embedding;
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

	public async Task<ResultError?> AddTagAsync(Guid callerId, Guid noteId, Guid tagId)
	{
		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return error;

		var note = await _dbContext.Set<Note>().FindAsync(noteId);
		if (note == null)
			return new ResultError(404, "Note doesn't exist.");
		if (note.OwnerId != caller.Id && note.OwnerId != null && !caller.IsAdmin)
			return new ResultError(403, "Cannot add tag to note that belongs to a different user.");

		var tag = await _dbContext.Set<Tag>().FindAsync(tagId);
		if (tag == null)
			return new ResultError(404, "Tag doesn't exist.");
		if (tag.OwnerId != caller.Id && tag.OwnerId != null && !caller.IsAdmin)
			return new ResultError(403, "Cannot add tag that belongs to a different user.");

		var noteTag = await _dbContext.Set<NoteTag>()
			.Where(x => x.NoteId == noteId && x.TagId == tag.Id)
			.FirstOrDefaultAsync();
		if (noteTag != null)
			return new ResultError(422, "Note already contains such tag.");

		noteTag = new NoteTag
		{
			NoteId = noteId,
			TagId = tagId,
		};
		_dbContext.Add(noteTag);
		await _dbContext.SaveChangesAsync();

		return null;
	}

	public async Task<ResultError?> RemoveTagAsync(Guid callerId, Guid noteId, Guid tagId)
	{
		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return error;

		var noteTag = await _dbContext.Set<NoteTag>()
			.Include(x => x.Tag)
			.Include(x => x.Note)
			.Where(x => x.NoteId == noteId && x.TagId == tagId)
			.FirstOrDefaultAsync();
		if (noteTag == null)
			return new ResultError(404, "Note doesn't have this tag.");

		if (noteTag.Note.OwnerId != caller.Id && noteTag.Note.OwnerId != null && !caller.IsAdmin)
			return new ResultError(403, "Cannot remove tag from note that belongs to a different user.");

		_dbContext.Remove(noteTag);
		await _dbContext.SaveChangesAsync();

		return null;
	}

	public async Task<ResultError?> SyncTagsAsync(Guid callerId)
	{
		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return error;

		if (!caller.IsAdmin)
			return new ResultError(403, "Only admins can sync tag embeddings.");

		var noteTags = await _dbContext.Set<NoteTag>()
			.Include(x => x.Note)
				.ThenInclude(x => x.Embedding)
			.Include(x => x.Tag)
			.ToListAsync();

		foreach (var tagNotes in noteTags.GroupBy(x => x.TagId))
		{
			var notes = tagNotes.Select(x => x.Note).ToArray();
			var pointsSum = new List<float>();
			var embeddedCount = 0;

			for (var i = 0; i < notes.Length; i++)
			{
				var note = notes[i];
				if (note.Embedding == null)
					continue;

				var points = note.Embedding.Value.ToArray();
				
				for(var j = 0; j < points.Length; j++)
				{
					if (pointsSum.Count < j + 1)
						pointsSum.Add(points[j]);
					else
						pointsSum[j] += points[j];
				}

				embeddedCount++;
			}

			if (embeddedCount == 0)
				continue;

			for(var i = 0; i < pointsSum.Count; i++)
				pointsSum[i] /= embeddedCount;

			var averagedEmbedding = new ReadOnlyMemory<float>([.. pointsSum]);
			var tag = tagNotes.First().Tag;
			tag.Embedding = new Pgvector.Vector(averagedEmbedding);
			_dbContext.Update(tag);
		}

		await _dbContext.SaveChangesAsync();
		return null;
	}

	public async Task<(List<Tag>?, ResultError?)> SuggestTagsAsync(Guid callerId, Guid noteId)
	{
		var (caller, error) = await _userService.GetUserAsync(callerId);
		if (error != null)
			return (null, error);

		var note = await _dbContext.Set<Note>()
			.Include(x => x.Tags)
			.Include(x => x.Embedding)
			.Where(x => x.Id == noteId)
			.FirstOrDefaultAsync();
		if (note == null)
			return (null, new ResultError(404, "Note doesn't exist."));

		if (note.OwnerId != caller.Id && note.OwnerId != null && !caller.IsAdmin)
			return (null, new ResultError(403, "Cannot suggest tags for note that belongs to another user."));

		if (note.Embedding == null)
			return (null, new ResultError(422, "Note must be embedded to get tag suggestions."));

		var attachedTagIds = note.Tags.Select(x => x.TagId).ToList();

		var tags = await _dbContext.Set<Tag>()
			.Where(x => !attachedTagIds.Contains(x.Id) && x.Embedding != null)
			.OrderBy(x => x.Embedding!.CosineDistance(note.Embedding.Value))
			.Take(5)
			.ToListAsync();

		return (tags, null);
	}
}
