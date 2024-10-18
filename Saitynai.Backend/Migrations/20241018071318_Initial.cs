using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Saitynai.Backend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "saitynai");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "embeddings",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<Vector>(type: "vector", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_embeddings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    embedding_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_notes_embeddings_embedding_id",
                        column: x => x.embedding_id,
                        principalSchema: "saitynai",
                        principalTable: "embeddings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notes_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "saitynai",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tag_groups",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_tag_groups_tag_groups_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "saitynai",
                        principalTable: "tag_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_tag_groups_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "saitynai",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_tags_tag_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "saitynai",
                        principalTable: "tag_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tags_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "saitynai",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "note_tags",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_tags_notes_note_id",
                        column: x => x.note_id,
                        principalSchema: "saitynai",
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_note_tags_tags_id",
                        column: x => x.id,
                        principalSchema: "saitynai",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tag_embeddings",
                schema: "saitynai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding_id = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.InsertData(
                schema: "saitynai",
                table: "users",
                columns: new[] { "id", "email", "is_admin", "password", "username" },
                values: new object[,]
                {
                    { new Guid("85891986-ba23-499e-8c02-59bee76a574e"), "adminas.adminavicius@email.com", true, "admin123", "admin" },
                    { new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5"), "user.useris@email.com", true, "user123", "user" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_note_tags_note_id",
                schema: "saitynai",
                table: "note_tags",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_embedding_id",
                schema: "saitynai",
                table: "notes",
                column: "embedding_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_owner_id",
                schema: "saitynai",
                table: "notes",
                column: "owner_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_name_parent_id_owner_id",
                schema: "saitynai",
                table: "tag_groups",
                columns: new[] { "name", "parent_id", "owner_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_owner_id",
                schema: "saitynai",
                table: "tag_groups",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_parent_id",
                schema: "saitynai",
                table: "tag_groups",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_group_id",
                schema: "saitynai",
                table: "tags",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name_group_id_owner_id",
                schema: "saitynai",
                table: "tags",
                columns: new[] { "name", "group_id", "owner_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_owner_id",
                schema: "saitynai",
                table: "tags",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "saitynai",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                schema: "saitynai",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "note_tags",
                schema: "saitynai");

            migrationBuilder.DropTable(
                name: "tag_embeddings",
                schema: "saitynai");

            migrationBuilder.DropTable(
                name: "notes",
                schema: "saitynai");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "saitynai");

            migrationBuilder.DropTable(
                name: "embeddings",
                schema: "saitynai");

            migrationBuilder.DropTable(
                name: "tag_groups",
                schema: "saitynai");

            migrationBuilder.DropTable(
                name: "users",
                schema: "saitynai");
        }
    }
}
