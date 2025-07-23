namespace Area.DTOs
{
    public class UI_EmployeeDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TasksThisMonth { get; set; }
        public int RemainingTasks { get; set; }
        public int maxTasksPerMonth { get; set; }
    }
}
