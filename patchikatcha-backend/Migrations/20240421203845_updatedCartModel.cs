using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace patchikatcha_backend.Migrations
{
    /// <inheritdoc />
    public partial class updatedCartModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlueprintId",
                table: "Carts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrintProviderId",
                table: "Carts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlueprintId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "PrintProviderId",
                table: "Carts");
        }
    }
}
