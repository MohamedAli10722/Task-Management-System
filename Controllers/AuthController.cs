using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Area.Models;
using Microsoft.AspNetCore.Identity.Data;
using LoginRequest = Area.Models.LoginRequest;
namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly AreaContext _context;
        public AuthController(AreaContext context)
        {
            _db = context;

        }
        private readonly AreaContext _db;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Find the user in the database
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserEmail == request.UserEmail && u.Password == request.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    user.UserEmail,
                    user.NationalNumber
                }
            });

            //[HttpPost("Login")]
            //public IActionResult Login([FromBody] User user)
            //{
            //    var existingUser = _context.Users
            //     .FirstOrDefault(u => u.UserEmail == user.UserEmail && u.Password == user.Password);

            //    if (existingUser == null)
            //    {
            //        return Unauthorized("Invalid email or password");
            //    }

            //    return Ok(new { userEmail = existingUser.UserEmail });
            //}


            //[HttpGet]
            //public async Task<IActionResult> GetPerson()
            //{
            //    var data = await _db.Users.ToListAsync();
            //    return Ok(data);
            //}
            //[HttpPost]
            //public async Task<IActionResult> AddPerson(string person)
            //{
            //    Person P = new() { };
            //    await _db.Persons.AddAsync(P);
            //    return Ok(P);
            //}


            //[HttpPost("login")]
            //public async Task<IActionResult> Login([FromBody] LoginRequest request)
            //{
            //    var user = await _context.Users
            //        .Include(u => u.Person) // Include related person data
            //        .FirstOrDefaultAsync(u => u.UserEmail == request.UserEmail && u.Password == request.Password);

            //    if (user == null)
            //        return Unauthorized(new { message = "Invalid credentials" });

            //    return Ok(new
            //    {
            //        message = "Login successful",
            //        user = new
            //        {
            //            user.UserName,
            //            user.Person.Name,
            //            user.Person.Email,
            //            user.Person.NationalNumber
            //        }
            //    });
            //}
        }
    }
}
