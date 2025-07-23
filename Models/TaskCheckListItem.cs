using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class TaskCheckListItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public bool IsDone { get; set; } = false;
        public string TaskTitle { get; set; }
        public int Score { get; set; } 
        public string TaskID { get; set; }
        public virtual Task Task { get; set; }
        public string ChecklistReviewId { get; set; }
        public ChecklistReview ChecklistReview { get; set; }

    }
}
