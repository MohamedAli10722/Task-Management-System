namespace Area.DTOs
{
    public class ChecklistGetRevDTO
    {
        public DateTime ReviewDate { get; set; }
        public List<CheckListGetDTO> Items { get; set; }
    }
}
