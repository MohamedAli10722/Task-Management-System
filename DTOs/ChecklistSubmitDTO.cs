namespace Area.DTOs
{
    public class ChecklistSubmitDTO
    {
        public string TaskTitle { get; set; }
        public string Note { get; set; }
        public List<ChecklistItemReviewDTO> Items { get; set; }
    }
}
