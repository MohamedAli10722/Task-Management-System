using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Area.Models
{
    public class DevicesToken
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserName { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }
    }
}
