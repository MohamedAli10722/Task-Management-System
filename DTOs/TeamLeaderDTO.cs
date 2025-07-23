namespace Area.DTOs
{
    public class TeamLeaderDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TasksThisMonth { get; set; }
        public int RemainingTasks { get; set; }
        public int maxTasksPerMonth { get; set; }
    }
}
