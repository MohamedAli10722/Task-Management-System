using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly AreaContext _context;

        public DeveloperController(AreaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeveloper_Employees()
        {
            try
            {
                int maxTasksPerMonth = 5;
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

                var Developer_Employees = await _context.Persons
                    .Where(p => EF.Property<string>(p, "Discriminator") == "Developer_Employee")
                    .Select(p => new Developer_EmployeeDTO
                    {
                        UserName = p.FirstName + " " + p.LastName,
                        Email = p.Email,
                        jobtitle = p.jobtitle,
                        TasksThisMonth = _context.Tasks
                            .Count(t => t.Developer_EmployeeId == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        RemainingTasks = maxTasksPerMonth - _context.Tasks
                            .Count(t => t.Developer_EmployeeId == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        maxTasksPerMonth = maxTasksPerMonth
                    })
                    .ToListAsync();

                if (Developer_Employees.Count == 0)
                {
                    return NotFound("No Developer Employees found.");
                }

                return Ok(Developer_Employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}