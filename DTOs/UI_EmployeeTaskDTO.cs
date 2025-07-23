namespace Area.DTOs
{
    public class UI_EmployeeTaskDTO
    {
        public string Title { get; set; }
        public string Discription { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DeadLine { get; set; }
        public IFormFile Attachment { get; set; }
        public string UI_EmployeeUserName { get; set; } 
        public string ProjectTitle { get; set; } 
    }
}
