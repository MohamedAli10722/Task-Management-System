using Area.DTOs;
using Area.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Area.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly AreaContext _context;

        public ProfileController(AreaContext context)
        {
            _context = context;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfileImage([FromForm] ProfileDTO profileDTO)
        {
            if (profileDTO == null || profileDTO.Image == null || profileDTO.Image.Length == 0)
                return BadRequest("Image is required.");

            if (string.IsNullOrEmpty(profileDTO.Email))
                return BadRequest("Email is required.");

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.Email == profileDTO.Email);

            if (person == null)
                return NotFound("Person not found.");

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImages");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileDTO.Image.FileName);
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileDTO.Image.CopyToAsync(stream);
            }

            var imageUrl = $"{Request.Scheme}://{Request.Host}/ProfileImages/{fileName}";

            person.ImagePath = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Image updated successfully.",
                imageUrl = imageUrl
            });
        }
    }
}