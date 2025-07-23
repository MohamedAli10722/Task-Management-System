using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v68 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Notifications_NotiID",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_NotiID",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "NotiID",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "comments",
                table: "Evaluations");

            migrationBuilder.AddColumn<string>(
                name: "EvaluationsEvaluationID",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_EvaluationsEvaluationID",
                table: "Notifications",
                column: "EvaluationsEvaluationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Evaluations_EvaluationsEvaluationID",
                table: "Notifications",
                column: "EvaluationsEvaluationID",
                principalTable: "Evaluations",
                principalColumn: "EvaluationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Evaluations_EvaluationsEvaluationID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_EvaluationsEvaluationID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EvaluationsEvaluationID",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "NotiID",
                table: "Evaluations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comments",
                table: "Evaluations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_NotiID",
                table: "Evaluations",
                column: "NotiID",
                unique: true,
                filter: "[NotiID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Notifications_NotiID",
                table: "Evaluations",
                column: "NotiID",
                principalTable: "Notifications",
                principalColumn: "NotificationID");
        }
    }
}
