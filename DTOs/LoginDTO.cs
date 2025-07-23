using System.ComponentModel.DataAnnotations;

namespace Area.DTOs
{
    public class LoginDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
