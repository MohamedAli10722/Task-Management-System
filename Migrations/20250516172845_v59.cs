using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v59 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChecklistReviewId",
                table: "TaskCheckListItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChecklistReview",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaskID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistReview_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskCheckListItems_ChecklistReviewId",
                table: "TaskCheckListItems",
                column: "ChecklistReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistReview_TaskID",
                table: "ChecklistReview",
                column: "TaskID");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCheckListItems_ChecklistReview_ChecklistReviewId",
                table: "TaskCheckListItems",
                column: "ChecklistReviewId",
                principalTable: "ChecklistReview",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCheckListItems_ChecklistReview_ChecklistReviewId",
                table: "TaskCheckListItems");

            migrationBuilder.DropTable(
                name: "ChecklistReview");

            migrationBuilder.DropIndex(
                name: "IX_TaskCheckListItems_ChecklistReviewId",
                table: "TaskCheckListItems");

            migrationBuilder.DropColumn(
                name: "ChecklistReviewId",
                table: "TaskCheckListItems");
        }
    }
}
