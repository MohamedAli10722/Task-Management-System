using Area.Enums;

namespace Area.DTOs
{
    public class UpdateProjectStatusDTO
    {
        public string Title { get; set; }
        public ProjectStatus NewStatus { get; set; }
    }
}