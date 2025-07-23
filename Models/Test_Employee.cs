namespace Area.Models
{
    public class Test_Employee : Person
    {
        public virtual ICollection<Task> Test_EmployeeTasks { get; set; } = new List<Task>();
    }
}
