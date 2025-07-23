using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskStatusController : ControllerBase
    {
        private readonly AreaContext _context;
        private readonly FirebaseService _firebaseService;

        public TaskStatusController(AreaContext context, FirebaseService firebaseService)
        {
            _context = context;
            _firebaseService = firebaseService;
        }

        #region Update State
        [HttpPut]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Task title is required.");

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Title == model.Title);

            if (task == null)
                return NotFound("Task not found.");


            task.Status = (Enums.TaskStatus)model.NewStatus;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Task '{task.Title}' status updated to '{task.Status}'.",
                taskTitle = task.Title,
                newStatus = task.Status
            });
        }
        #endregion

        #region Leader State
        [HttpPut("Leader")]
        public async Task<IActionResult> UpdateLeaderTaskStatus([FromBody] UpdateTaskStatusDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Task title is required.");

            var task = await _context.Tasks
                .Include(t => t.ChildTasks)
                .FirstOrDefaultAsync(t => t.Title == model.Title);

            if (task == null)
                return NotFound("Task not found.");

            if ((Enums.TaskStatus)model.NewStatus == Enums.TaskStatus.Done && task.ChildTasks?.Any() == true)
            {
                var unfinishedSubTasks = task.ChildTasks
                    .Where(st => st.Status != Enums.TaskStatus.Done)
                    .ToList();

                if (unfinishedSubTasks.Any())
                {
                    return BadRequest("Cannot mark parent task as Done while there are unfinished subtasks.");
                }
            }

            task.Status = (Enums.TaskStatus)model.NewStatus;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Task '{task.Title}' status updated to '{task.Status}'.",
                taskTitle = task.Title,
                newStatus = task.Status
            });
        }
        #endregion

        #region InprogressToTesting
        [HttpPut("InprogressToTesting")]
        public async Task<IActionResult> UpdateTaskStatusFromInProgressToTesting([FromBody] UpdateTaskStatusDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Task title is required.");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == model.Title);
            if (task == null)
                return NotFound("Task not found.");

            var oldStatus = task.Status;
            task.Status = Enums.TaskStatus.Test;

            var isFirstTime = !task.IsFirstTimeInTesting;
            if (isFirstTime)
                task.IsFirstTimeInTesting = true;

            await _context.SaveChangesAsync();

            var parentTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == task.ParentTaskTitle);
            string teamLeaderName = parentTask?.TeamLeader_Name;

            var testUserName = task.Test_Employee_Name;

            var notificationTitle = isFirstTime
                ? $"New Task Ready for Test: {task.Title}"
                : $"Task '{task.Title}' fixed and back for testing.";

            var notificationBody = isFirstTime
                ? $"Task is completed by Developer and ready for testing."
                : $"Developer fixed task '{task.Title}' and sent it back to testing.";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = testUserName,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = task.Developer_Employee_Name 
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceTokens = await _context.DevicesToken
                .Where(dt => dt.UserName == testUserName)
                .Select(dt => dt.DeviceToken)
                .ToListAsync();

            foreach (var token in deviceTokens)
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(token, notificationTitle, notificationBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error for token {token}: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(teamLeaderName))
            {
                var tlTitle = isFirstTime
                    ? $"Task Completed by Developer"
                    : $"Task '{task.Title}' fixed and back for testing.";

                var tlBody = isFirstTime
                    ? $"Developer finished task '{task.Title}' and it's ready for testing."
                    : $"Developer has fixed task '{task.Title}' and it's ready for testing again.";

                var tlNotification = new Area.Models.Notification
                {
                    Title = tlTitle,
                    Body = tlBody,
                    UserName = teamLeaderName,
                    TaskID = task.TaskID,
                    TaskTitle = task.Title,
                    sender = testUserName
                };

                _context.Notifications.Add(tlNotification);
                await _context.SaveChangesAsync();

                var tlTokens = await _context.DevicesToken
                    .Where(dt => dt.UserName == teamLeaderName)
                    .Select(dt => dt.DeviceToken)
                    .ToListAsync();

                foreach (var token in tlTokens)
                {
                    try
                    {
                        await _firebaseService.SendNotificationAsync(token, tlTitle, tlBody);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Notification error for TL token {token}: {ex.Message}");
                    }
                }
            }

            return Ok(new { message = $"Task '{task.Title}' status updated to '{task.Status}'." });
        }
        #endregion

        #region TestingToInprogress

        [HttpPut("TestingToInprogress")]
        public async Task<IActionResult> UpdateTaskStatusFromTestingToInProgress([FromBody] UpdateTaskStatusDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Task title is required.");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == model.Title);
            if (task == null)
                return NotFound("Task not found.");

            var oldStatus = task.Status;
            task.Status = Enums.TaskStatus.InProgress;
            task.InprogressCount += 1;
            await _context.SaveChangesAsync();

            var parentTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == task.ParentTaskTitle);
            string teamLeaderName = parentTask?.TeamLeader_Name;
            var developerUserName = task.Developer_Employee_Name;

            if (oldStatus == Enums.TaskStatus.Test)
            {
                var notificationTitle = $"Task '{task.Title}' has bugs!";
                var notificationBody = $"Tester returned task '{task.Title}' to developer for fixing bugs.";

                var developerNotification = new Area.Models.Notification
                {
                    Title = notificationTitle,
                    Body = notificationBody,
                    UserName = developerUserName,
                    TaskID = task.TaskID,
                    TaskTitle = task.Title,
                    sender = task.Test_Employee_Name
                };

                _context.Notifications.Add(developerNotification);
                await _context.SaveChangesAsync();

                var developerTokens = await _context.DevicesToken
                    .Where(dt => dt.UserName == developerUserName)
                    .Select(dt => dt.DeviceToken)
                    .ToListAsync();

                foreach (var token in developerTokens)
                {
                    try { await _firebaseService.SendNotificationAsync(token, notificationTitle, notificationBody); }
                    catch (Exception ex) { Console.WriteLine($"Notification error: {ex.Message}"); }
                }

                if (!string.IsNullOrEmpty(teamLeaderName))
                {
                    var tlTitle = $"Task '{task.Title}' has bugs!";
                    var tlBody = $"Tester reported bugs in task '{task.Title}', developer needs to fix.";

                    var teamLeaderNotification = new Area.Models.Notification
                    {
                        Title = tlTitle,
                        Body = tlBody,
                        UserName = teamLeaderName,
                        TaskID = task.TaskID,
                        TaskTitle = task.Title,
                        sender = task.Test_Employee_Name
                    };

                    _context.Notifications.Add(teamLeaderNotification);
                    await _context.SaveChangesAsync();

                    var teamLeaderTokens = await _context.DevicesToken
                        .Where(dt => dt.UserName == teamLeaderName)
                        .Select(dt => dt.DeviceToken)
                        .ToListAsync();

                    foreach (var token in teamLeaderTokens)
                    {
                        try { await _firebaseService.SendNotificationAsync(token, tlTitle, tlBody); }
                        catch (Exception ex) { Console.WriteLine($"Notification error: {ex.Message}"); }
                    }
                }
            }

            return Ok(new
            {
                message = $"Task '{task.Title}' status updated to 'InProgress'.",
            });
        }
        #endregion

        #region TestToDone
        [HttpPut("TestToDone")]
        public async Task<IActionResult> UpdateTaskStatusFromTestingToDone([FromBody] UpdateTaskStatusDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Task title is required.");

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == model.Title);
            if (task == null)
                return NotFound("Task not found.");

            var oldStatus = task.Status;
            task.Status = Enums.TaskStatus.Done;
            task.SubmissionTaskOnDate = DateTime.Now;
            await _context.SaveChangesAsync();

            // Checklist Score Calculation (Multiple Reviews)
            var checklistReviews = await _context.ChecklistReview
                .Where(r => r.TaskID == task.TaskID)
                .Include(r => r.Items)
                .ToListAsync();

            int checklistScore = checklistReviews.Sum(r => r.TotalScore);
            int numberOfReviews = checklistReviews.Count;
            int maxChecklistScore = numberOfReviews * 30; 

            int deadlineScore = 0;
            bool submittedOnTime = false;

            TimeSpan delay = task.SubmissionTaskOnDate.Date - task.DeadLine.Date;
            if (delay.TotalDays <= 0)
            {
                deadlineScore = 10;
                submittedOnTime = true;
            }
            else if (delay.TotalDays == 1)
            {
                deadlineScore = 9;
            }
            else
            {
                deadlineScore = 8;
            }

            int actualTotalScore = checklistScore + deadlineScore;
            int maxTotalScore = maxChecklistScore + 10;

            double percentage = maxTotalScore > 0
                ? ((double)actualTotalScore / maxTotalScore) * 100
                : 0;

            string formattedPercentage = $"{Math.Round(percentage, 2):0.00} %";
            string formattedChecklistScore = $"{checklistScore}/{maxChecklistScore}";
            string formattedDeadlineScore = $"{deadlineScore}/10";

            var evaluation = new Evaluation
            {
                EvaluationID = Guid.NewGuid().ToString(),
                CheckListScore = formattedChecklistScore,
                DeadLineScore = formattedDeadlineScore,
                Score = formattedPercentage,
                SubmittedOnTime = submittedOnTime,
                TaskEvaluationID = task.TaskID,
                CreatedAT = DateTime.Now
            };

            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            var parentTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == task.ParentTaskTitle);
            string teamLeaderName = parentTask?.TeamLeader_Name;
            var testerUserName = task.Test_Employee_Name;

            if (oldStatus == Enums.TaskStatus.Test && !string.IsNullOrEmpty(teamLeaderName))
            {
                var notificationTitle = $"Task '{task.Title}' is Done!";
                var notificationBody = $"The task has been completed and marked as Done.";

                var teamLeaderNotification = new Area.Models.Notification
                {
                    Title = notificationTitle,
                    Body = notificationBody,
                    UserName = teamLeaderName,
                    TaskID = task.TaskID,
                    TaskTitle = task.Title,
                    sender = testerUserName
                };

                _context.Notifications.Add(teamLeaderNotification);
                await _context.SaveChangesAsync();

                var teamLeaderTokens = await _context.DevicesToken
                    .Where(dt => dt.UserName == teamLeaderName)
                    .Select(dt => dt.DeviceToken)
                    .ToListAsync();

                foreach (var token in teamLeaderTokens)
                {
                    try
                    {
                        await _firebaseService.SendNotificationAsync(token, notificationTitle, notificationBody);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Notification error: {ex.Message}");
                    }
                }
            }

            var checklistHistory = checklistReviews.Select(r => new
            {
                reviewDate = r.ReviewDate,
                note = r.Note,
                totalScore = r.TotalScore,
                items = r.Items.Select(i => new
                {
                    title = i.Title,
                    isSelected = i.IsDone,
                    score = i.Score
                })
            });

            return Ok(new
            {
                message = $"Task '{task.Title}' status updated to 'Done'.",
                checklistScore = formattedChecklistScore,
                deadlineScore = formattedDeadlineScore,
                finalPercentage = formattedPercentage,
                checklistHistory = checklistHistory
            });
        }
        #endregion
    }
}