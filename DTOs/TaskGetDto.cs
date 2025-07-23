namespace Area.DTOs
{
    public class TaskGetDto
    {
        public string Title { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public string Discription { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DeadLine { get; set; }
        public string Attachment { get; set; }
        public string Team_Leader_Name { get; set; }
        public string UI_UX_Name { get; set; }
        public string Developer_Employee_Name { get; set; }
        public string Test_Employee_Name { get; set; }
        public string ProjectTitle { get; set; }
        public string ParentTaskTitle { get; set; }
        public string TotalScore { get; set; }
        public string DeveloperTask { get; set; }
        public string DeveloperNotes { get; set; }
        public string UITask { get; set; }
        public string UINotes { get; set; }
        public List<ChecklistGetRevDTO> ChecklistItems { get; set; }
    }
}