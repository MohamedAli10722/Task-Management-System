namespace Area.Models
{
    public class TeamLeader : Person
    {
        public virtual ICollection<Task> TeamLeaderTasks { get; set; } = new List<Task>();
    }
}
