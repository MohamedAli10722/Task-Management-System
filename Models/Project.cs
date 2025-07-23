using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Area.Enums;
namespace Area.Models
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ProjectID { get; set; }
        public string Title { get; set; }
        public ProjectStatus Status { get; set; }
        public string Discription { get; set; }
        public DateTime CreationData { get; set; }
        public DateTime DeadLine { get; set; }
        public string Product_Name { get; set; }
        public string Product_Id { get; set; }
        public ProductOwner ProductOwner { get; set; }
        public DateTime SubmissionProjectDate { get; set; }
        public bool SubmittedProjectOnTime { get; set; }
        public double EvaluationScore { get; set; }
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        public string EvaluatioProjectid { get; set; }
        public Evaluation EvaluationProject { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }                       
}
