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
		modelBuilder.Entity<NoteTag>();
	}
}
