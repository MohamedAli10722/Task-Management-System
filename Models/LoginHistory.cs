using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public DateTime LogoutTime { get; set; }
    }
}
