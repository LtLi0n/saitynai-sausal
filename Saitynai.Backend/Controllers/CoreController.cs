using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Saitynai.Backend.Controllers;

[ApiController]
[Route("api")]
public class CoreController : ControllerBase
{
	[HttpGet("health")]
	public async Task<IActionResult> HealthAsync()
	{
		return Ok();
	}

	[Authorize]
	[HttpGet("is-logged-in")]
	public async Task<IActionResult> IsLoggedInAsync()
	{
		return Ok();
	}
}
