using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class ChecklistReview
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TaskID { get; set; }
        public string Note { get; set; }
        public Task Task { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
        public List<TaskCheckListItem> Items { get; set; } = new List<TaskCheckListItem>();
        public int TotalScore { get; set; }
        public double TotalPercentage { get; set; }

    }
}
