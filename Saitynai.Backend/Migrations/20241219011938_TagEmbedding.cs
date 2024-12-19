using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Saitynai.Backend.Migrations
{
    /// <inheritdoc />
    public partial class TagEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tag_embeddings",
                schema: "saitynai");

            migrationBuilder.AddColumn<Vector>(
                name: "embedding",
                schema: "saitynai",
                table: "tags",
                type: "vector",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "embedding",
                schema: "saitynai",
                table: "tags");

            migrationBuilder.CreateTable(
                name: "tag_embeddings",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_embeddings", x => x.id);
                    table.ForeignKey(
                        name: "fk_tag_embeddings_embeddings_embedding_id",
                        column: x => x.embedding_id,
                        principalSchema: "saitynai",
                        principalTable: "embeddings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tag_embeddings_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "saitynai",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tag_embeddings_embedding_id",
                schema: "saitynai",
                table: "tag_embeddings",
                column: "embedding_id");

            migrationBuilder.CreateIndex(
                name: "ix_tag_embeddings_tag_id",
                schema: "saitynai",
                table: "tag_embeddings",
                column: "tag_id");
        }
    }
}
