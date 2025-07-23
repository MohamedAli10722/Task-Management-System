namespace Area.Dtos
{
    public class ReportDTO
    {
        public string RoleName { get; set; }
        public double Percentage { get; set; } 
        public List<string> Permissions { get; set; }
        public List<string> Employees { get; set; }
    }
}

