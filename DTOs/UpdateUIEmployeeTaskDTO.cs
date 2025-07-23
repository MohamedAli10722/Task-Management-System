namespace Area.DTOs
{
    public class UpdateUIEmployeeTaskDTO
    {

        public string Title { get; set; }
        public string Discription { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? DeadLine { get; set; }
        public string Attachment { get; set; }
        public string UiEmployeeName { get; set; }
        public List<IFormFile> UploadUITask { get; set; }
        public List<string> UploadUIUrls { get; set; }
        public string UINotes { get; set; }
    }
}

