using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saitynai.Backend.Contracts.Models;

[DbEntity]
[Table("tags")]
[Index(nameof(Name), nameof(GroupId), nameof(OwnerId), IsUnique = true)]
public class Tag : IEntity, IOwnableEntity
{
	public Guid Id { get; set; }

	public Guid GroupId { get; set; }
	public TagGroup Group { get; set; }

	public string Name { get; set; }
	
	public List<NoteTag> Contents { get; set; } = [];
	public List<TagEmbedding> Embeddings { get; set; } = [];

	public Guid? OwnerId { get; set; }
	public User Owner { get; set; }

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Tag>(x =>
		{
			x.HasMany(x => x.Contents)
			.WithOne(x => x.Tag)
			.HasForeignKey(x => x.Id)
			.OnDelete(DeleteBehavior.Cascade);

			x.HasMany(x => x.Embeddings)
			.WithOne(x => x.Tag)
			.HasForeignKey(x => x.TagId)
			.OnDelete(DeleteBehavior.SetNull);
		});
	}
}
