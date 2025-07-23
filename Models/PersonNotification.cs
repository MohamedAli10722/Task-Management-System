namespace Area.Models
{
    public class PersonNotification
    {
        public string person_ID { get; set; }
        public Person Person { get; set; }
        public string notifi_ID { get; set; }
        public Notification Notification { get; set; }
    }
}
