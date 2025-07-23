using Area.DTOs;
using Area.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Area.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    public class ChecklistController : ControllerBase
    {
        private readonly AreaContext _context;
        private readonly FirebaseService _firebaseService;

        public ChecklistController(AreaContext context, FirebaseService firebaseService)
        {
            _context = context;
            _firebaseService = firebaseService;
        }

        #region Get Check list
        [HttpGet("{taskTitle}")]
        public async Task<IActionResult> GetChecklistForTask(string taskTitle, [FromQuery] string testerName)
        {
            var task = await _context.Tasks
                .Include(t => t.Developer_Employee)
                .FirstOrDefaultAsync(t => t.Title == taskTitle);

            if (task == null)
                return NotFound("Task not found.");

            if (task.Test_Employee_Name != testerName)
                return Forbid("Unauthorized tester.");

            var developer = task.Developer_Employee;

            if (developer == null)
                return BadRequest("Developer info not found.");

            var checklistItems = await _context.CheckListItems
                .Where(i => i.Discriminator == developer.Discriminator && i.job == developer.jobtitle)
                .ToListAsync();

            return Ok(checklistItems);
        }
        #endregion

        #region Add Item
        [HttpPost("AddItem")]
        public async Task<IActionResult> AddChecklistItem([FromBody] CheckListAddItemDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Discriminator) || string.IsNullOrWhiteSpace(dto.job))
                return BadRequest("All fields are required.");

            var checklistItem = new CheckListItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Discriminator = dto.Discriminator,
                job = dto.job,
                Score = dto.Score
            };

            _context.CheckListItems.Add(checklistItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Checklist item added successfully.",
                checklistItem.Id
            });
        }
        #endregion

        #region UI Check List
        [HttpPost("UI")]
        public async Task<IActionResult> GetChecklistForUiUxTask([FromBody] ChecklistRequestUIDTO request)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Title == request.TaskTitle);

            if (task == null)
                return NotFound("Task not found.");

            var project = task.Project;
            if (project == null)
                return BadRequest("Project not associated with the task.");

            if (project.Product_Name != request.OwnerName)
                return Forbid("Unauthorized product owner.");

            var uiuxEmployee = task.UI_UX_Name;

            if (uiuxEmployee == null)
                return BadRequest("UI_Employee info not found.");

            var checklistItems = await _context.CheckListItems
                .Where(i => i.Discriminator == "UI_Employee" && i.job == null)
                .ToListAsync();

            return Ok(checklistItems);
        }
        #endregion

        #region Developer Check List
        [HttpPost]
        public async Task<IActionResult> GetChecklistForTask([FromBody] ChecklistRequestDevTestDTO request)
        {
            var task = await _context.Tasks
                .Include(t => t.Developer_Employee)
                .FirstOrDefaultAsync(t => t.Title == request.TaskTitle);

            if (task == null)
                return NotFound("Task not found.");

            if (task.Test_Employee_Name != request.TesterName)
                return Forbid("Unauthorized tester.");

            var developer = task.Developer_Employee;

            if (developer == null)
                return BadRequest("Developer info not found.");

            var checklistItems = await _context.CheckListItems
                .Where(i => i.Discriminator == developer.Discriminator && i.job == developer.jobtitle)
                .ToListAsync();

            return Ok(checklistItems);
        }
        #endregion

        #region Test Check List
        [HttpPost("Test")]
        public async Task<IActionResult> GetChecklistForTester([FromBody] ChecklistRequestDevTestDTO request)
        {
            var task = await _context.Tasks
                .Include(t => t.Test_Employee)
                .FirstOrDefaultAsync(t => t.Title == request.TaskTitle);

            if (task == null)
                return NotFound("Task not found.");

            if (task.Test_Employee_Name != request.TesterName)
                return Forbid("Unauthorized tester.");

            var tester = await _context.Persons
                .FirstOrDefaultAsync(p => p.UserName == request.TesterName);

            if (tester == null)
                return BadRequest("Tester info not found.");

            var checklistItems = await _context.CheckListItems
                .Where(i => i.Discriminator == "Test_Employee" && i.job == null)
                .ToListAsync();

            return Ok(checklistItems);
        }
        #endregion

        #region Submit Check List
        [Authorize]
        [HttpPost("Submit")]
        public async Task<IActionResult> SubmitChecklistReview([FromBody] ChecklistSubmitDTO dto)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == dto.TaskTitle);
            if (task == null)
                return NotFound("Task not found.");

            var review = new ChecklistReview
            {
                TaskID = task.TaskID,
                ReviewDate = DateTime.UtcNow,
                Note = dto.Note,
                Items = new List<TaskCheckListItem>()
            };

            int totalScore = 0;
            int fullScore = 0;

            foreach (var itemDto in dto.Items)
            {
                var checkListItem = await _context.CheckListItems.FindAsync(itemDto.CheckListItemId);
                if (checkListItem == null)
                    continue;

                fullScore += checkListItem.Score;

                var score = itemDto.IsSelected ? checkListItem.Score : 0;

                var taskCheckItem = new TaskCheckListItem
                {
                    Title = checkListItem.Title,
                    IsDone = itemDto.IsSelected,
                    TaskTitle = task.Title,
                    TaskID = task.TaskID,
                    Score = score
                };

                review.Items.Add(taskCheckItem);
                totalScore += score;
            }

            review.TotalScore = totalScore;

            review.TotalPercentage = fullScore == 0 ? 0 : Math.Round(((double)totalScore / fullScore) * 100, 2);

            _context.ChecklistReview.Add(review);
            await _context.SaveChangesAsync();

            var returnedItems = review.Items.Select(i => new
            {
                i.Title,
                IsSelected = i.IsDone,
                i.Score
            });

            var Evaluator = User.FindFirst("UserName")?.Value;

            var notificationTitle = $"Task Check List Submited";
            var notificationBody = $"Task {task.Title} had Evaluated by {Evaluator}";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = task.Developer_Employee_Name ?? task.UI_UX_Name,
                TaskID = task.TaskID,
                TaskTitle = task.Title,
                sender = Evaluator
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceToken = await _context.DevicesToken
                .Where(dt => dt.UserName == Evaluator)
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
                message = "Checklist review submitted successfully",
                totalScore = $"{totalScore}/{fullScore}",
                percentageScore = $"{review.TotalPercentage}%",
                checklist = returnedItems,
                Note = dto.Note,
            });
        }
        #endregion
    }
}