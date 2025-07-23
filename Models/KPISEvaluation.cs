namespace Area.Models
{
    public class KPISEvaluation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EmployeeId { get; set; }
        public Person Employee { get; set; }
        public string ManagerId { get; set; }
        public Person Manager { get; set; }
        public DateTime EvaluationDate { get; set; }
        public string FinalScore { get; set; } 
        public List<KPISelection> KPISelections { get; set; }
    }
}
