using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace patchikatcha_backend.Migrations
{
    /// <inheritdoc />
    public partial class addedproductimagetoreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "Reviews");
        }
    }
}
