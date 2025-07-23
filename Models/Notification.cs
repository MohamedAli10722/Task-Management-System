using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class Notification
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string NotificationID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt {  get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string UserName { get; set; }
        public string sender { get; set; }
        public string ProjectID { get; set; }
        public string ProjectTitle { get; set; }
        public Project Project { get; set; }
        public string TaskID { get; set; }
        public string TaskTitle { get; set; }
        public Task Task { get; set; }
        public virtual ICollection<PersonNotification> PersonNotifications { get; set; }
    }
}
