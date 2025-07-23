using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v29 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_Developer_EmployeeId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_Test_EmployeeId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_UI_UXId",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_Developer_Employee_NationalNumber",
                table: "Tasks",
                column: "Developer_EmployeeId",
                principalTable: "Persons",
                principalColumn: "NationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_Test_Employee_NationalNumber",
                table: "Tasks",
                column: "Test_EmployeeId",
                principalTable: "Persons",
                principalColumn: "NationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_UI_UX_NationalNumber",
                table: "Tasks",
                column: "UI_UXId",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_Developer_Employee_NationalNumber",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_Test_Employee_NationalNumber",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Persons_UI_UX_NationalNumber",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_Developer_EmployeeId",
                table: "Tasks",
                column: "Developer_EmployeeId",
                principalTable: "Persons",
                principalColumn: "NationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_Test_EmployeeId",
                table: "Tasks",
                column: "Test_EmployeeId",
                principalTable: "Persons",
                principalColumn: "NationalNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Persons_UI_UXId",
                table: "Tasks",
                column: "UI_UXId",
                principalTable: "Persons",
                principalColumn: "NationalNumber");
        }
    }
}
