using Newtonsoft.Json;

namespace Area.DTOs
{
    public abstract class BaseEmployeeEvaluationDto
    {
        [JsonProperty(Order = 1)]
        public string UserName { get; set; }
        [JsonProperty(Order = 2)]
        public string Discriminator { get; set; }
        [JsonProperty(Order = 3)]
        public List<KPIEvaluationSummaryDto> KPIEvaluations { get; set; } = new();
    }

    public class ProductOwnerEvaluationDto : BaseEmployeeEvaluationDto
    {
        [JsonProperty(Order = 4)]
        public List<string> OwnedProjects { get; set; } = new();
    }

    public class EmployeeTaskEvaluationDto : BaseEmployeeEvaluationDto
    {
        [JsonProperty(Order = 4)]
        public List<TaskReviewSummaryDto> TaskEvaluations { get; set; } = new();
    }

    public class TaskReviewSummaryDto
    {
        public string TaskTitle { get; set; }
        public string TotalScore { get; set; }
        public DateTime ReviewDate { get; set; }
    }

    public class KPIEvaluationSummaryDto
    {
        public string FinalScore { get; set; }
        public DateTime EvaluationDate { get; set; }
    }

    public class EmployeeEvaluationGroupDto
    {
        public string Role { get; set; } 
        public List<object> Employees { get; set; } = new();
    }
}
