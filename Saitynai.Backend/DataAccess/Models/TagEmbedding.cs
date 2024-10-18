using Microsoft.EntityFrameworkCore;
using Saitynai.Backend.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saitynai.Backend.Contracts.Models;

[DbEntity]
[Table("tag_embeddings")]
public class TagEmbedding : IEntity
{
	public Guid Id { get; set; }

	public Guid TagId { get; set; }
	public Tag Tag { get; set; }

	public Guid EmbeddingId { get; set; }
	public Embedding Embedding { get; set; }

	public static void CreateModel(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TagEmbedding>();
	}
}
