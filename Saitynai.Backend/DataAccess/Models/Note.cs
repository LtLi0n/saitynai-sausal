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

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Note>(x =>
		{
			x
			.HasMany(x => x.Tags)
			.WithOne(x => x.Note)
			.HasForeignKey(x => x.NoteId)
			.OnDelete(DeleteBehavior.Cascade);
		});
	}
}
