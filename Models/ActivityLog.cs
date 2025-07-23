using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        public string Action { get; set; }        
        public string EntityName { get; set; }     
        public string EntityTitle { get; set; }     
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public string Username { get; set; }        
    }
}
