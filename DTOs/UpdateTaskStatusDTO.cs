using Area.Enums;

namespace Area.DTOs
{
    public class UpdateTaskStatusDTO
    {
        public string Title { get; set; }
        public System.Threading.Tasks.TaskStatus NewStatus { get; set; }
    }
}