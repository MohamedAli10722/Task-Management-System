namespace Area.DTOs
{
    public class Developer_EmployeeDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string jobtitle { get; set; }
        public int TasksThisMonth { get; set; }
        public int RemainingTasks { get; set; }
        public int maxTasksPerMonth { get; set; }
    }
}
