using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Area.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Permission_id = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Permission_name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Permission_id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Role_id = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Role_name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Role_id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    NationalNumber = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Roleid = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    job = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.NationalNumber);
                    table.ForeignKey(
                        name: "FK_Persons_Roles_Roleid",
                        column: x => x.Roleid,
                        principalTable: "Roles",
                        principalColumn: "Role_id");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Permission_id = table.Column<string>(type: "nvarchar(14)", nullable: false),
                    Role_id = table.Column<string>(type: "nvarchar(14)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.Role_id, x.Permission_id });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_Permission_id",
                        column: x => x.Permission_id,
                        principalTable: "Permissions",
                        principalColumn: "Permission_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_Role_id",
                        column: x => x.Role_id,
                        principalTable: "Roles",
                        principalColumn: "Role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Graduation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    University = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalNumberId = table.Column<string>(type: "nvarchar(14)", nullable: true)
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
                name: "Projects",
                columns: table => new
                {
                    ProjectID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeadLine = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionProjectDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedProjectOnTime = table.Column<bool>(type: "bit", nullable: false),
                    EvaluationScore = table.Column<double>(type: "float", nullable: false),
                    Manager_Id = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Product_Id = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    EvaluatioProjectid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectID);
                    table.ForeignKey(
                        name: "FK_Projects_Persons_Manager_Id",
                        column: x => x.Manager_Id,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_Projects_Persons_Product_Id",
                        column: x => x.Product_Id,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                });

            migrationBuilder.CreateTable(
                name: "WorkIn",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalNumberId = table.Column<string>(type: "nvarchar(14)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    TaskID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeadLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmissionTaskOnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductOwner_Id = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    TeamLeader_Id = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Project_Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedTo_Id = table.Column<string>(type: "nvarchar(14)", nullable: true),
                    Evaluatiotasknid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_Tasks_Persons_AssignedTo_Id",
                        column: x => x.AssignedTo_Id,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_Tasks_Persons_ProductOwner_Id",
                        column: x => x.ProductOwner_Id,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_Tasks_Persons_TeamLeader_Id",
                        column: x => x.TeamLeader_Id,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber");
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_Project_Id",
                        column: x => x.Project_Id,
                        principalTable: "Projects",
                        principalColumn: "ProjectID");
                });

            migrationBuilder.CreateTable(
                name: "BugsReports",
                columns: table => new
                {
                    Bug_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bug_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaskBugId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TestEmployee_id = table.Column<string>(type: "nvarchar(14)", nullable: true)
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
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ProjectID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TaskID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Evaluatioid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID");
                    table.ForeignKey(
                        name: "FK_Notifications_Tasks_TaskID",
                        column: x => x.TaskID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID");
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    EvaluationID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BugsFixed = table.Column<int>(type: "int", nullable: false),
                    SubmittedOnTime = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaskEvaluationID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProjectEvaluationID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NotiID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.EvaluationID);
                    table.ForeignKey(
                        name: "FK_Evaluations_Notifications_NotiID",
                        column: x => x.NotiID,
                        principalTable: "Notifications",
                        principalColumn: "NotificationID");
                    table.ForeignKey(
                        name: "FK_Evaluations_Projects_ProjectEvaluationID",
                        column: x => x.ProjectEvaluationID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID");
                    table.ForeignKey(
                        name: "FK_Evaluations_Tasks_TaskEvaluationID",
                        column: x => x.TaskEvaluationID,
                        principalTable: "Tasks",
                        principalColumn: "TaskID");
                });

            migrationBuilder.CreateTable(
                name: "PersonNotifications",
                columns: table => new
                {
                    person_ID = table.Column<string>(type: "nvarchar(14)", nullable: false),
                    notifi_ID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonNotifications", x => new { x.person_ID, x.notifi_ID });
                    table.ForeignKey(
                        name: "FK_PersonNotifications_Notifications_notifi_ID",
                        column: x => x.notifi_ID,
                        principalTable: "Notifications",
                        principalColumn: "NotificationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonNotifications_Persons_person_ID",
                        column: x => x.person_ID,
                        principalTable: "Persons",
                        principalColumn: "NationalNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Permission_id", "Permission_name" },
                values: new object[,]
                {
                    { "1", "Create Project" },
                    { "10", "View Reports" },
                    { "11", "Test Tasks" },
                    { "2", "Edit Project" },
                    { "3", "Delete Project" },
                    { "4", "Create Task" },
                    { "5", "Edit Task" },
                    { "6", "Delete Task" },
                    { "7", "Add Member" },
                    { "8", "View Projects" },
                    { "9", "View Tasks" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Role_id", "Role_name" },
                values: new object[,]
                {
                    { "1", "Manager" },
                    { "2", "Product_Owner" },
                    { "3", "Team_Leader" },
                    { "4", "UI_Employee" },
                    { "5", "Developer_Employee" },
                    { "6", "Test_Employee" }
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "NationalNumber", "DateOfBirth", "Discriminator", "Email", "FirstName", "LastName", "Location", "MobileNumber", "Nationality", "Password", "Roleid", "UserName" },
                values: new object[] { "12345678901234", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Manager", "ali.ahmed@gmail.com", "Ali", "Ahmed", null, "01012345678", null, null, "1", "Ali_manager" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Permission_id", "Role_id" },
                values: new object[,]
                {
                    { "1", "1" },
                    { "10", "1" },
                    { "2", "1" },
                    { "3", "1" },
                    { "7", "1" },
                    { "8", "1" },
                    { "4", "2" },
                    { "5", "2" },
                    { "4", "3" },
                    { "9", "4" },
                    { "9", "5" },
                    { "11", "6" }
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
                name: "IX_Evaluations_NotiID",
                table: "Evaluations",
                column: "NotiID",
                unique: true,
                filter: "[NotiID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ProjectEvaluationID",
                table: "Evaluations",
                column: "ProjectEvaluationID",
                unique: true,
                filter: "[ProjectEvaluationID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_TaskEvaluationID",
                table: "Evaluations",
                column: "TaskEvaluationID",
                unique: true,
                filter: "[TaskEvaluationID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Graduation_NationalNumberId",
                table: "Graduation",
                column: "NationalNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ProjectID",
                table: "Notifications",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TaskID",
                table: "Notifications",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_PersonNotifications_notifi_ID",
                table: "PersonNotifications",
                column: "notifi_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Email",
                table: "Persons",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Roleid",
                table: "Persons",
                column: "Roleid");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_UserName",
                table: "Persons",
                column: "UserName",
                unique: true,
                filter: "[UserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Manager_Id",
                table: "Projects",
                column: "Manager_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Product_Id",
                table: "Projects",
                column: "Product_Id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_Permission_id",
                table: "RolePermissions",
                column: "Permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedTo_Id",
                table: "Tasks",
                column: "AssignedTo_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProductOwner_Id",
                table: "Tasks",
                column: "ProductOwner_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Project_Id",
                table: "Tasks",
                column: "Project_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TeamLeader_Id",
                table: "Tasks",
                column: "TeamLeader_Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkIn_NationalNumberId",
                table: "WorkIn",
                column: "NationalNumberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BugsReports");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "Graduation");

            migrationBuilder.DropTable(
                name: "LoginHistory");

            migrationBuilder.DropTable(
                name: "PersonNotifications");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "WorkIn");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
