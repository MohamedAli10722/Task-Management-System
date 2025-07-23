namespace Area.Models
{
    public class ProductOwner : Person
    {
        public ICollection<Project> Projects { get; set; }
        public ICollection<Task> ProductTasks { get; set; }
    }
}
