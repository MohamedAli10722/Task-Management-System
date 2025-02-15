using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class M4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    ProjectID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PersonId = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeadLine = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.ProjectID);
                    table.ForeignKey(
                        name: "FK_Project_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                });

            migrationBuilder.CreateTable(
                name: "Task",
                columns: table => new
                {
                    TaskID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProjectID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PersonId = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeadLine = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Task", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_Task_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_Task_Project_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Project",
                        principalColumn: "ProjectID");
                });

            migrationBuilder.CreateTable(
                name: "Evaluation",
                columns: table => new
                {
                    EvaluationID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProjectID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TaskID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PersonId = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluation", x => x.EvaluationID);
                    table.ForeignKey(
                        name: "FK_Evaluation_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_Evaluation_Project_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Project",
                        principalColumn: "ProjectID");
                    table.ForeignKey(
                        name: "FK_Evaluation_Task_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Task",
                        principalColumn: "TaskID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evaluation_PersonId",
                table: "Evaluation",
                column: "PersonId",
                unique: true,
                filter: "[PersonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluation_ProjectID",
                table: "Evaluation",
                column: "ProjectID",
                unique: true,
                filter: "[ProjectID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluation_TaskID",
                table: "Evaluation",
                column: "TaskID",
                unique: true,
                filter: "[TaskID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PersonId",
                table: "Project",
                column: "PersonId",
                unique: true,
                filter: "[PersonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Task_PersonId",
                table: "Task",
                column: "PersonId",
                unique: true,
                filter: "[PersonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Task_ProjectID",
                table: "Task",
                column: "ProjectID",
                unique: true,
                filter: "[ProjectID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evaluation");

            migrationBuilder.DropTable(
                name: "Task");

            migrationBuilder.DropTable(
                name: "Project");
        }
    }
}
