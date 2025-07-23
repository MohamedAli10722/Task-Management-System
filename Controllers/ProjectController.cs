using Area.DTOs;
using Area.Enums;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

using Microsoft.AspNetCore.Authorization;



namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly AreaContext _context;
        private readonly FirebaseService _firebaseService;

        public ProjectController(AreaContext context, FirebaseService firebaseService)
        {
            _context = context;
            _firebaseService = firebaseService;
        }

        #region GetProjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.EvaluationProject) 
                .Select(p => new ProjectDTO
                {
                    Title = p.Title,
                    Status = p.Status,
                    Discription = p.Discription,
                    CreationDate = p.CreationData,
                    DeadLine = p.DeadLine,
                    ProductOwnerName = p.Product_Name,
                    ProjectEvaluation = p.EvaluationProject != null ? p.EvaluationProject.Score : "N/A"
                })
                .ToListAsync();

            return Ok(projects);
        }
        #endregion

        #region CreateNewProject
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateNewProject([FromBody] ProjectDTO createProjectDTO)
        {
            if (createProjectDTO == null)
                return BadRequest("Invalid project data");

            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.Title == createProjectDTO.Title);

            if (existingProject != null)
                return BadRequest($"A project with the title '{createProjectDTO.Title}' already exists.");

            var productOwner = await _context.Persons
                .Where(p => p.UserName == createProjectDTO.ProductOwnerName &&
                            EF.Property<string>(p, "Discriminator") == "Product_Owner")
                .FirstOrDefaultAsync();

            if (productOwner == null)
                return BadRequest("Invalid ProductOwner Name. User not found.");

            var managerUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(managerUserName))
                return Unauthorized("Could not determine manager username from token.");

            // Limit logic
            int maxProjectsPerMonth = 5;
            var projectDeadline = createProjectDTO.DeadLine;
            var monthStart = new DateTime(projectDeadline.Year, projectDeadline.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

            var projectsInSameMonth = await _context.Projects
                .Where(p => p.Product_Id == productOwner.NationalNumber &&
                            p.DeadLine >= monthStart &&
                            p.DeadLine <= monthEnd)
                .ToListAsync();

            int projectsThisMonth = projectsInSameMonth.Count;

            if (projectsThisMonth >= maxProjectsPerMonth)
            {
                return BadRequest($"This Product Owner already has {projectsThisMonth} projects in {monthStart:yyyy-MM}. The maximum allowed is {maxProjectsPerMonth}.");
            }

            var project = new Project
            {
                Title = createProjectDTO.Title,
                Status = createProjectDTO.Status,
                Discription = createProjectDTO.Discription,
                CreationData = createProjectDTO.CreationDate,
                DeadLine = createProjectDTO.DeadLine,
                Product_Name = createProjectDTO.ProductOwnerName,
                Product_Id = productOwner.NationalNumber
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Create",
                EntityName = "Project",
                EntityTitle = project.Title,
                Time = DateTime.UtcNow,
                Username = managerUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            // Notification
            var notificationTitle = $"New Project: {project.Title}";
            var notificationBody = $"You have been assigned a new project by manager: {managerUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = createProjectDTO.ProductOwnerName,
                ProjectID = project.ProjectID,
                ProjectTitle = project.Title,
                CreatedAt = DateTime.UtcNow,
                sender = managerUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send FCM notification to all devices
            var deviceTokens = await _context.DevicesToken
                .Where(dt => dt.UserName == createProjectDTO.ProductOwnerName)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceTokens)
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(
                        token,
                        notificationTitle,
                        notificationBody
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error for token {token}: {ex.Message}");
                }
            }

            var response = new
            {
                project.Title,
                project.Status,
                project.Discription,
                project.CreationData,
                project.DeadLine,
                project.Product_Name,
            };

            return CreatedAtAction(nameof(CreateNewProject), new { id = project.ProjectID }, response);
        }
        #endregion

        #region UpdateProject
        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateProject([FromBody] UpdateProjectDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
                return BadRequest("Project data is missing or title is empty.");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Title == dto.Title);

            if (project == null)
                return NotFound("Project not found.");

            var productOwner = await _context.Persons
                .FirstOrDefaultAsync(p => p.UserName == dto.ProductOwnerName &&
                     EF.Property<string>(p, "Discriminator") == "Product_Owner");

            if (productOwner == null)
                return BadRequest("Product Owner not found.");

            var managerUserName = User.FindFirst("UserName")?.Value;
            if (string.IsNullOrEmpty(managerUserName))
                return Unauthorized("Could not determine manager username from token.");

            //  Update only provided values
            if (!string.IsNullOrWhiteSpace(dto.Discription))
                project.Discription = dto.Discription;

            if (dto.CreationDate.HasValue && dto.CreationDate.Value != default)
                project.CreationData = dto.CreationDate.Value;

            if (dto.DeadLine.HasValue && dto.DeadLine.Value != default)
                project.DeadLine = dto.DeadLine.Value;

            if (!string.IsNullOrWhiteSpace(dto.ProductOwnerName))
            {
                project.Product_Name = dto.ProductOwnerName;
                project.Product_Id = productOwner.NationalNumber;
            }

            await _context.SaveChangesAsync();

            //  Log
            var log = new ActivityLog
            {
                Action = "Update",
                EntityName = "Project",
                EntityTitle = project.Title,
                Time = DateTime.UtcNow,
                Username = managerUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            //  Notification
            var notificationTitle = $"Project {project.Title} Updated";
            var notificationBody = $"The project was updated by manager: {managerUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = dto.ProductOwnerName,
                ProjectID = project.ProjectID,
                ProjectTitle = project.Title,
                sender = managerUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceTokens = await _context.DevicesToken
                .Where(dt => dt.UserName == dto.ProductOwnerName)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceTokens)
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(
                        token,
                        notificationTitle,
                        notificationBody
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error for token {token}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Project updated successfully",
                updatedProject = new
                {
                    project.Title,
                    project.Discription,
                    project.CreationData,
                    project.DeadLine,
                    ProductOwner = productOwner.UserName
                }
            });
        }

        #endregion

        #region DeleteProject
        [Authorize]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteProject([FromBody] string title)
        {
            if (string.IsNullOrEmpty(title))
                return BadRequest("Project title is required.");

            var project = await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.ChildTasks)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.EvaluationTask)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.NotificationsTask)
                .FirstOrDefaultAsync(p => p.Title == title);

            if (project == null)
                return NotFound("Project not found");

            var managerUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(managerUserName))
                return Unauthorized("Could not determine manager username from token.");

            var notifications = await _context.Notifications
                .Where(n => n.ProjectID == project.ProjectID)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);

            //Notification
            var notificationTitle = $"Project {project.Title} Deleted";
            var notificationBody = $"The project deleted by manager: {managerUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = project.Product_Name,
                ProjectID = project.ProjectID,
                ProjectTitle = project.Title,
                sender = managerUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceTokens = await _context.DevicesToken
                .Where(dt => dt.UserName == project.Product_Name)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceTokens)
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(
                        token,
                        notificationTitle,
                        notificationBody
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error for token {token}: {ex.Message}");
                }
            }

            foreach (var task in project.Tasks)
            {
                if (task.ChildTasks != null && task.ChildTasks.Any())
                {
                    foreach (var child in task.ChildTasks)
                    {
                        if (child.NotificationsTask != null && child.NotificationsTask.Any())
                            _context.Notifications.RemoveRange(child.NotificationsTask);
                    }
                    _context.Tasks.RemoveRange(task.ChildTasks);
                }

                if (task.NotificationsTask != null && task.NotificationsTask.Any())
                    _context.Notifications.RemoveRange(task.NotificationsTask);
            }

            _context.Tasks.RemoveRange(project.Tasks);

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Delete",
                EntityName = "Project",
                EntityTitle = project.Title,
                Time = DateTime.UtcNow,
                Username = managerUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project deleted successfully" });
        }
        #endregion
    }
}