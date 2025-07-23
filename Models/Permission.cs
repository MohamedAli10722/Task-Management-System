
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Area.Models
{
    public class Permission
    {
        [Key]
        [Required]
        [StringLength(14)]
        public string Permission_id { get; set; }
        [StringLength(30)]
        public string Permission_name { get; set; }
        public virtual List<RolePermission> RolePermission { get; set; }
    }
}
