using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saitynai.Backend.Services;

namespace Saitynai.Backend.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
	private readonly IUserService _userService;

	public UsersController(IUserService userService)
	{
		_userService = userService;
	}

	public class RequestLoginDto
	{
		public required string Username { get; set; }
		public required string Password { get; set; }
	}
	public class ResponseLoginDto
	{
		public required string Token { get; set; }
	}

	[AllowAnonymous]
	[HttpPost("login")]
	public async Task<ActionResult<ResponseLoginDto>> LoginAsync([FromBody] RequestLoginDto requestDto)
	{
		var (token, error) = await _userService.LoginAsync(requestDto.Username, requestDto.Password);
		if (error != null)
		{
			return StatusCode(error.StatusCode, error.Message);
		}

		var toReturn = new ResponseLoginDto() 
		{
			Token = token
		};

		return toReturn;
	}
}
