using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Area.DTOs;
using Area.Jwt;

namespace Area.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AreaContext _context;
        private readonly IConfiguration _config;
        private readonly JwtTokenService _jwtService;

        public LoginController(AreaContext context, IConfiguration config, JwtTokenService jwtService)
        {
            _context = context;
            _config = config;
            _jwtService = jwtService;
        }

        #region Login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Persons
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || user.Password != model.Password) 
                return Unauthorized("Invalid email or password");

            var loginHistory = new LoginHistory
            {
                Email = model.Email,
                LoginTime = DateTime.UtcNow
            };

            _context.LoginHistory.Add(loginHistory);
            await _context.SaveChangesAsync();

            _context.Update(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful ",

                Token = token,  
                Role = user.Role.Role_name,
                jobTitle = user.jobtitle,
                email = user.Email,
                LoginId = loginHistory.Id,
                phoneNumber = user.MobileNumber,
                username = user.UserName,
                ImagePath = user.ImagePath, 
            });
        }
        #endregion

        #region Logout
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] int loginId)
        {
            var loginRecord = await _context.LoginHistory.FindAsync(loginId);

            if (loginRecord == null)
                return NotFound("Login history record not found");

            loginRecord.LogoutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Logout time recorded");
        }
        #endregion
    }
}