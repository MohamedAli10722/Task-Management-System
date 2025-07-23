namespace Area.DTOs
{
    public class DeveloperAndTestDTO
    {
        public string Title { get; set; }
        public string Discription { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DeadLine { get; set; }
        public IFormFile Attachment { get; set; }
        public string DeveloperUserName { get; set; }
        public string TestUserName { get; set; }
        public string ParentTaskTitle { get; set; }
    }
}

