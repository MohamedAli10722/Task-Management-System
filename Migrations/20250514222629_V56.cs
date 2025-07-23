using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class V56 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskCheckListItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDone = table.Column<bool>(type: "bit", nullable: false),
                    TaskID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCheckListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCheckListItems_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskCheckListItems_TaskID",
                table: "TaskCheckListItems",
                column: "TaskID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskCheckListItems");
        }
    }
}
