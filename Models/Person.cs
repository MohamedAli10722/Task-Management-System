using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Area.Models
{
    public class Person
    {
        [Key]
        [Required]
        [MaxLength(14)]
        public string NationalNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(50)]
        public string Nationality { get; set; }

        [MaxLength(100)]
        public string Graduation { get; set; }

        [MaxLength(100)]
        public string Worked_In { get; set; }


        // علاقة الـ Person بالـ User
        [JsonIgnore] // مش هنطلع بيانات الـ User هنا

        public User User { get; set; }
        public Project Project { get; set; }
        public Task Task { get; set; }
        public Evaluation Evaluation { get; set; }  
    }
}
