using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "NationalNumber",
                keyValue: "12345678901234",
                columns: new[] { "Location", "Nationality", "Password", "UserName" },
                values: new object[] { "Cairo", "Egyptian", "AliAhmed123", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "NationalNumber",
                keyValue: "12345678901234",
                columns: new[] { "Location", "Nationality", "Password", "UserName" },
                values: new object[] { null, null, null, "Ali_manager" });
        }
    }
}
