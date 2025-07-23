using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Persons_Manager_Id",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Persons_Product_Id",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "Product_Id",
                table: "Projects",
                newName: "ProductOwnerNationalNumber");

            migrationBuilder.RenameColumn(
                name: "Manager_Id",
                table: "Projects",
                newName: "ManagerNationalNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_Product_Id",
                table: "Projects",
                newName: "IX_Projects_ProductOwnerNationalNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_Manager_Id",
                table: "Projects",
                newName: "IX_Projects_ManagerNationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Persons_ManagerNationalNumber",
                table: "Projects",
                column: "ManagerNationalNumber",
                principalTable: "Persons",
                principalColumn: "NationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Persons_ProductOwnerNationalNumber",
                table: "Projects",
                column: "ProductOwnerNationalNumber",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Persons_ManagerNationalNumber",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Persons_ProductOwnerNationalNumber",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ProductOwnerNationalNumber",
                table: "Projects",
                newName: "Product_Id");

            migrationBuilder.RenameColumn(
                name: "ManagerNationalNumber",
                table: "Projects",
                newName: "Manager_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ProductOwnerNationalNumber",
                table: "Projects",
                newName: "IX_Projects_Product_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ManagerNationalNumber",
                table: "Projects",
                newName: "IX_Projects_Manager_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Persons_Manager_Id",
                table: "Projects",
                column: "Manager_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Persons_Product_Id",
                table: "Projects",
                column: "Product_Id",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }
    }
}
