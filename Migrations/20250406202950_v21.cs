using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_Owner_Id",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Owner_Id",
                table: "Tasks",
                newName: "Product_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_Owner_Id",
                table: "Tasks",
                newName: "IX_Tasks_Product_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_Product_Id",
                table: "Tasks",
                column: "Product_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_Product_Id",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Product_Id",
                table: "Tasks",
                newName: "Owner_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_Product_Id",
                table: "Tasks",
                newName: "IX_Tasks_Owner_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_Owner_Id",
                table: "Tasks",
                column: "Owner_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }
    }
}
