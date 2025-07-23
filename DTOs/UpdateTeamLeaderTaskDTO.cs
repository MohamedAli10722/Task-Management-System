using Area.Enums;

namespace Area.DTOs
{
    public class UpdateTeamLeaderTaskDTO
    {
        public string Title { get; set; }
        public string Discription { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? DeadLine { get; set; }
        public string Attachment { get; set; }
        public string TeamLeaderName { get; set; }
    }
}
