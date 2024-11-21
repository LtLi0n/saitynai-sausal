using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saitynai.Backend.Contracts.Models;

[DbEntity]
[Table("note_tags")]
public class NoteTag : IEntity
{
	public Guid Id { get; set; }

	public Guid NoteId { get; set; }
	public Note Note { get; set; }

	public Guid TagId { get; set; }
	public Tag Tag { get; set; }

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<NoteTag>(x =>
		{
			x.HasData(new List<NoteTag>()
			{
				new NoteTag()
				{
					Id = new("9acad4bd-aa3e-42e5-8496-c18b1ba2b2c9"),
					NoteId = Note.Id1,
					TagId = Tag.Id1,
				},
				new NoteTag()
				{
					Id = new("61724840-f4f5-4286-9b73-d56dd059b1dc"),
					NoteId = Note.Id1,
					TagId = Tag.Id2,
				},
			});
		});
	}
}
