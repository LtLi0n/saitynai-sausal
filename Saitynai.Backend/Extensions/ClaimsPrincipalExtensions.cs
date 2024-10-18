using System.Security.Claims;

namespace Saitynai.Backend.Extensions;

public static class ClaimsPrincipalExtensions
{
	public static Guid GetUserId(this ClaimsPrincipal user)
	{
		var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

		if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
		{
			return userId;
		}
		else
		{
			return Guid.Empty;
		}
	}
}
