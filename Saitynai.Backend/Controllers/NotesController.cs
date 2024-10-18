using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Extensions;
using Saitynai.Backend.Services;

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
	}
	[HttpGet]
	public async Task<ActionResult<List<ResponseListNoteDto>>> ListPersonalNotesAsync()
	{
		var userId = User.GetUserId();
		
		var notes = await _notesService.ListPersonalNotesAsync(userId);

		return Ok(notes);
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
}
