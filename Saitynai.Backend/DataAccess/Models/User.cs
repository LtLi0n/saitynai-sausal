using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saitynai.Backend.Contracts.Models;

[DbEntity]
[Table("users")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
public class User : IEntity
{
	public Guid Id { get; set; }

	public string Email { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }

	public bool IsAdmin { get; set; }

	public List<Note> Contents { get; set; }
	public List<TagGroup> TagGroups { get; set; }

	public static Guid UserId { get; } = new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5");

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>(x =>
		{
			x.HasMany(x => x.Contents)
			.WithOne(x => x.Owner)
			.HasForeignKey(x => x.OwnerId)
			.OnDelete(DeleteBehavior.Cascade);

			x.HasData(new List<User>() 
			{
				new()
				{
					Id = new Guid("85891986-ba23-499e-8c02-59bee76a574e"),
					Email = "adminas.adminavicius@email.com",
					Username = "admin",
					Password = "admin123",
					IsAdmin = true,
				},
				new()
				{
					Id = new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5"),
					Email = "user.useris@email.com",
					Username = "user",
					Password = "user123",
					IsAdmin = false,
				},
			});
		});
	}

	internal static Guid GetUserId()
	{
		throw new NotImplementedException();
	}
}
