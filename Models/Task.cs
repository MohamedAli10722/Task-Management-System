using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Area.Models
{
    public class Task
    {
        [Key]
        public string TaskID { get; set; }

        [ForeignKey("Project")]
        public string ProjectID { get; set; }

        [ForeignKey("Person")]
        public string PersonId { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public string Discription { get; set; }

        public string DeadLine { get; set; }


        public Person Person { get; set; }
        public Project Project { get; set; }  
        public Evaluation Evaluation { get; set; }
    }
}
