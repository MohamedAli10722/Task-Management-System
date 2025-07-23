using Area.DTOs;
using Area.Enums;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectStatusController : ControllerBase
    {
        private readonly AreaContext _context;
        private readonly FirebaseService _firebaseService;

        public ProjectStatusController(AreaContext context, FirebaseService firebaseService)
        {
            _context = context;
            _firebaseService = firebaseService;
        }
        [HttpPut]
        public async Task<IActionResult> UpdateProjectStatus([FromBody] UpdateProjectStatusDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Project title is required.");

            var project = await _context.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.EvaluationTask)
                .FirstOrDefaultAsync(p => p.Title == model.Title);

            if (project == null)
                return NotFound("Project not found.");

            // ❗ Prevent changing status to Done if not all tasks are Done
            if (model.NewStatus == ProjectStatus.Done &&
                project.Tasks.Any(t => t.Status != Enums.TaskStatus.Done))
            {
                return BadRequest("Cannot mark the project as Done until all its tasks are marked as Done.");
            }

            // Update status
            project.Status = model.NewStatus;

            // Calculate evaluation if all tasks are done and project is marked as Done
            if (model.NewStatus == ProjectStatus.Done)
            {
                var taskEvaluations = project.Tasks
                    .Where(t => t.EvaluationTask != null &&
                                !string.IsNullOrWhiteSpace(t.EvaluationTask.Score) &&
                                double.TryParse(t.EvaluationTask.Score.Replace("%", "").Trim(), out _))
                    .Select(t => double.Parse(t.EvaluationTask.Score.Replace("%", "").Trim()))
                    .ToList();

                double taskScoreAvg = 0;
                if (taskEvaluations.Any())
                {
                    taskScoreAvg = Math.Round(taskEvaluations.Average(), 2);
                }

                int deadlineScore = 0;
                bool submittedOnTime = false;
                TimeSpan delay = project.SubmissionProjectDate.Date - project.DeadLine.Date;

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

                double finalScore = Math.Round((taskScoreAvg + deadlineScore) / 2, 2);

                var evaluation = new Evaluation
                {
                    EvaluationID = Guid.NewGuid().ToString(),
                    Score = $"{finalScore:0.00} %",
                    CheckListScore = $"{taskScoreAvg:0.00} %",
                    DeadLineScore = $"{deadlineScore}/10",
                    SubmittedOnTime = submittedOnTime,
                    CreatedAT = DateTime.Now,
                    ProjectEvaluationID = project.ProjectID,
                    TaskEvaluationID = null
                };

                _context.Evaluations.Add(evaluation);

                project.EvaluatioProjectid = evaluation.EvaluationID;
                project.EvaluationProject = evaluation;
            }
            await _context.SaveChangesAsync();

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.UserName == "Ali Ahmed");

            var notificationTitle = $"Project: {project.Title} Has Been Finished";
            var notificationBody = $"{project.Product_Name} Submitted the Project";

            var notification = new Area.Models.Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                UserName = person.UserName,
                ProjectID = project.ProjectID,
                ProjectTitle = project.Title,
                sender = project.Product_Name
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var deviceTokens = await _context.DevicesToken
                .Where(dt => dt.UserName == person.UserName)
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
                message = $"Project '{project.Title}' status updated to '{project.Status}'.",
                projectTitle = project.Title,
                newStatus = project.Status.ToString()
            });
        }
    }
}