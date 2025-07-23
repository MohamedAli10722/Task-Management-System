using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AreaContext _context;

        public TestController(AreaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTest_Employees()
        {
            try
            {
                int maxTasksPerMonth = 5;
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

                var Test_Employees = await _context.Persons
                    .Where(p => EF.Property<string>(p, "Discriminator") == "Test_Employee")
                    .Select(p => new Test_EmployeeDTO
                    {
                        UserName = p.FirstName + " " + p.LastName,
                        Email = p.Email,
                        TasksThisMonth = _context.Tasks
                            .Count(t => t.Test_EmployeeId == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        RemainingTasks = maxTasksPerMonth - _context.Tasks
                            .Count(t => t.Test_EmployeeId == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        maxTasksPerMonth = maxTasksPerMonth
                    })
                    .ToListAsync();

                if (Test_Employees.Count == 0)
                {
                    return NotFound("No Test Employees found.");
                }

                return Ok(Test_Employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
