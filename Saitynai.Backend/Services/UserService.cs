using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Saitynai.Backend.Contracts.Models;
using Saitynai.Backend.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Saitynai.Backend.Services;

public interface IUserService
{
	Task<(string?, ResultError?)> LoginAsync(string username, string password);
	Task<(User?, ResultError?)> GetUserAsync(Guid id);
}

public class UserService : IUserService
{
	private readonly SaitynaiDbContext _dbContext;
	private readonly IConfiguration _configuration;

	public UserService(SaitynaiDbContext dbContext, IConfiguration configuration)
	{
		_dbContext = dbContext;
		_configuration = configuration;
	}

	public async Task<(string?, ResultError?)> LoginAsync(string username, string password)
	{
		var existingUser = await _dbContext.Set<User>()
			.Where(x => x.Username == username && x.Password == password)
			.FirstOrDefaultAsync();

		if (existingUser == null)
		{
			return (null, new ResultError(404, "No such user exists."));
		}

		var token = GenerateJwtToken(existingUser, _configuration);
		return (token, null);
	}

	public async Task<(User?, ResultError?)> GetUserAsync(Guid id)
	{
		var user = await _dbContext.Set<User>().FindAsync(id);
		if (user == null)
		{
			return (null, new ResultError(404, "User not found."));
		}

		return (user, null);
	}

	private static string GenerateJwtToken(User user, IConfiguration configuration)
	{
		var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
		var securityKey = new SymmetricSecurityKey(key);
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(JwtRegisteredClaimNames.PreferredUsername, user.Username),
			new Claim("is_admin", user.IsAdmin.ToString().ToLower())
		};

		var token = new JwtSecurityToken(
			claims: claims,
			expires: DateTime.Now.AddHours(1),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
	{
		using var hmac = new HMACSHA512();
		passwordSalt = hmac.Key;
		passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
	}

	private static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
	{
		using var hmac = new HMACSHA512(storedSalt);
		var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

		return computedHash.SequenceEqual(storedHash);
	}
}
