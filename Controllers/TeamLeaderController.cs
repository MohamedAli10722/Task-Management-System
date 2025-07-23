using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task = Area.Models.Task;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamLeaderController : ControllerBase
    {
        private readonly AreaContext _context;

        public TeamLeaderController(AreaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeamLeaders()
        {
            try
            {
                int maxTasksPerMonth = 5;
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

                var TeamLeaders = await _context.Persons
                    .Where(p => EF.Property<string>(p, "Discriminator") == "Team_Leader")
                    .Select(p => new TeamLeaderDTO
                    {
                        UserName = p.FirstName + " " + p.LastName,
                        Email = p.Email,
                        TasksThisMonth = _context.Tasks
                            .Count(t => t.Leader_Id == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        RemainingTasks = maxTasksPerMonth - _context.Tasks
                            .Count(t => t.Leader_Id == p.NationalNumber &&
                                        t.DeadLine >= monthStart &&
                                        t.DeadLine <= monthEnd),
                        maxTasksPerMonth = maxTasksPerMonth
                    })
                    .ToListAsync();

                if (TeamLeaders.Count == 0)
                {
                    return NotFound("No Team Leaders found.");
                }

                return Ok(TeamLeaders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}