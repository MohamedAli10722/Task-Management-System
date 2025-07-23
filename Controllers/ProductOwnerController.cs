using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductOwnerController : ControllerBase
    {
        private readonly AreaContext _context;

        public ProductOwnerController(AreaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductOwners()
        {
            try
            {
                int maxProjectsPerMonth = 5;
                var now = DateTime.UtcNow; 
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

                var productOwners = await _context.Persons
                    .Where(p => EF.Property<string>(p, "Discriminator") == "Product_Owner")
                    .ToListAsync();

                var projectsThisMonth = await _context.Projects
                    .Where(p => p.DeadLine >= monthStart && p.DeadLine <= monthEnd)
                    .ToListAsync();

                var result = productOwners.Select(po =>
                {
                    var fullName = po.FirstName + " " + po.LastName;
                    var count = projectsThisMonth.Count(p =>
                        p.Product_Id.Trim().ToLower() == po.NationalNumber.Trim().ToLower());

                    return new ProductOwnerDTO
                    {
                        UserName = fullName,
                        Email = po.Email,
                        ProjectsThisMonth = count,
                        RemainingProjects = maxProjectsPerMonth - count,
                        maxProjectsPerMonth = maxProjectsPerMonth
                    };
                }).ToList();

                if (result.Count == 0)
                {
                    return NotFound("No Product Owners found.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
