using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly AreaContext _context;

        public SearchController(AreaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SearchByTitle([FromBody] SearchDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Title))
                return BadRequest("Title is required.");

            // 1. Try to find project by title
            var projectMatch = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Title.Contains(dto.Title));

            if (projectMatch != null)
            {
                var tasksWithEmployees = new List<object>();

                foreach (var task in projectMatch.Tasks)
                {
                    var employeeUsernames = new List<string>();

                    if (!string.IsNullOrWhiteSpace(task.Developer_Employee_Name))
                        employeeUsernames.Add(task.Developer_Employee_Name);
                    if (!string.IsNullOrWhiteSpace(task.UI_UX_Name))
                        employeeUsernames.Add(task.UI_UX_Name);
                    if (!string.IsNullOrWhiteSpace(task.TeamLeader_Name))
                        employeeUsernames.Add(task.TeamLeader_Name);
                    if (!string.IsNullOrWhiteSpace(task.Test_Employee_Name))
                        employeeUsernames.Add(task.Test_Employee_Name);

                    var assignedEmployees = new List<string>();
                    foreach (var username in employeeUsernames.Distinct())
                    {
                        var person = await _context.Persons.FirstOrDefaultAsync(p => p.UserName == username);
                        if (person != null)
                            assignedEmployees.Add($"{person.UserName} - {person.Discriminator}");
                        else
                            assignedEmployees.Add(username); 
                    }

                    tasksWithEmployees.Add(new
                    {
                        task.Title,
                        task.Discription,
                        task.Status,
                        task.CreationData,
                        task.DeadLine,
                        AssignedTo = assignedEmployees,
                    });
                }

                return Ok(new
                {
                    ProjectTitle = projectMatch.Title,
                    Discription = projectMatch.Discription,
                    Status = projectMatch.Status,
                    CreationDate = projectMatch.CreationData,
                    DeadLine = projectMatch.DeadLine,
                    ProductOwnerName = projectMatch.Product_Name,
                    IncludedTasks = tasksWithEmployees
                });
            }

            // If project not found, search in tasks
            var taskMatch = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Title.Contains(dto.Title));

            if (taskMatch != null)
            {
                var employeeUsernames = new List<string>();

                if (!string.IsNullOrWhiteSpace(taskMatch.Developer_Employee_Name))
                    employeeUsernames.Add(taskMatch.Developer_Employee_Name);
                if (!string.IsNullOrWhiteSpace(taskMatch.UI_UX_Name))
                    employeeUsernames.Add(taskMatch.UI_UX_Name);
                if (!string.IsNullOrWhiteSpace(taskMatch.TeamLeader_Name))
                    employeeUsernames.Add(taskMatch.TeamLeader_Name);
                if (!string.IsNullOrWhiteSpace(taskMatch.Test_Employee_Name))
                    employeeUsernames.Add(taskMatch.Test_Employee_Name);

                var assignedEmployees = new List<string>();
                foreach (var username in employeeUsernames.Distinct())
                {
                    var person = await _context.Persons.FirstOrDefaultAsync(p => p.UserName == username);
                    if (person != null)
                        assignedEmployees.Add($"{person.UserName} - {person.Discriminator}");
                    else
                        assignedEmployees.Add(username);
                }

                return Ok(new
                {
                    TaskTitle = taskMatch.Title,
                    Discription = taskMatch.Discription,
                    Status = taskMatch.Status,
                    CreationDate = taskMatch.CreationData,
                    DeadLine = taskMatch.DeadLine,
                    AssignedTo = assignedEmployees,
                    FromProject = taskMatch.Project?.Title
                });
            }

            return NotFound("No project or task found with the given title.");
        }

        [HttpGet("Suggest")]
        public async Task<IActionResult> SuggestTitles([FromQuery] string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
                return BadRequest("Please enter at least 2 characters.");

            var projectTitles = await _context.Projects
                .Where(p => p.Title.Contains(input))
                .Select(p => new { Title = p.Title, Type = "Project" })
                .ToListAsync();

            var taskTitles = await _context.Tasks
                .Where(t => t.Title.Contains(input))
                .Select(t => new { Title = t.Title, Type = "Task" })
                .ToListAsync();

            var suggestions = projectTitles
                .Concat(taskTitles)
                .Distinct()
                .Take(10) 
                .ToList();

            return Ok(suggestions);
        }
    }
}
