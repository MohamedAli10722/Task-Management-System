using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v69 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Evaluations_EvaluationsEvaluationID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_EvaluationsEvaluationID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Evaluatioid",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EvaluationsEvaluationID",
                table: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Evaluatioid",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

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
    }
}
