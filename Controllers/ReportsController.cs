using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Area.Enums;
using System.Threading.Tasks;
using Area.ReportService;
using Area.Dtos;
using System.Security.Claims;
using Area.DTOs;



namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AreaContext _context;
        private readonly Area.ReportService.ReportService _reportService;

        public ReportsController(AreaContext context, Area.ReportService.ReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        #region All Activity
        [HttpGet("Activity")]
        public IActionResult GetActivityReportPdf()
        {
            var reportData = new List<EmployeeActivityDTO>();

            // Get all login sessions
            var loginSessions = _context.LoginHistory
                .OrderByDescending(l => l.LoginTime)
                .ToList();

            // Group sessions by user email
            var usersEmails = loginSessions.Select(l => l.Email.ToLower()).Distinct();

            foreach (var email in usersEmails)
            {
                var person = _context.Persons.FirstOrDefault(p => p.Email.ToLower() == email);
                if (person == null) continue; // skip if no matching person found

                var userName = person.UserName;

                // Get login sessions for this user
                var sessions = loginSessions.Where(l => l.Email.ToLower() == email).ToList();

                // Get activity logs for this userName
                var activityLogs = _context.ActivityLogs
                    .Where(a => a.Username.ToLower() == userName.ToLower())
                    .ToList();

                var sessionDTOs = new List<LoginSessionWithActivitiesDTO>();

                foreach (var session in sessions)
                {
                    var login = session.LoginTime;
                    var logout = session.LogoutTime == default(DateTime) ? (DateTime?)null : session.LogoutTime;

                    var activities = activityLogs
                        .Where(a => a.Time >= login && (!logout.HasValue || a.Time <= logout.Value))
                        .Select(a => new ActivityEntryDTO
                        {
                            Action = a.Action,
                            EntityName = a.EntityName,
                            EntityTitle = a.EntityTitle,
                            Time = a.Time
                        }).ToList();

                    sessionDTOs.Add(new LoginSessionWithActivitiesDTO
                    {
                        LoginTime = login,
                        LogoutTime = logout,
                        Activities = activities
                    });
                }

                reportData.Add(new EmployeeActivityDTO
                {
                    UserName = userName,
                    Sessions = sessionDTOs
                });
            }

            var userNameFromToken = User.Identity?.Name ?? "Unknown";
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Unknown";

            var metadata = new ReportMetadataDTO
            {
                ProgramName = "Task Area",
                LogoPath = "wwwroot/images/logo.png",
                UserName = userNameFromToken,
                UserRole = userRole,
                CreatedAt = DateTime.UtcNow
            };

            var pdfBytes = _reportService.GenerateActivityReport(reportData, metadata);
            return File(pdfBytes, "application/pdf", "SessionActivityReport.pdf");
        }
        #endregion

        #region One User Activity
        [HttpPost("Activity/User")]
        public IActionResult GetActivityReportForUser([FromBody] string userName)
        {
            var reportData = new List<EmployeeActivityDTO>();

            var person = _context.Persons.FirstOrDefault(p => p.UserName.ToLower() == userName.ToLower());
            if (person == null)
                return NotFound("User not found.");

            var email = person.Email.ToLower();

            // Get login sessions for this user
            var sessions = _context.LoginHistory
                .Where(l => l.Email.ToLower() == email)
                .OrderByDescending(l => l.LoginTime)
                .ToList(); 

            // Get activity logs for this user
            var activityLogs = _context.ActivityLogs
                .Where(a => a.Username.ToLower() == userName.ToLower())
                .ToList();

            var sessionDTOs = new List<LoginSessionWithActivitiesDTO>();

            foreach (var session in sessions)
            {
                var login = session.LoginTime;
                var logout = session.LogoutTime == default(DateTime) ? (DateTime?)null : session.LogoutTime;

                var activities = activityLogs
                    .Where(a => a.Time >= login && (!logout.HasValue || a.Time <= logout.Value))
                    .Select(a => new ActivityEntryDTO
                    {
                        Action = a.Action,
                        EntityName = a.EntityName,
                        EntityTitle = a.EntityTitle,
                        Time = a.Time
                    }).ToList();

                sessionDTOs.Add(new LoginSessionWithActivitiesDTO
                {
                    LoginTime = login,
                    LogoutTime = logout,
                    Activities = activities
                });
            }

            reportData.Add(new EmployeeActivityDTO
            {
                UserName = userName,
                Sessions = sessionDTOs
            });

            var userNameFromToken = User.Identity?.Name ?? "Unknown";
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Unknown";

            var metadata = new ReportMetadataDTO
            {
                ProgramName = "Task Area",
                LogoPath = "wwwroot/images/logo.png",
                UserName = userNameFromToken,
                UserRole = userRole,
                CreatedAt = DateTime.UtcNow
            };

            var pdfBytes = _reportService.GenerateActivityReport(reportData, metadata);
            return File(pdfBytes, "application/pdf", $"{userName}-ActivityReport.pdf");
        }
        #endregion

        #region Employees With Roles and Distribution
        [HttpGet("EmployeesRoles")]
        public IActionResult GetEmployeeReportPdf()
        {
            var totalEmployees = _context.Persons.Count();

            var reportData = _context.Roles
                .Include(r => r.Persons)
                .Include(r => r.RolePermission).ThenInclude(rp => rp.Permission)
                .Select(role => new ReportDTO
                {
                    RoleName = role.Role_name,
                    Percentage = totalEmployees == 0 ? 0 : Math.Round((double)role.Persons.Count() * 100 / totalEmployees, 2),
                    Permissions = role.RolePermission.Select(rp => rp.Permission.Permission_name).ToList(),
                    Employees = role.Persons.Select(p => p.UserName).ToList()
                })
                .ToList();

            var userName = User.FindFirst("UserName")?.Value;
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Unknown";

            var metadata = new ReportMetadataDTO
            {
                ProgramName = "Task Area",
                LogoPath = "wwwroot/images/logo.png",
                UserName = userName,
                UserRole = userRole,
                CreatedAt = DateTime.UtcNow
            };

            var pdfBytes = _reportService.GenerateRolesReport(reportData, metadata, totalEmployees);
            return File(pdfBytes, "application/pdf", "EmployeeRoleReport.pdf");
        }
        #endregion

        #region Reports only Queries not as pdf
        [HttpGet("api/roles-distribution")]
        public IActionResult GetRoleDistribution()
        {
            var totalEmployees = _context.Persons.Count();

            var roleDistribution = _context.Roles
                .Select(role => new
                {
                    RoleName = role.Role_name,
                    EmployeeCount = role.Persons.Count(),
                    Percentage = (double)role.Persons.Count() * 100 / totalEmployees
                })
                .OrderByDescending(r => r.Percentage)
                .ToList();

            return Ok(roleDistribution);
        }

        [HttpGet("api/projects-status")]
        public IActionResult GetProjectsStatus()
        {
            var projects = _context.Projects
                .Select(p => new
                {
                    ProjectTitle = p.Title,
                    Status = p.Status.ToString()
                })
                .ToList();

            return Ok(projects);
        }

        [HttpGet("project-completion-rates")]
        public IActionResult GetProjectCompletionRates()
        {
            var projectCompletionRates = _context.Projects
               .Select(p => new
               {
                   ProjectTitle = p.Title,
                   TotalTasks = p.Tasks.Count(),
                   CompletedTasks = p.Tasks.Count(t => t.Status == Enums.TaskStatus.Done),
                   CompletionRate = p.Tasks.Count() == 0
                   ? 0
                  : Math.Round((double)p.Tasks.Count(t => t.Status == Enums.TaskStatus.Done) / p.Tasks.Count() * 100, 2)
               })
               .ToList();


            return Ok(projectCompletionRates);
        }

        [HttpGet("api/inactive-employees")]
        public IActionResult GetInactiveEmployees()
        {
            DateTime fromDate = new DateTime(2024, 1, 1);
            DateTime toDate = new DateTime(2024, 12, 31);

            var inactiveEmployees = _context.Persons
                .Where(p =>
                    !_context.Tasks.Any(t =>
                        ((t.Leader_Id == p.NationalNumber) ||
                         (t.UI_UXId == p.NationalNumber) ||
                         (t.Developer_EmployeeId == p.NationalNumber) ||
                         (t.Test_EmployeeId == p.NationalNumber)) &&
                        t.CreationData >= fromDate &&
                        t.DeadLine <= toDate))
                .Select(p => new
                {
                    p.UserName
                })
                .ToList();

            return Ok(inactiveEmployees);
        }

        [HttpGet("TaskDeliveryReport")]
        public IActionResult GetTaskDeliveryReport()
        {
            var result = _context.Projects
                .Select(project => new
                {
                    ProjectTitle = project.Title,
                    TotalCompletedTasks = project.Tasks.Count(t => t.Status == Enums.TaskStatus.Done),
                    OnTimeTasks = project.Tasks.Count(t => t.Status == Enums.TaskStatus.Done && t.SubmissionTaskOnDate <= t.DeadLine),
                    DeliveryRate = project.Tasks.Count(t => t.Status == Enums.TaskStatus.Done) == 0
                        ? 0
                        : Math.Round((double)project.Tasks.Count(t => t.Status == Enums.TaskStatus.Done && t.SubmissionTaskOnDate <= t.DeadLine)
                            / project.Tasks.Count(t => t.Status == Enums.TaskStatus.Done) * 100, 2)
                })
                .ToList();

            return Ok(result);
        }
        #endregion
    }
}


