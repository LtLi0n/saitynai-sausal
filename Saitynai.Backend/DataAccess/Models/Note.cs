using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saitynai.Backend.Contracts.Models;

[DbEntity]
[Table("notes")]
public class Note : IEntity, IOwnableEntity
{
	public Guid Id { get; set; }

	public Guid? OwnerId { get; set; }
	public User Owner { get; set; }

	public Guid? EmbeddingId { get; set; }
	public Embedding Embedding { get; set; }

	public string Content { get; set; }

	public List<NoteTag> Tags { get; set; } = [];

	public static Guid Id1 { get; } = new("b0c5301d-4a02-427d-bb10-2a23b281d2fc");

	public DateTime CreatedAt { get; set; }

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Note>(x =>
		{
			x
			.HasMany(x => x.Tags)
			.WithOne(x => x.Note)
			.HasForeignKey(x => x.NoteId)
			.OnDelete(DeleteBehavior.Cascade);

			x.HasData(new List<Note>()
			{
				new()
				{
					Id = Id1,
					OwnerId = User.UserId,
					Content = "Seeded note."
				}
			});
		});
	}
}
