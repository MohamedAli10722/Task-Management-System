using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class Evaluation
    {

        [Key]
        public string EvaluationID { get; set; }

        [ForeignKey("Project")]
        public string ProjectID { get; set; }

        [ForeignKey("Task")]
        public string TaskID { get; set; }

        [ForeignKey("Person")]
        public string PersonId { get; set; }

        public string Title { get; set; }

        public string Discription { get; set; }


        public Project Project { get; set; }
        public Task Task { get; set; }
        public Person Person { get; set; }
    }
}
