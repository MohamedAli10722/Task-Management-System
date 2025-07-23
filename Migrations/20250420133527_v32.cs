using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v32 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tasks_TaskID",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tasks_TaskID",
                table: "Notifications",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "TaskID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tasks_TaskID",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tasks_TaskID",
                table: "Notifications",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "TaskID");
        }
    }
}
