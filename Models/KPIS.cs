using System.ComponentModel.DataAnnotations;

namespace Area.Models
{
    public class KPIS
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public int Score { get; set; }
    }
}
