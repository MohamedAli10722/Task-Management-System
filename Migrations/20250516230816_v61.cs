using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v61 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BugScore",
                table: "Evaluations",
                newName: "CheckListScore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CheckListScore",
                table: "Evaluations",
                newName: "BugScore");
        }
    }
}
