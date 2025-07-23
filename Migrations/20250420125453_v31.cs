using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v31 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_Project_Id",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_Project_Id",
                table: "Tasks",
                column: "Project_Id",
                principalTable: "Projects",
                principalColumn: "ProjectID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_Project_Id",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_Project_Id",
                table: "Tasks",
                column: "Project_Id",
                principalTable: "Projects",
                principalColumn: "ProjectID");
        }
    }
}
