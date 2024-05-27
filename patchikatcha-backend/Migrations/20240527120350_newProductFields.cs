using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace patchikatcha_backend.Migrations
{
    /// <inheritdoc />
    public partial class newProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "Products",
                newName: "Tag");

            migrationBuilder.AddColumn<string>(
                name: "CategoryTag",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondImage",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryTag",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SecondImage",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "Products",
                newName: "Tags");
        }
    }
}
