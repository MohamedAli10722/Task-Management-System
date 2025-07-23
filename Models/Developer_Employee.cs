using Area.Enums;
using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class Developer_Employee : Person
    {
        public virtual ICollection<Task> Developer_EmployeeTasks { get; set; } = new List<Task>();

    }
}
