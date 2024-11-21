using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saitynai.Backend.Contracts.Models;

[DbEntity]
[Table("tag_groups")]
[Index(nameof(Name), nameof(ParentId), nameof(OwnerId), IsUnique = true)]
public class TagGroup : IEntity, IOwnableEntity
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public Guid? ParentId { get; set; }
	public TagGroup Parent { get; set; }

	public List<Tag> Tags { get; set; }
	public List<TagGroup> ChildrenGroups { get; set; }

	public Guid? OwnerId { get; set; }
	public User Owner { get; set; }

	public static Guid Id1 { get; } = new Guid("7e9a83c8-2bac-418b-af6b-8ddc2ec34ae7");

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TagGroup>(x =>
		{
			x.HasMany(x => x.Tags)
			.WithOne(x => x.Group)
			.HasForeignKey(x => x.GroupId)
			.OnDelete(DeleteBehavior.Cascade);

			x.HasOne(x => x.Parent)
			.WithMany(x => x.ChildrenGroups)
			.HasForeignKey(x => x.ParentId)
			.OnDelete(DeleteBehavior.SetNull);

			x.HasData(new List<TagGroup>()
			{
				new TagGroup()
				{
					Id = Id1,
					OwnerId = User.UserId,
					Name = "Seeded tag group"
				}
			});
		});
	}
}
