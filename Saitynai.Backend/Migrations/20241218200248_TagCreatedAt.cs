using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Saitynai.Backend.Migrations
{
    /// <inheritdoc />
    public partial class TagCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "saitynai",
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("331226db-d78d-4dd4-aaab-fb9c2e92d84a"));

            migrationBuilder.DeleteData(
                schema: "saitynai",
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("51532d76-1926-4adb-9173-85485876ea42"));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "saitynai",
                table: "tags",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "saitynai",
                table: "tags");

            migrationBuilder.InsertData(
                schema: "saitynai",
                table: "tags",
                columns: new[] { "id", "group_id", "name", "owner_id" },
                values: new object[,]
                {
                    { new Guid("331226db-d78d-4dd4-aaab-fb9c2e92d84a"), new Guid("7e9a83c8-2bac-418b-af6b-8ddc2ec34ae7"), "Seeded tag 2", new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5") },
                    { new Guid("51532d76-1926-4adb-9173-85485876ea42"), new Guid("7e9a83c8-2bac-418b-af6b-8ddc2ec34ae7"), "Seeded tag 1", new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5") }
                });
        }
    }
}
