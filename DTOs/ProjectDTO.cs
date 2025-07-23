using Area.Enums;

namespace Area.DTOs
{
    public class ProjectDTO
    {
        public string Title { get; set; }
        public ProjectStatus Status { get; set; }
        public string Discription { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DeadLine { get; set; }
        public string ProductOwnerName { get; set; }
        public string ProjectEvaluation { get; set; }
    }
}
