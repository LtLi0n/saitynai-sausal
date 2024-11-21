using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Saitynai.Backend.Migrations
{
    /// <inheritdoc />
    public partial class Seed2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_note_tags_tags_id",
                schema: "saitynai",
                table: "note_tags");

            migrationBuilder.InsertData(
                schema: "saitynai",
                table: "note_tags",
                columns: new[] { "id", "note_id", "tag_id" },
                values: new object[,]
                {
                    { new Guid("61724840-f4f5-4286-9b73-d56dd059b1dc"), new Guid("b0c5301d-4a02-427d-bb10-2a23b281d2fc"), new Guid("331226db-d78d-4dd4-aaab-fb9c2e92d84a") },
                    { new Guid("9acad4bd-aa3e-42e5-8496-c18b1ba2b2c9"), new Guid("b0c5301d-4a02-427d-bb10-2a23b281d2fc"), new Guid("51532d76-1926-4adb-9173-85485876ea42") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_note_tags_tag_id",
                schema: "saitynai",
                table: "note_tags",
                column: "tag_id");

            migrationBuilder.AddForeignKey(
                name: "fk_note_tags_tags_tag_id",
                schema: "saitynai",
                table: "note_tags",
                column: "tag_id",
                principalSchema: "saitynai",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_note_tags_tags_tag_id",
                schema: "saitynai",
                table: "note_tags");

            migrationBuilder.DropIndex(
                name: "ix_note_tags_tag_id",
                schema: "saitynai",
                table: "note_tags");

            migrationBuilder.DeleteData(
                schema: "saitynai",
                table: "note_tags",
                keyColumn: "id",
                keyValue: new Guid("61724840-f4f5-4286-9b73-d56dd059b1dc"));

            migrationBuilder.DeleteData(
                schema: "saitynai",
                table: "note_tags",
                keyColumn: "id",
                keyValue: new Guid("9acad4bd-aa3e-42e5-8496-c18b1ba2b2c9"));

            migrationBuilder.AddForeignKey(
                name: "fk_note_tags_tags_id",
                schema: "saitynai",
                table: "note_tags",
                column: "id",
                principalSchema: "saitynai",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
