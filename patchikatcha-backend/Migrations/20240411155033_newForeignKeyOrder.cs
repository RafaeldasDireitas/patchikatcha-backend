using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace patchikatcha_backend.Migrations
{
    /// <inheritdoc />
    public partial class newForeignKeyOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "Orders",
                newName: "ApplicationUserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApplicationUserEmail",
                table: "Orders",
                column: "ApplicationUserEmail",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ApplicationUserEmail",
                table: "Orders",
                column: "ApplicationUserEmail",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ApplicationUserEmail",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ApplicationUserEmail",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserEmail",
                table: "Orders",
                newName: "UserEmail");
        }
    }
}
