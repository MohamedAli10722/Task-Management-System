using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Area.Enums;
namespace Area.Models
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string TaskID { get; set; }
        public string Title { get; set; }
        [Required]
        public Enums.TaskStatus Status { get; set; }
        public string Discription { get; set; }
        public int InprogressCount {  get; set; } 
        public DateTime CreationData { get; set; }
        public DateTime DeadLine { get; set; }
        public bool IsFirstTimeInTesting { get; set; }
        public DateTime SubmissionTaskOnDate { get; set; }
        public string AttachmentPath { get; set; } 
        public string Product_Id { get; set; }
        public ProductOwner ProductOwner { get; set; }
        public string Leader_Id { get; set; }
        public TeamLeader TeamLeader { get; set; }
        public string TeamLeader_Name { get; set; }
        public string Project_Id { get; set; }
        public string Project_Title { get; set; }
        public Project Project { get; set; }
        public string UI_UXId { get; set; }
        [JsonIgnore]
        public virtual UI_UX UI_UX { get; set; }
        public string UI_UX_Name { get; set; }
        public string Developer_EmployeeId { get; set; }
        [JsonIgnore]
        public virtual Developer_Employee Developer_Employee { get; set; }
        public string Developer_Employee_Name { get; set; }
        public string Test_EmployeeId { get; set; }
        [JsonIgnore]
        public virtual Test_Employee Test_Employee { get; set; }
        public string Test_Employee_Name { get; set; }
        public string Evaluatiotasknid { get; set; }
        public Evaluation EvaluationTask { get; set; }
        public virtual ICollection<Notification> NotificationsTask { get; set; } = new List<Notification>();
        public virtual ICollection<Task> ChildTasks { get; set; } = new List<Task>();
        public string ParentTaskId { get; set; }
        public string ParentTaskTitle { get; set; }
        public Task ParentTask { get; set; }
        public virtual ICollection<TaskCheckListItem> TaskCheckListItems { get; set; } = new List<TaskCheckListItem>();
        public ICollection<ChecklistReview> ChecklistReviews { get; set; }
        public string UploadDeveloperTask { get; set; }
        public string DeveloperNotes { get; set; }
        public string UploadUITask { get; set; }
        public string UINotes { get; set; }
    }
}
