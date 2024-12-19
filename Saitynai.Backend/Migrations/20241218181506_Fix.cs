using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saitynai.Backend.Migrations
{
    /// <inheritdoc />
    public partial class Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "saitynai",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5"),
                column: "is_admin",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "saitynai",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("b07f84e9-8074-4e0e-ae18-644bd9d45ee5"),
                column: "is_admin",
                value: true);
        }
    }
}
