﻿using Microsoft.EntityFrameworkCore;
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

	public static Guid Id1 { get; } = new("51532d76-1926-4adb-9173-85485876ea42");
	public static Guid Id2 { get; } = new("331226db-d78d-4dd4-aaab-fb9c2e92d84a");

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Tag>(x =>
		{
			x.HasMany(x => x.Contents)
			.WithOne(x => x.Tag)
			.HasForeignKey(x => x.TagId)
			.OnDelete(DeleteBehavior.Cascade);

			x.HasMany(x => x.Embeddings)
			.WithOne(x => x.Tag)
			.HasForeignKey(x => x.TagId)
			.OnDelete(DeleteBehavior.SetNull);

			x.HasData(new List<Tag>()
			{
				new()
				{
					Id = Id1,
					GroupId = TagGroup.Id1,
					Name = "Seeded tag 1",
					OwnerId = User.UserId
				},
				new()
				{
					Id = Id2,
					GroupId = TagGroup.Id1,
					Name = "Seeded tag 2",
					OwnerId = User.UserId
				},
			});
		});
	}
}
