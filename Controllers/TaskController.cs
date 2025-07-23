using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Task = Area.Models.Task;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AreaContext _context;
        private readonly FirebaseService _firebaseService;

        public TaskController(AreaContext context, FirebaseService firebaseService)
        {
            _context = context;
            _firebaseService = firebaseService;
        }

        #region  GetTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskGetDto>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.ParentTask)
                .Include(t => t.ChecklistReviews)
                    .ThenInclude(r => r.Items)
                .Include(t => t.EvaluationTask)
                .Select(p => new TaskGetDto
                {
                    Title = p.Title,
                    Status = p.Status,
                    Discription = p.Discription,
                    CreationDate = p.CreationData,
                    DeadLine = p.DeadLine,
                    Attachment = p.AttachmentPath,
                    Team_Leader_Name = p.TeamLeader_Name,
                    UI_UX_Name = p.UI_UX_Name,
                    UITask = p.UploadUITask,
                    UINotes = p.UINotes,
                    Developer_Employee_Name = p.Developer_Employee_Name,
                    DeveloperTask = p.UploadDeveloperTask,
                    DeveloperNotes = p.DeveloperNotes,
                    Test_Employee_Name = p.Test_Employee_Name,
                    ProjectTitle = p.Project.Title,
                    ParentTaskTitle = p.ParentTask.Title,               
                    TotalScore = p.EvaluationTask != null ? p.EvaluationTask.Score : "Not Evaluated",

                    ChecklistItems = p.ChecklistReviews.Select(r => new ChecklistGetRevDTO
                    {
                        ReviewDate = r.ReviewDate,
                        Items = r.Items.Select(i => new CheckListGetDTO
                        {
                            Title = i.Title,
                            IsDone = i.IsDone,
                            Score = i.Score
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(tasks);
        }
        #endregion

        #region CreateTeamleadertask
        [Authorize]
        [HttpPost("LeaderTask")]
        public async Task<IActionResult> CreateNewTeamLeaderTask([FromForm] TeamLeaderTaskDTO createTaskDTO)
        {
            // Check Task
            if (createTaskDTO == null)
                return BadRequest("Invalid Task data");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Title == createTaskDTO.ProjectTitle);

            if (project == null)
                return NotFound("Project not found");

            if (createTaskDTO.DeadLine > project.DeadLine)
            {
                return BadRequest("Task deadline cannot be after the project deadline.");
            }

            var existingTask = await _context.Tasks
            .FirstOrDefaultAsync(p => p.Title == createTaskDTO.Title);

            if (existingTask != null)
            {
                return BadRequest($"A Task with the title '{createTaskDTO.Title}' already exists.");
            }

            var teamLeader = await _context.Persons
                .Where(p => p.UserName == createTaskDTO.Team_Leader_Name &&
                 EF.Property<string>(p, "Discriminator") == "Team_Leader")
                .FirstOrDefaultAsync();

            if (teamLeader == null)
                return BadRequest("Invalid TeamLeader Name. User not found.");

            var ProductOwnerUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(ProductOwnerUserName))
                return Unauthorized("Could not determine Product Owner username from token.");

            //Tasks Limit
            int maxTasksPerMonth = 5;
            var taskDeadline = createTaskDTO.DeadLine;
            var taskMonth = new DateTime(taskDeadline.Year, taskDeadline.Month, 1);
            var endOfTaskMonth = taskMonth.AddMonths(1).AddSeconds(-1);

            var tasksInSameMonth = await _context.Tasks
                .Where(t => t.Leader_Id == teamLeader.NationalNumber &&
                            t.DeadLine.Month == taskDeadline.Month &&
                            t.DeadLine.Year == taskDeadline.Year &&
                            t.Status != Enums.TaskStatus.Done)
                .ToListAsync(); 

            Console.WriteLine($"Team Leader: {teamLeader.UserName}, Checking tasks for month: {taskMonth.ToString("yyyy-MM")}");
            foreach (var t in tasksInSameMonth)
            {
                Console.WriteLine($"Found task: {t.Title}, Deadline: {t.DeadLine.ToString("yyyy-MM-dd")}");
            }

            int tasksThisMonth = tasksInSameMonth.Count;

            if (tasksThisMonth >= maxTasksPerMonth)
            {
                return BadRequest($"This teamleader already has {tasksThisMonth} Tasks in a month {taskMonth.ToString("yyyy-MM")}. The maximum is {maxTasksPerMonth} Tasks.");
            }
            int remainingTasks = maxTasksPerMonth - tasksThisMonth;

            // Image Path
            string filePath = null;
            if (createTaskDTO.Attachment != null && createTaskDTO.Attachment.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedFiles");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createTaskDTO.Attachment.FileName);
                var fullPath = Path.Combine(folderPath, fileName);
                var relativePath = Path.Combine("UploadedFiles", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await createTaskDTO.Attachment.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                filePath = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
            }

            var task = new Task
            {
                Title = createTaskDTO.Title,
                Status = (Enums.TaskStatus)createTaskDTO.Status,
                Discription = createTaskDTO.Discription,
                CreationData = createTaskDTO.CreationDate,
                DeadLine = createTaskDTO.DeadLine,
                AttachmentPath = filePath,
                TeamLeader_Name = createTaskDTO.Team_Leader_Name,
                Leader_Id = teamLeader.NationalNumber,
                Project_Id = project.ProjectID,
                Project_Title = project.Title,
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Create",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = ProductOwnerUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            // Notification
            var notificationTitle = $"New Task: {task.Title}";
            var notificationBody = $"You have been assigned a new Task by Product Owner: {ProductOwnerUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = createTaskDTO.Team_Leader_Name,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = ProductOwnerUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
           .Where(dt => dt.UserName == createTaskDTO.Team_Leader_Name)
           .Select(dt => dt.DeviceToken)
           .ToListAsync();

            foreach (var token in deviceToken)
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
                task.Title,
                task.Status,
                task.Discription,
                task.CreationData,
                task.DeadLine,
                task.TeamLeader_Name,
                task.AttachmentPath,
                RemainingTasks = remainingTasks-1, 
                TasksThisMonth = tasksThisMonth+1, 
                MaxTasksPerMonth = maxTasksPerMonth, 
                MonthChecked = taskMonth.ToString("yyyy-MM") 
            };
            return CreatedAtAction(nameof(CreateNewTeamLeaderTask), new { id = task.TaskID }, response);
        }
        #endregion

        #region CreateUITask
        [Authorize]
        [HttpPost("UITask")]
        public async Task<IActionResult> CreateNewUITask([FromForm] UI_EmployeeTaskDTO createUI_EmployeeTaskDTO)
        {
            if (createUI_EmployeeTaskDTO == null)
                return BadRequest("Invalid Task data");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Title == createUI_EmployeeTaskDTO.ProjectTitle);

            if (project == null)
                return NotFound("Project not found");

            if (createUI_EmployeeTaskDTO.DeadLine > project.DeadLine)
            {
                return BadRequest("Task deadline cannot be after the project deadline.");
            }

            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(p => p.Title == createUI_EmployeeTaskDTO.Title);

            if (existingTask != null)
            {
                return BadRequest($"A Task with the title '{createUI_EmployeeTaskDTO.Title}' already exists.");
            }

            var UiEmployee = await _context.Persons
                .Where(p => p.UserName == createUI_EmployeeTaskDTO.UI_EmployeeUserName &&
                    EF.Property<string>(p, "Discriminator").Trim() == "UI_Employee")
                .FirstOrDefaultAsync();

            if (UiEmployee == null)
                return BadRequest("Invalid UI_UX_Employee Name. User not found.");

            var ProductOwnerUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(ProductOwnerUserName))
                return Unauthorized("Could not determine Product Owner username from token.");

            int maxTasksPerMonth = 5;
            var taskDeadline = createUI_EmployeeTaskDTO.DeadLine;
            var taskMonth = new DateTime(taskDeadline.Year, taskDeadline.Month, 1);

            var tasksInSameMonth = await _context.Tasks
                .Where(t => t.UI_UXId == UiEmployee.NationalNumber &&
                            t.DeadLine.Month == taskDeadline.Month &&
                            t.DeadLine.Year == taskDeadline.Year &&
                            t.Status != Enums.TaskStatus.Done)
                .ToListAsync();

            int tasksThisMonth = tasksInSameMonth.Count;

            if (tasksThisMonth >= maxTasksPerMonth)
            {
                return BadRequest($"This UI_Employee already has {tasksThisMonth} tasks in month {taskMonth:yyyy-MM}. Max is {maxTasksPerMonth}.");
            }

            int remainingTasks = maxTasksPerMonth - tasksThisMonth;

            string filePath = null;
            if (createUI_EmployeeTaskDTO.Attachment != null && createUI_EmployeeTaskDTO.Attachment.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedFiles");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createUI_EmployeeTaskDTO.Attachment.FileName);
                var fullPath = Path.Combine(folderPath, fileName);
                var relativePath = Path.Combine("UploadedFiles", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await createUI_EmployeeTaskDTO.Attachment.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                filePath = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
            }

            var task = new Task
            {
                Title = createUI_EmployeeTaskDTO.Title,
                Status = (Enums.TaskStatus)createUI_EmployeeTaskDTO.Status,
                Discription = createUI_EmployeeTaskDTO.Discription,
                CreationData = createUI_EmployeeTaskDTO.CreationDate,
                DeadLine = createUI_EmployeeTaskDTO.DeadLine,
                AttachmentPath = filePath,
                UI_UX_Name = createUI_EmployeeTaskDTO.UI_EmployeeUserName,
                UI_UXId = UiEmployee.NationalNumber,
                Project_Id = project.ProjectID,
                Project_Title = project.Title,
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var checkListItems = await _context.CheckListItems
                .Where(c => c.Discriminator == "UI_Employee")
                .ToListAsync();

            var taskCheckListItems = checkListItems.Select(c => new TaskCheckListItem
            {
                Title = c.Title,
                IsDone = false,
                TaskID = task.TaskID,
                TaskTitle = task.Title
            }).ToList();

            _context.TaskCheckListItems.AddRange(taskCheckListItems);
            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Create",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = ProductOwnerUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            var notificationTitle = $"New Task: {task.Title}";
            var notificationBody = $"You have been assigned a new Task by Product Owner: {ProductOwnerUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = createUI_EmployeeTaskDTO.UI_EmployeeUserName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = ProductOwnerUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
                .Where(dt => dt.UserName == createUI_EmployeeTaskDTO.UI_EmployeeUserName)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceToken)
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
                task.Title,
                task.Status,
                task.Discription,
                task.CreationData,
                task.DeadLine,
                task.UI_UX_Name,
                task.AttachmentPath,
                task.Project_Title,
                RemainingTasks = remainingTasks - 1,
                TasksThisMonth = tasksThisMonth + 1,
                MaxTasksPerMonth = maxTasksPerMonth,
                MonthChecked = taskMonth.ToString("yyyy-MM"),

                //CheckListItems = taskCheckListItems.Select(i => new
                //{
                //    i.Title,
                //    i.IsDone,
                //    i.Id,
                //    i.TaskTitle
                //}).ToList()
            };

            return Ok(response);
        }
        #endregion

        #region CreateDeveloperTask
        [Authorize]
        [HttpPost("DeveloperTask")]
        public async Task<IActionResult> CreateNewDeveloperTask([FromForm] DeveloperTaskDTO createdeveloperTaskDTO)
        {
            if (createdeveloperTaskDTO == null)
                return BadRequest("Invalid Task data");

            var developer = await _context.Persons
                .Where(p => p.UserName == createdeveloperTaskDTO.DeveloperUserName &&
                            EF.Property<string>(p, "Discriminator").Trim() == "Developer_Employee")
                .FirstOrDefaultAsync();

            var existingTask = await _context.Tasks
            .FirstOrDefaultAsync(p => p.Title == createdeveloperTaskDTO.Title);

            if (existingTask != null)
            {
                return BadRequest($"A Task with the title '{createdeveloperTaskDTO.Title}' already exists.");
            }

            if (developer == null)
                return BadRequest("Invalid developer Name. User not found.");

            var TeamLeaderUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(TeamLeaderUserName))
                return Unauthorized("Could not determine Team Leader username from token.");

            Task parentTask = null;
            Project project = null;

            if (!string.IsNullOrEmpty(createdeveloperTaskDTO.ParentTaskTitle))
            {
                parentTask = await _context.Tasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Title == createdeveloperTaskDTO.ParentTaskTitle);

                if (parentTask == null)
                    return NotFound("Parent task not found");

                project = parentTask.Project;
            }
            else
            {
                return BadRequest("Parent task is required to assign the project automatically.");
            }

            if (createdeveloperTaskDTO.DeadLine > parentTask.DeadLine)
            {
                return BadRequest("The developer task deadline cannot be after the parent task's deadline.");
            }

            int maxTasksPerMonth = 5;
            var taskDeadline = createdeveloperTaskDTO.DeadLine;
            var taskMonth = new DateTime(taskDeadline.Year, taskDeadline.Month, 1);

            var tasksThisMonth = await _context.Tasks
                .Where(t => t.Developer_EmployeeId == developer.NationalNumber &&
                            t.DeadLine.Month == taskDeadline.Month &&
                            t.DeadLine.Year == taskDeadline.Year &&
                            t.Status != Enums.TaskStatus.Done)
                .CountAsync();

            if (tasksThisMonth >= maxTasksPerMonth)
            {
                return BadRequest($"This developer already has {tasksThisMonth} tasks in {taskMonth:yyyy-MM}. The maximum is {maxTasksPerMonth}.");
            }
            int remainingTasks = maxTasksPerMonth - tasksThisMonth;

            string filePath = null;
            if (createdeveloperTaskDTO.Attachment != null && createdeveloperTaskDTO.Attachment.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedFiles");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createdeveloperTaskDTO.Attachment.FileName);
                var fullPath = Path.Combine(folderPath, fileName);
                var relativePath = Path.Combine("UploadedFiles", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await createdeveloperTaskDTO.Attachment.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                filePath = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
            }

            var task = new Task
            {
                Title = createdeveloperTaskDTO.Title,
                Status = (Enums.TaskStatus)createdeveloperTaskDTO.Status,
                Discription = createdeveloperTaskDTO.Discription,
                CreationData = createdeveloperTaskDTO.CreationDate,
                DeadLine = createdeveloperTaskDTO.DeadLine,
                AttachmentPath = filePath,
                Developer_Employee_Name = createdeveloperTaskDTO.DeveloperUserName,
                Developer_EmployeeId = developer.NationalNumber,
                Project_Id = project.ProjectID,
                Project_Title = project.Title,
                ParentTaskId = parentTask.TaskID,
                ParentTaskTitle = parentTask.Title
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Create",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = TeamLeaderUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            // Notification
            var notificationTitle = $"New Task: {task.Title}";
            var notificationBody = $"You have been assigned a new Task by Team Leader: {TeamLeaderUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = createdeveloperTaskDTO.DeveloperUserName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = TeamLeaderUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
           .Where(dt => dt.UserName == createdeveloperTaskDTO.DeveloperUserName)
           .Select(dt => dt.DeviceToken)
           .ToListAsync();

            foreach (var token in deviceToken)
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
                task.Title,
                task.Status,
                task.Discription,
                task.CreationData,
                task.DeadLine,
                task.Developer_Employee_Name,
                task.AttachmentPath,
                task.ParentTaskTitle,
                task.Project_Title,
                RemainingTasks = remainingTasks - 1,
                TasksThisMonth = tasksThisMonth + 1,
                MaxTasksPerMonth = maxTasksPerMonth,
                MonthChecked = taskMonth.ToString("yyyy-MM")
            };
            return CreatedAtAction(nameof(CreateNewDeveloperTask), new { id = task.TaskID }, response);
        }
        #endregion

        #region CreateTestTask
        [Authorize]
        [HttpPost("TestTask")]
        public async Task<IActionResult> CreateNewTestTask([FromForm] TestTaskDTO createTestTaskDTO)
        {
            if (createTestTaskDTO == null)
                return BadRequest("Invalid Task data");

            var test = await _context.Persons
                .Where(p => p.UserName == createTestTaskDTO.TestUserName &&
                    EF.Property<string>(p, "Discriminator").Trim() == "Test_Employee")
                .FirstOrDefaultAsync();

            var existingTask = await _context.Tasks
            .FirstOrDefaultAsync(p => p.Title == createTestTaskDTO.Title);

            if (existingTask != null)
            {
                return BadRequest($"A Task with the title '{createTestTaskDTO.Title}' already exists.");
            }

            if (test == null)
                return BadRequest("Invalid Test_Employee Name. User not found.");

            var TeamLeaderUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(TeamLeaderUserName))
                return Unauthorized("Could not determine Team Leader username from token.");

            Task parentTask = null;
            Project project = null;

            if (!string.IsNullOrEmpty(createTestTaskDTO.ParentTaskTitle))
            {
                parentTask = await _context.Tasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Title == createTestTaskDTO.ParentTaskTitle);

                if (parentTask == null)
                    return NotFound("Parent task not found");

                project = parentTask.Project;
            }
            else
            {
                return BadRequest("Parent task is required to assign the project automatically.");
            }

            if (createTestTaskDTO.DeadLine > parentTask.DeadLine)
            {
                return BadRequest("The developer task deadline cannot be after the parent task's deadline.");
            }

            int maxTasksPerMonth = 5;
            var taskDeadline = createTestTaskDTO.DeadLine;
            var taskMonth = new DateTime(taskDeadline.Year, taskDeadline.Month, 1);

            var tasksThisMonth = await _context.Tasks
                .Where(t => t.Test_EmployeeId == test.NationalNumber &&
                            t.DeadLine.Month == taskDeadline.Month &&
                            t.DeadLine.Year == taskDeadline.Year &&
                            t.Status != Enums.TaskStatus.Done)
                .CountAsync();

            if (tasksThisMonth >= maxTasksPerMonth)
            {
                return BadRequest($"This tester already has {tasksThisMonth} tasks in {taskMonth:yyyy-MM}. The maximum is {maxTasksPerMonth}.");
            }
            int remainingTasks = maxTasksPerMonth - tasksThisMonth;

            string filePath = null;
            if (createTestTaskDTO.Attachment != null && createTestTaskDTO.Attachment.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedFiles");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createTestTaskDTO.Attachment.FileName);
                var fullPath = Path.Combine(folderPath, fileName);
                var relativePath = Path.Combine("UploadedFiles", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await createTestTaskDTO.Attachment.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                filePath = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
            }

            var task = new Task
            {
                Title = createTestTaskDTO.Title,
                Status = (Enums.TaskStatus)createTestTaskDTO.Status,
                Discription = createTestTaskDTO.Discription,
                DeadLine = createTestTaskDTO.DeadLine,
                CreationData = createTestTaskDTO.CreationDate,
                AttachmentPath = filePath,
                Test_Employee_Name = createTestTaskDTO.TestUserName,
                Test_EmployeeId = test.NationalNumber,
                Project_Id = project?.ProjectID,
                Project_Title = project?.Title,
                ParentTaskId = parentTask.TaskID,
                ParentTaskTitle = parentTask.Title
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Create",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = TeamLeaderUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            // Notification
            var notificationTitle = $"New Task: {task.Title}";
            var notificationBody = $"You have been assigned a new Task by Team Leader: {TeamLeaderUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = createTestTaskDTO.TestUserName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = TeamLeaderUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
           .Where(dt => dt.UserName == createTestTaskDTO.TestUserName)
           .Select(dt => dt.DeviceToken)
           .ToListAsync();

            foreach (var token in deviceToken)
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
                task.Title,
                task.Status,
                task.Discription,
                task.CreationData,
                task.DeadLine,
                task.Test_Employee_Name,
                task.AttachmentPath,
                task.ParentTaskTitle,
                task.Project_Title,
                RemainingTasks = remainingTasks - 1,
                TasksThisMonth = tasksThisMonth + 1,
                MaxTasksPerMonth = maxTasksPerMonth,
                MonthChecked = taskMonth.ToString("yyyy-MM")
            };
            return CreatedAtAction(nameof(CreateNewTestTask), new { id = task.TaskID }, response);
        }
        #endregion

        #region CreateTaskForDeveloperAndTester
        [HttpPost("DeveloperTest")]
        public async Task<IActionResult> CreateNewTask([FromForm] DeveloperAndTestDTO newTaskDTO)
        {
            if (newTaskDTO == null)
                return BadRequest("Invalid Task data");

            // الحصول على الـ Developer و الـ Tester
            var developer = await _context.Persons
                .Where(p => p.UserName == newTaskDTO.DeveloperUserName &&
                            EF.Property<string>(p, "Discriminator").Trim() == "Developer_Employee")
                .FirstOrDefaultAsync();

            var test = await _context.Persons
                .Where(p => p.UserName == newTaskDTO.TestUserName &&
                            EF.Property<string>(p, "Discriminator").Trim() == "Test_Employee")
                .FirstOrDefaultAsync();

            if (developer == null)
                return BadRequest("Developer not found.");

            if (test == null)
                return BadRequest("Test Employee not found.");

            // التحقق من وجود المهمة
            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(p => p.Title == newTaskDTO.Title);

            if (existingTask != null)
            {
                return BadRequest($"A Task with the title '{newTaskDTO.Title}' already exists.");
            }

            // التحقق من ParentTask و ربط المشروع
            Task parentTask = null;
            Project project = null;

            if (!string.IsNullOrEmpty(newTaskDTO.ParentTaskTitle))
            {
                parentTask = await _context.Tasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Title == newTaskDTO.ParentTaskTitle);

                if (parentTask == null)
                    return NotFound("Parent task not found");

                project = parentTask.Project;
            }
            else
            {
                return BadRequest("Parent task is required to assign the project automatically.");
            }

            if (newTaskDTO.DeadLine > parentTask.DeadLine)
            {
                return BadRequest("The developer task deadline cannot be after the parent task's deadline.");
            }

            // التحقق من عدد المهام الشهرية
            int maxTasksPerMonth = 5;
            var taskDeadline = newTaskDTO.DeadLine;
            var taskMonth = new DateTime(taskDeadline.Year, taskDeadline.Month, 1);

            var tasksThisMonthForDeveloper = await _context.Tasks
                .Where(t => t.Developer_EmployeeId == developer.NationalNumber &&
                            t.DeadLine.Month == taskDeadline.Month &&
                            t.DeadLine.Year == taskDeadline.Year &&
                            t.Status != Enums.TaskStatus.Done)
                .CountAsync();

            if (tasksThisMonthForDeveloper >= maxTasksPerMonth)
            {
                return BadRequest($"This developer already has {tasksThisMonthForDeveloper} tasks in {taskMonth:yyyy-MM}. The maximum is {maxTasksPerMonth}.");
            }

            int remainingTasksForDeveloper = maxTasksPerMonth - tasksThisMonthForDeveloper;

            string filePath = null;
            if (newTaskDTO.Attachment != null && newTaskDTO.Attachment.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedFiles");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newTaskDTO.Attachment.FileName);
                var fullPath = Path.Combine(folderPath, fileName);
                var relativePath = Path.Combine("UploadedFiles", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await newTaskDTO.Attachment.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                filePath = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
            }

            // إنشاء المهمة
            var task = new Task
            {
                Title = newTaskDTO.Title,
                Status = (Enums.TaskStatus)newTaskDTO.Status,
                Discription = newTaskDTO.Discription,
                CreationData = newTaskDTO.CreationDate,
                DeadLine = newTaskDTO.DeadLine,
                AttachmentPath = filePath,
                Developer_Employee_Name = newTaskDTO.DeveloperUserName,
                Developer_EmployeeId = developer.NationalNumber,
                Test_Employee_Name = newTaskDTO.TestUserName,
                Test_EmployeeId = test.NationalNumber,
                Project_Id = project.ProjectID,
                Project_Title = project.Title,
                ParentTaskId = parentTask.TaskID,
                ParentTaskTitle = parentTask.Title
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // إضافة CheckListItems تلقائيًا حسب وظيفة المطور
            if (developer != null)
            {
                string developerJob = developer.jobtitle?.Trim() ?? "";
                string discriminator = "Developer_Employee";

                // استخدام استعلام واحد بدلاً من عدة استعلامات متشابهة
                var checkListItems = await _context.CheckListItems
                    .Where(c => c.Discriminator == discriminator && c.job == developerJob)
                    .ToListAsync();

                if (checkListItems.Any())
                {
                    var taskCheckListItems = checkListItems.Select(c => new TaskCheckListItem
                    {
                        Title = c.Title,
                        IsDone = false,
                        TaskID = task.TaskID,
                        TaskTitle = task.Title
                    }).ToList();

                    _context.TaskCheckListItems.AddRange(taskCheckListItems);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // يمكن إضافة تسجيل للأخطاء هنا لمعرفة ما إذا كانت القائمة فارغة بشكل غير متوقع
                    Console.WriteLine($"No checklist items found for job: {developerJob} and discriminator: {discriminator}");
                }
            }

            // إشعار
            var TeamLeaderUserName = User.FindFirst("UserName")?.Value;

            var notificationTitle = $"New Task: {task.Title}";
            var notificationBody = $"You have been assigned a new Task by Team Leader {TeamLeaderUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = newTaskDTO.DeveloperUserName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = TeamLeaderUserName,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
                .Where(dt => dt.UserName == newTaskDTO.DeveloperUserName)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceToken)
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
                task.Title,
                task.Status,
                task.Discription,
                task.CreationData,
                task.DeadLine,
                task.Developer_Employee_Name,
                task.Developer_EmployeeId,
                task.Test_Employee_Name,
                task.Test_EmployeeId,
                task.AttachmentPath,
                task.ParentTaskTitle,
                task.Project_Title,
                RemainingTasksForDeveloper = remainingTasksForDeveloper - 1,
                TasksThisMonthForDeveloper = tasksThisMonthForDeveloper + 1,
                MaxTasksPerMonth = maxTasksPerMonth,
                MonthChecked = taskMonth.ToString("yyyy-MM")
            };

            return Ok(response);
        }
        #endregion

        #region UpdateTeamLeaderTask
        [Authorize]
        [HttpPut("LeaderTask")]
        public async Task<IActionResult> UpdateTeamLeaderTask([FromForm] UpdateTeamLeaderTaskDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
                return BadRequest("Task data is missing or title is empty.");

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Title == dto.Title);

            if (task == null)
                return NotFound("Task not found.");

            var TeamLeader = await _context.Persons
                .FirstOrDefaultAsync(t => t.UserName == dto.TeamLeaderName &&
                 EF.Property<string>(t, "Discriminator") == "Team_Leader");

            if (TeamLeader == null)
                return BadRequest("TeamLeader not found.");

            var ProductOwnerUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(ProductOwnerUserName))
                return Unauthorized("Could not determine Product Owner username from token.");

            //  Apply changes conditionally
            if (!string.IsNullOrWhiteSpace(dto.Discription))
                task.Discription = dto.Discription;

            if (!string.IsNullOrWhiteSpace(dto.Attachment))
                task.AttachmentPath = dto.Attachment;

            if (dto.CreationDate.HasValue && dto.CreationDate.Value != default)
                task.CreationData = dto.CreationDate.Value;

            if (dto.DeadLine.HasValue && dto.DeadLine.Value != default)
                task.DeadLine = dto.DeadLine.Value;

            if (!string.IsNullOrWhiteSpace(dto.TeamLeaderName))
            {
                task.TeamLeader_Name = dto.TeamLeaderName;
                task.Leader_Id = TeamLeader.NationalNumber;
            }

            await _context.SaveChangesAsync();

            //  Log the update
            var log = new ActivityLog
            {
                Action = "Update",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = ProductOwnerUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            //  Notify the team leader
            var notification = new Area.Models.Notification
            {
                Title = $"Task: {task.Title} updated",
                Body = $"The Task updated by Product Owner: {ProductOwnerUserName}",
                UserName = dto.TeamLeaderName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = ProductOwnerUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
                .Where(dt => dt.UserName == dto.TeamLeaderName)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceToken)
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(
                        token,
                        notification.Title,
                        notification.Body
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error for token {token}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Task updated successfully",
                updatedTask = new
                {
                    task.Title,
                    task.Discription,
                    task.Status,
                    task.CreationData,
                    task.DeadLine,
                    task.AttachmentPath,
                    TeamLeader = TeamLeader.UserName
                }
            });
        }
        #endregion

        #region UpdateUIEmployeeTask
        [Authorize]
        [HttpPut("UITask")]
        public async Task<IActionResult> UpdateUIEmployeeTask([FromForm] UpdateUIEmployeeTaskDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
                return BadRequest("Task data is missing or title is empty.");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == dto.Title);
            if (task == null)
                return NotFound("Task not found.");

            var UI_Employee = await _context.Persons
                .FirstOrDefaultAsync(u => u.UserName == dto.UiEmployeeName &&
                EF.Property<string>(u, "Discriminator") == "UI_Employee");

            if (UI_Employee == null)
                return BadRequest("UI_Employee not found.");

            var ProductOwnerUserName = User.FindFirst("UserName")?.Value;
            if (string.IsNullOrEmpty(ProductOwnerUserName))
                return Unauthorized("Could not determine Product Owner username from token.");

            //  Conditionally update only non-null/empty fields
            if (!string.IsNullOrWhiteSpace(dto.Discription))
                task.Discription = dto.Discription;

            if (!string.IsNullOrWhiteSpace(dto.Attachment))
                task.AttachmentPath = dto.Attachment;

            if (dto.CreationDate.HasValue && dto.CreationDate.Value != default)
                task.CreationData = dto.CreationDate.Value;

            if (dto.DeadLine.HasValue && dto.DeadLine.Value != default)
                task.DeadLine = dto.DeadLine.Value;

            if (!string.IsNullOrWhiteSpace(dto.UiEmployeeName))
            {
                task.UI_UX_Name = dto.UiEmployeeName;
                task.UI_UXId = UI_Employee.NationalNumber;
            }

            if (!string.IsNullOrWhiteSpace(dto.UINotes))
                task.UINotes = dto.UINotes;

            //  Handle uploads (new upload list or URL list)
            var uploadPaths = new List<string>();

            if (dto.UploadUITask != null && dto.UploadUITask.Any())
            {
                var UIUploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UIUploads");
                if (!Directory.Exists(UIUploadFolder))
                    Directory.CreateDirectory(UIUploadFolder);

                foreach (var file in dto.UploadUITask)
                {
                    if (file != null && file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var fullPath = Path.Combine(UIUploadFolder, fileName);
                        var relativePath = Path.Combine("UIUploads", fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var request = HttpContext.Request;
                        var baseUrl = $"{request.Scheme}://{request.Host}";
                        var fileUrl = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
                        uploadPaths.Add(fileUrl);
                    }
                }
            }

            if (dto.UploadUIUrls != null && dto.UploadUIUrls.Any())
            {
                uploadPaths.AddRange(dto.UploadUIUrls);
            }

            if (uploadPaths.Any())
                task.UploadDeveloperTask = string.Join(";", uploadPaths);

            await _context.SaveChangesAsync();

            //  Log
            var log = new ActivityLog
            {
                Action = "Update",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = ProductOwnerUserName,
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            //  Notification
            var notificationTitle = $"Task: {task.Title} updated";
            var notificationBody = $"The Task updated by Product Owner: {ProductOwnerUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = dto.UiEmployeeName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = ProductOwnerUserName
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
                .Where(dt => dt.UserName == dto.UiEmployeeName)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceToken)
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
                message = "Task updated successfully",
                updatedTask = new
                {
                    task.Title,
                    task.Discription,
                    task.Status,
                    task.CreationData,
                    task.DeadLine,
                    task.AttachmentPath,
                    task.UploadDeveloperTask,
                    task.UINotes,
                    UI_Employee = UI_Employee.UserName
                }
            });
        }
        #endregion

        #region UpdateDeveloTask
        [Authorize]
        [HttpPut("DeveloperTask")]
        public async Task<IActionResult> UpdateDeveloTask([FromForm] UpdateDeveloTaskDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
                return BadRequest("Task data is missing or title is empty.");

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Title == dto.Title);

            if (task == null)
                return NotFound("Task not found.");

            var DeveloperEmployee = await _context.Persons
                .FirstOrDefaultAsync(d => d.UserName == dto.DeveloperUserName &&
                 EF.Property<string>(d, "Discriminator") == "Developer_Employee");

            if (DeveloperEmployee == null)
                return BadRequest("Developer_Employee not found.");

            var TeamLeaderUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(TeamLeaderUserName))
                return Unauthorized("Could not determine Team Leader username from token.");

            task.Discription = dto.Discription;
            task.CreationData = dto.CreationDate;
            task.DeadLine = dto.DeadLine;
            task.AttachmentPath = dto.Attachment;
            task.Developer_Employee_Name = dto.DeveloperUserName;
            task.Developer_EmployeeId = DeveloperEmployee.NationalNumber;

            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Update",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = TeamLeaderUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            // Notification
            var notificationTitle = $"Task: {task.Title} updated";
            var notificationBody = $"The Task updated by Team Leader: {TeamLeaderUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = dto.DeveloperUserName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = TeamLeaderUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
           .Where(dt => dt.UserName == dto.DeveloperUserName)
           .Select(dt => dt.DeviceToken)
           .ToListAsync();

            foreach (var token in deviceToken)
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
                message = "Task updated successfully",
                updatedTask = new
                {
                    task.Title,
                    task.Discription,
                    task.Status,
                    task.CreationData,
                    task.DeadLine,
                    task.AttachmentPath,
                    Developer_Employee = DeveloperEmployee.UserName,
                }
            });
        }
        #endregion

        #region UpdateTestTask
        [Authorize]
        [HttpPut("TestTask")]
        public async Task<IActionResult> UpdateTestTask([FromForm] UpdateTestTaskDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
                return BadRequest("Task data is missing or title is empty.");

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Title == dto.Title);

            if (task == null)
                return NotFound("Task not found.");


            var TestEmployee = await _context.Persons
               .FirstOrDefaultAsync(ts => ts.UserName == dto.TestEmployeeName &&
                EF.Property<string>(ts, "Discriminator") == "Test_Employee");

            if (TestEmployee == null)
                return BadRequest("Test_Employee not found.");

            var TeamLeaderUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(TeamLeaderUserName))
                return Unauthorized("Could not determine Team Leader username from token.");

            task.Discription = dto.Discription;
            task.CreationData = dto.CreationDate;
            task.DeadLine = dto.DeadLine;
            task.AttachmentPath = dto.Attachment;
            task.Test_Employee_Name = dto.TestEmployeeName;
            task.Test_EmployeeId = TestEmployee.NationalNumber;

            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Update",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = TeamLeaderUserName,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            // Notification
            var notificationTitle = $"Task: {task.Title} updated";
            var notificationBody = $"The Task updated by Team Leader: {TeamLeaderUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = dto.TestEmployeeName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = TeamLeaderUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
           .Where(dt => dt.UserName == dto.TestEmployeeName)
           .Select(dt => dt.DeviceToken)
           .ToListAsync();

            foreach (var token in deviceToken)
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
                message = "Task updated successfully",
                updatedTask = new
                {
                    task.Title,
                    task.Discription,
                    task.Status,
                    task.CreationData,
                    task.DeadLine,
                    task.AttachmentPath,
                    Test_Employee = TestEmployee.UserName
                }
            });
        }
        #endregion

        #region UpdateTaskForDeveloperAndTester
        [Authorize]
        [HttpPut("DeveloperTest")]
        public async Task<IActionResult> UpdateTaskForDeveloperAndTester([FromForm] UpdateDeveloperAndTestDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
                return BadRequest("Task data is missing or title is empty.");

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Title == dto.Title);

            if (task == null)
                return NotFound("Task not found.");

            var TeamLeaderUserName = User.FindFirst("UserName")?.Value;

            if (!string.IsNullOrWhiteSpace(dto.DeveloperEmployeeName))
            {
                var developerEmployee = await _context.Persons
                    .FirstOrDefaultAsync(d => d.UserName == dto.DeveloperEmployeeName &&
                    EF.Property<string>(d, "Discriminator") == "Developer_Employee");

                if (developerEmployee == null)
                    return BadRequest("Developer_Employee not found.");

                task.Developer_Employee_Name = dto.DeveloperEmployeeName;
                task.Developer_EmployeeId = developerEmployee.NationalNumber;
            }

            if (!string.IsNullOrWhiteSpace(dto.TestEmployeeName))
            {
                var testEmployee = await _context.Persons
                    .FirstOrDefaultAsync(ts => ts.UserName == dto.TestEmployeeName &&
                    EF.Property<string>(ts, "Discriminator") == "Test_Employee");

                if (testEmployee == null)
                    return BadRequest("Test_Employee not found.");

                task.Test_Employee_Name = dto.TestEmployeeName;
                task.Test_EmployeeId = testEmployee.NationalNumber;
            }

            if (!string.IsNullOrWhiteSpace(dto.Discription))
                task.Discription = dto.Discription;

            if (!string.IsNullOrWhiteSpace(dto.DeveloperNotes))
                task.DeveloperNotes = dto.DeveloperNotes;

            if (dto.CreationDate.HasValue)
                task.CreationData = dto.CreationDate.Value;

            if (dto.DeadLine.HasValue)
                task.DeadLine = dto.DeadLine.Value;

            // Handle attachment upload
            if (dto.Attachment != null && dto.Attachment.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedFiles");
                Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Attachment.FileName);
                var fullPath = Path.Combine(folderPath, fileName);
                var relativePath = Path.Combine("UploadedFiles", fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                task.AttachmentPath = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
            }

            // Handle Developer Task uploads
            var uploadPaths = new List<string>();

            if (dto.UploadDeveloperTask != null && dto.UploadDeveloperTask.Any())
            {
                var devUploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DeveloperUploads");
                Directory.CreateDirectory(devUploadFolder);

                foreach (var file in dto.UploadDeveloperTask)
                {
                    if (file != null && file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var fullPath = Path.Combine(devUploadFolder, fileName);
                        var relativePath = Path.Combine("DeveloperUploads", fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var request = HttpContext.Request;
                        var baseUrl = $"{request.Scheme}://{request.Host}";
                        var fileUrl = $"{baseUrl}/{relativePath.Replace("\\", "/")}";
                        uploadPaths.Add(fileUrl);
                    }
                }
            }

            if (dto.UploadDeveloperUrls != null && dto.UploadDeveloperUrls.Any())
            {
                uploadPaths.AddRange(dto.UploadDeveloperUrls);
            }

            if (uploadPaths.Any())
                task.UploadDeveloperTask = string.Join(";", uploadPaths);

            await _context.SaveChangesAsync();

            // Create notification
            var notification = new Area.Models.Notification
            {
                Title = $"Task: {task.Title} updated",
                Body = "Some fields updated",
                UserName = task.Developer_Employee_Name,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = TeamLeaderUserName
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send push notifications
            var deviceTokens = await _context.DevicesToken
                .Where(dt => dt.UserName == task.Developer_Employee_Name || dt.UserName == task.Test_Employee_Name)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceTokens)
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(
                        token,
                        notification.Title,
                        notification.Body
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error for token {token}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Task updated successfully",
                updatedTask = new
                {
                    task.Title,
                    task.Discription,
                    task.Status,
                    task.CreationData,
                    task.DeadLine,
                    task.AttachmentPath,
                    task.UploadDeveloperTask,
                    task.DeveloperNotes,
                    Developer_Employee = new
                    {
                        task.Developer_Employee_Name,
                        task.Developer_EmployeeId
                    },
                    Test_Employee = new
                    {
                        task.Test_Employee_Name,
                        task.Test_EmployeeId
                    }
                }
            });
        }
        #endregion

        #region DeleteTask
        [Authorize]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteTask([FromBody] string title)
        {
            if (string.IsNullOrEmpty(title))
                return BadRequest("Task title is required.");

            var task = await _context.Tasks
               .Include(t => t.EvaluationTask)
               .Include(t => t.ChildTasks)
               .ThenInclude(ct => ct.NotificationsTask)
               .Include(t => t.NotificationsTask)
               .FirstOrDefaultAsync(t => t.Title == title);


            if (task == null)
                return NotFound("Task not found.");

            var SenderUserName = User.FindFirst("UserName")?.Value;

            if (string.IsNullOrEmpty(SenderUserName))
                return Unauthorized("Could not determine Employee username from token.");

            //Notification
            string employeeUsername = task.TeamLeader_Name
            ?? task.UI_UX_Name
            ?? task.Developer_Employee_Name
            ?? task.Test_Employee_Name
            ?? "Unknown";

            var notificationTitle = $"Task {task.Title} Deleted";
            var notificationBody = $"The Task deleted by: {SenderUserName}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = employeeUsername,
                TaskTitle = task.Title,
                sender = SenderUserName
            };

            var deviceTokens = await _context.DevicesToken
                .Where(dt => dt.UserName == employeeUsername)
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

            _context.Tasks.Remove(task); 

            await _context.SaveChangesAsync();

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Action = "Delete",
                EntityName = "Task",
                EntityTitle = task.Title,
                Time = DateTime.UtcNow,
                Username = employeeUsername,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok($"Task '{title}' and its child tasks have been deleted successfully.");
        }
        #endregion
    }
}