using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_AssignedTo_Id",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_ProductOwnerNationalNumber",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_AssignedTo_Id",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AssignedTo_Id",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ProductOwnerNationalNumber",
                table: "Tasks",
                newName: "ProductOwner_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_ProductOwnerNationalNumber",
                table: "Tasks",
                newName: "IX_Tasks_ProductOwner_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_ProductOwner_Id",
                table: "Tasks",
                column: "ProductOwner_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_ProductOwner_Id",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ProductOwner_Id",
                table: "Tasks",
                newName: "ProductOwnerNationalNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_ProductOwner_Id",
                table: "Tasks",
                newName: "IX_Tasks_ProductOwnerNationalNumber");

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo_Id",
                table: "Tasks",
                type: "nvarchar(14)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedTo_Id",
                table: "Tasks",
                column: "AssignedTo_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_AssignedTo_Id",
                table: "Tasks",
                column: "AssignedTo_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_ProductOwnerNationalNumber",
                table: "Tasks",
                column: "ProductOwnerNationalNumber",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }
    }
}
