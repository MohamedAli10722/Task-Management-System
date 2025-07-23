using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v50 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BugsFixed",
                table: "Evaluations");

            migrationBuilder.AddColumn<int>(
                name: "InprogressCount",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstTimeInTesting",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InprogressCount",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsFirstTimeInTesting",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "BugsFixed",
                table: "Evaluations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
