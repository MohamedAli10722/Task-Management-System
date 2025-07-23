using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UIController : ControllerBase
    {
        private readonly AreaContext _context;

        public UIController(AreaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUI_Employees()
        {
            try
            {
                int maxTasksPerMonth = 5;
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

                var UI_Employees = await _context.Persons
                    .Where(p => EF.Property<string>(p, "Discriminator") == "UI_Employee")
                    .Select(p => new UI_EmployeeDTO
                    {
                        UserName = p.FirstName + " " + p.LastName,
                        Email = p.Email,
                        TasksThisMonth = _context.Tasks
                            .Count(t => t.UI_UXId == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        RemainingTasks = maxTasksPerMonth - _context.Tasks
                            .Count(t => t.UI_UXId == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        maxTasksPerMonth = maxTasksPerMonth
                    })
                    .ToListAsync();

                if (UI_Employees.Count == 0)
                {
                    return NotFound("No UI/UX Employees found.");
                }

                return Ok(UI_Employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
