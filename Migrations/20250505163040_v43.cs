using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v43 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DevicesToken",
                table: "DevicesToken");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "DevicesToken",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "DevicesToken",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevicesToken",
                table: "DevicesToken",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DevicesToken",
                table: "DevicesToken");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DevicesToken");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "DevicesToken",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevicesToken",
                table: "DevicesToken",
                column: "UserName");
        }
    }
}
