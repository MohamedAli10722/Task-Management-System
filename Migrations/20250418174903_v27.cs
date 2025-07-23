using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Developer_EmployeeId",
                table: "Tasks",
                type: "nvarchar(14)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Test_EmployeeId",
                table: "Tasks",
                type: "nvarchar(14)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UI_UXId",
                table: "Tasks",
                type: "nvarchar(14)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Developer_EmployeeId",
                table: "Tasks",
                column: "Developer_EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Test_EmployeeId",
                table: "Tasks",
                column: "Test_EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UI_UXId",
                table: "Tasks",
                column: "UI_UXId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Developer_EmployeeId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Test_EmployeeId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UI_UXId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Developer_EmployeeId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Test_EmployeeId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UI_UXId",
                table: "Tasks");
        }
    }
}
