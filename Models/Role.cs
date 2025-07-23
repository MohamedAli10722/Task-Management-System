using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class Role
    {
        [Key]
        [Required]
        [StringLength(14)]
        public string Role_id{ get; set; }
        [MaxLength()]
        public string Role_name { get; set; }
        public virtual List<Person> Persons { get; set; }
        public virtual List<RolePermission> RolePermission { get; set; }
    }
}
