namespace Area.DTOs
{
    public class KPISDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public int Score { get; set; }
    }
}
