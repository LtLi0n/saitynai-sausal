using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saitynai.Backend.Migrations
{
    /// <inheritdoc />
    public partial class TagGroupCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "saitynai",
                table: "tag_groups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "saitynai",
                table: "tag_groups",
                keyColumn: "id",
                keyValue: new Guid("7e9a83c8-2bac-418b-af6b-8ddc2ec34ae7"),
                column: "created_at",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "saitynai",
                table: "tag_groups");
        }
    }
}
