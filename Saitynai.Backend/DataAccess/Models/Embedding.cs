using Microsoft.EntityFrameworkCore;
using Pgvector;
using Saitynai.Backend.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saitynai.Backend.Contracts.Models;

[DbEntity]
[Table("embeddings")]
public class Embedding : IEntity
{
	public Guid Id { get; set; }

	[Column(TypeName = "vector")]
	public Vector? Value { get; set; }

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Embedding>();
	}
}