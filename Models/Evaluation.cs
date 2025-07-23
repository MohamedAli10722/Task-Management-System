using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class Evaluation
    {
        [Key]
        public string EvaluationID { get; set; }
        public string CheckListScore { get; set; }      
        public string DeadLineScore { get; set; }    
        public string Score { get; set; }            
        public bool SubmittedOnTime { get; set; }
        public DateTime CreatedAT { get; set; } = DateTime.Now;
        public string TaskEvaluationID { get; set; }
        public Task TaskEvaluation { get; set; }
        public string ProjectEvaluationID { get; set; }
        public Project ProjectEvaluation { get; set; }
    }
}
