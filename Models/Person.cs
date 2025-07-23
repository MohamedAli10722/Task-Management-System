using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        public string Password { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(50)]
        public string Nationality { get; set; }

        public string Discriminator { get; set; }

        [ForeignKey("Roleid")]
        public string Roleid { get; set; }

        public  virtual Role Role { get; set; }

        public string jobtitle { get; set; }
        public string ImagePath { get; set; }

        public virtual ICollection<PersonNotification> PersonNotifications { get; set; }
    }
}