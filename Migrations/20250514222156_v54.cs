using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v54 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // حذف العمود اللي اتشال بالخطأ
            migrationBuilder.AddColumn<string>(
                name: "job",
                table: "CheckListItems",
                type: "nvarchar(max)",
                nullable: true); // إضافة عمود job مرة تانية

            // حذف العمود IsDone
            migrationBuilder.DropColumn(
                name: "IsDone",
                table: "CheckListItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // لو رجعت التعديلات دي وعايز ترجع كل حاجة زي ما كانت
            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                table: "CheckListItems",
                type: "bit",
                nullable: false,
                defaultValue: false); // إرجاع العمود IsDone

            // لو رجعت العمود job زي ما كان
            migrationBuilder.DropColumn(
                name: "job",
                table: "CheckListItems");
        }

    }
}
