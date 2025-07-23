namespace Area.Models
{
    public class UI_UX : Person
    {
        public virtual ICollection<Task> UI_EmployeeTasks { get; set; } = new List<Task>();
    }
}
