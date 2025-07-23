namespace Area.Models
{
    public class KPISelection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string KPIId { get; set; }
        public KPIS KPI { get; set; }
        public bool IsSelected { get; set; }
        public int Score { get; set; }
        public string EvaluationId { get; set; }
        public KPISEvaluation Evaluation { get; set; }
    }
}
