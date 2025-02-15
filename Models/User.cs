using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Area.Models
{
    public class User
    {
        [Key]
        [ForeignKey("Person")]
        public string NationalNumber { get; set; }

        [MaxLength(50)]
        public required string UserEmail { get; set; }


        [MaxLength(50)]
        public required string Password { get; set; }

        [JsonIgnore] // مش هنطلع بيانات الـ Person هنا
        public Person Person { get; set; }
    }
}
