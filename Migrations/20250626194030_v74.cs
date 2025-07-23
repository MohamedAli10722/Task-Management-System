using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v74 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Projects_ProjectEvaluationID",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Tasks_TaskEvaluationID",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_ProjectEvaluationID",
                table: "Evaluations");

            migrationBuilder.AlterColumn<string>(
                name: "EvaluatioProjectid",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectEvaluationID",
                table: "Evaluations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EvaluatioProjectid",
                table: "Projects",
                column: "EvaluatioProjectid",
                unique: true,
                filter: "[EvaluatioProjectid] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Tasks_TaskEvaluationID",
                table: "Evaluations",
                column: "TaskEvaluationID",
                principalTable: "Tasks",
                principalColumn: "TaskID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Evaluations_EvaluatioProjectid",
                table: "Projects",
                column: "EvaluatioProjectid",
                principalTable: "Evaluations",
                principalColumn: "EvaluationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Tasks_TaskEvaluationID",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Evaluations_EvaluatioProjectid",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_EvaluatioProjectid",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "EvaluatioProjectid",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectEvaluationID",
                table: "Evaluations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ProjectEvaluationID",
                table: "Evaluations",
                column: "ProjectEvaluationID",
                unique: true,
                filter: "[ProjectEvaluationID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Projects_ProjectEvaluationID",
                table: "Evaluations",
                column: "ProjectEvaluationID",
                principalTable: "Projects",
                principalColumn: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Tasks_TaskEvaluationID",
                table: "Evaluations",
                column: "TaskEvaluationID",
                principalTable: "Tasks",
                principalColumn: "TaskID");
        }
    }
}
