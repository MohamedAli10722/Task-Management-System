using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJobColumnFromCheckListItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "job",
                table: "CheckListItems",
                type: "nvarchar(max)",
                nullable: true); // اتأكد إنك حاطط نفس النوع والمواصفات اللي كانت عليه
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "job",
                table: "CheckListItems"); // ده لو عايز تشيل العمود تاني لو رجعت الـ Migration
        }


    }
}
