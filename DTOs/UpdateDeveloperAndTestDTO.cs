namespace Area.DTOs
{
    public class UpdateDeveloperAndTestDTO
    {
        public string Title { get; set; }
        public string Discription { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? DeadLine { get; set; }
        public IFormFile Attachment { get; set; }
        public string DeveloperEmployeeName { get; set; }
        public string TestEmployeeName { get; set; }
        public List<IFormFile> UploadDeveloperTask { get; set; }
        public List<string> UploadDeveloperUrls { get; set; }
        public string DeveloperNotes { get; set; }
    }
}
