using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Core;
using Saitynai.Backend.Extensions;
using Saitynai.Backend.Services;
using System.ComponentModel.DataAnnotations;

namespace Saitynai.Backend.Controllers;

[Authorize]
[ApiController]
[Route("notes")]
public class NotesController : ControllerBase
{
	private readonly INotesService _notesService;

	public NotesController(INotesService notesService)
	{
		_notesService = notesService;
	}

	public class RequestCreateNoteDto
	{
		[MinLength(10)]
		[MaxLength(10_000)]
		public required string Content { get; set; }
	}

	[HttpPost]
	public async Task<IActionResult> CreateAsync([FromBody] RequestCreateNoteDto requestDto)
	{
		var userId = User.GetUserId();
		var note = await _notesService.CreateAsync(userId, requestDto.Content);

		var responseDto = ToResponseDto(note);
		return Created($"notes/{note.Id}", responseDto);
	}

	public class ResponseGetNoteDto
	{
		public required Guid Id { get; set; }
		public required string Content { get; set; }
		public float[]? Embedding { get; set; }
	}
	[HttpGet("{noteId:guid}")]
	public async Task<ActionResult<ResponseGetNoteDto>> GetAsync(Guid noteId)
	{
		var userId = User.GetUserId();

		var (note, error) = await _notesService.GetAsync(userId, noteId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		var responseDto = ToResponseDto(note);
		return Ok(responseDto);
	}

	public class RequestListNotesDto
	{
		public string? Search { get; set; }
	}
	public class ResponseListNoteDto
	{
		public required Guid Id { get; set; }
		public required string Content { get; set; }
		public required List<string> Tags { get; set; }
	}
	[HttpGet]
	public async Task<ActionResult<List<ResponseListNoteDto>>> ListPersonalNotesAsync([FromQuery] RequestListNotesDto requestDto)
	{
		var userId = User.GetUserId();

		var notes = await _notesService.ListPersonalNotesAsync(userId, requestDto.Search);

		return Ok(notes);
	}

	public class ResponseNoteTagsDto
	{
		public required Guid NoteId { get; set; }
		public required List<ResponseTagDto> Tags { get; set; } = [];
	}
	public class ResponseTagDto
	{
		public required Guid Id { get; set; }
		public required Guid TagId { get; set; }
		public required Guid TagGroupId { get; set; }
		public required string Name { get; set; }
	}

	[HttpGet("{noteId:guid}/tags")]
	public async Task<ActionResult<ResponseNoteTagsDto>> ListNoteTagsAsync([FromRoute] Guid noteId)
	{
		var userId = User.GetUserId();

		var (note, error) = await _notesService.ListNoteTagsAsync(userId, noteId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		var responseDto = new ResponseNoteTagsDto()
		{
			NoteId = note.Id,
			Tags = note.Tags.Select(x => new ResponseTagDto
			{
				Id = x.Id,
				TagId = x.TagId,
				TagGroupId = x.Tag.GroupId,
				Name = x.Tag.Name
			}).ToList()
		};

		return Ok(responseDto);
	}

	[HttpPost("{noteId:guid}/compute-embedding")]
	public async Task<IActionResult> ComputeEmbeddingsAsync([FromRoute] Guid noteId)
	{
		var userId = User.GetUserId();

		var error = await _notesService.ComputeEmbeddingsAsync(userId, noteId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	public class RequestEditContentDto
	{
		[MinLength(10)]
		[MaxLength(10_000)]
		public string NewContent { get; set; }
	}
	[HttpPatch("{noteId:guid}")]
	public async Task<IActionResult> EditContentAsync([FromRoute] Guid noteId, [FromBody] RequestEditContentDto requestDto)
	{
		var userId = User.GetUserId();

		var error = await _notesService.EditContentAsync(userId, noteId, requestDto.NewContent);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	[HttpDelete("{noteId:guid}")]
	public async Task<IActionResult> DeleteAsync([FromRoute] Guid noteId)
	{
		var userId = User.GetUserId();

		var error = await _notesService.DeleteAsync(userId, noteId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	private static ResponseGetNoteDto ToResponseDto(Note? note)
	{
		var responseDto = new ResponseGetNoteDto()
		{
			Id = note.Id,
			Content = note.Content
		};

		if (note.Embedding != null)
			responseDto.Embedding = note.Embedding.Value.ToArray();
		return responseDto;
	}

	[HttpPost("{noteId:guid}/tags/{tagId:guid}")]
	public async Task<IActionResult> AddTagAsync([FromRoute] Guid noteId, [FromRoute] Guid tagId)
	{
		var userId = User.GetUserId();

		var error = await _notesService.AddTagAsync(userId, noteId, tagId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	[HttpDelete("{noteId:guid}/tags/{tagId:guid}")]
	public async Task<IActionResult> RemoveTagAsync([FromRoute] Guid noteId, [FromRoute] Guid tagId)
	{
		var userId = User.GetUserId();

		var error = await _notesService.RemoveTagAsync(userId, noteId, tagId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	[HttpPost("sync-tags")]
	public async Task<IActionResult> SyncTagsAsync()
	{
		var userId = User.GetUserId();

		var error = await _notesService.SyncTagsAsync(userId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return Ok();
	}

	public class SuggestTagsResponseDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}

	[HttpGet("{noteId:guid}/suggest-tags")]
	public async Task<ActionResult<List<SuggestTagsResponseDto>>> SuggestTagsAsync([FromRoute] Guid noteId)
	{
		var userId = User.GetUserId();

		var (tags, error) = await _notesService.SuggestTagsAsync(userId, noteId);
		if (error != null)
			return StatusCode(error.StatusCode, error.Message);

		return tags
			.Select(x => new SuggestTagsResponseDto { Id = x.Id, Name = x.Name })
			.ToList();
	}
}
