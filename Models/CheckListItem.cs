namespace Area.Models
{
    public class CheckListItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Discriminator { get; set; }
        public string job { get; set; }
        public int Score { get; set; }
    }
}
