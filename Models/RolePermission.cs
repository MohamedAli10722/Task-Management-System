using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Area.Models
{
    public class RolePermission
    {
        public string Role_id { get; set; }
        public virtual Role Role { get; set; }
        public string Permission_id { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
