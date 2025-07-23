namespace Area.Models
{
    public class Manager: Person
    {
        public ICollection<Project> Projects { get; set; }
    }
}
