using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v72 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BugsReports");

            migrationBuilder.DropTable(
                name: "Graduation");

            migrationBuilder.DropTable(
                name: "WorkIn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BugsReports",
                columns: table => new
                {
                    Bug_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskBugId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TestEmployee_id = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Bug_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BugsReports", x => x.Bug_Id);
                    table.ForeignKey(
                        name: "FK_BugsReports_Persons_TestEmployee_id",
                        column: x => x.TestEmployee_id,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_BugsReports_Tasks_TaskBugId",
                        column: x => x.TaskBugId,
                        principalTable: "Tasks",
                        principalColumn: "TaskID");
                });

            migrationBuilder.CreateTable(
                name: "Graduation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalNumberId = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    University = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Graduation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Graduation_Persons_NationalNumberId",
                        column: x => x.NationalNumberId,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                });

            migrationBuilder.CreateTable(
                name: "WorkIn",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalNumberId = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    WorkPlace = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkIn", x => x.id);
                    table.ForeignKey(
                        name: "FK_WorkIn_Persons_NationalNumberId",
                        column: x => x.NationalNumberId,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BugsReports_TaskBugId",
                table: "BugsReports",
                column: "TaskBugId");

            migrationBuilder.CreateIndex(
                name: "IX_BugsReports_TestEmployee_id",
                table: "BugsReports",
                column: "TestEmployee_id");

            migrationBuilder.CreateIndex(
                name: "IX_Graduation_NationalNumberId",
                table: "Graduation",
                column: "NationalNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkIn_NationalNumberId",
                table: "WorkIn",
                column: "NationalNumberId");
        }
    }
}
