using Microsoft.AspNetCore.Identity;

namespace Area.Models
{
    public class LoginRequest
    {
        public string UserEmail { get; set; }
        public string Password { get; set; }
    }
}
