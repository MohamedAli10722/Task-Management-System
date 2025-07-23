using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_ProductOwner_Id",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ProductOwner_Id",
                table: "Tasks",
                newName: "Owner_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_ProductOwner_Id",
                table: "Tasks",
                newName: "IX_Tasks_Owner_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_Owner_Id",
                table: "Tasks",
                column: "Owner_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_Owner_Id",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Owner_Id",
                table: "Tasks",
                newName: "ProductOwner_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_Owner_Id",
                table: "Tasks",
                newName: "IX_Tasks_ProductOwner_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_ProductOwner_Id",
                table: "Tasks",
                column: "ProductOwner_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }
    }
}
