using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class _66 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KPIS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPIS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPISEvaluation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    ManagerId = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    EvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalScore = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPISEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPISEvaluation_Persons_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_KPISEvaluation_Persons_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                });

            migrationBuilder.CreateTable(
                name: "KPISelection",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KPIId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    EvaluationId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPISelection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPISelection_KPISEvaluation_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "KPISEvaluation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KPISelection_KPIS_KPIId",
                        column: x => x.KPIId,
                        principalTable: "KPIS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KPISelection_EvaluationId",
                table: "KPISelection",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_KPISelection_KPIId",
                table: "KPISelection",
                column: "KPIId");

            migrationBuilder.CreateIndex(
                name: "IX_KPISEvaluation_EmployeeId",
                table: "KPISEvaluation",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_KPISEvaluation_ManagerId",
                table: "KPISEvaluation",
                column: "ManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KPISelection");

            migrationBuilder.DropTable(
                name: "KPISEvaluation");

            migrationBuilder.DropTable(
                name: "KPIS");
        }
    }
}
