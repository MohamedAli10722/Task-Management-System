namespace Area.DTOs
{
    public class EmployeeActivityDTO
    {
        public string UserName { get; set; }
        public List<LoginSessionWithActivitiesDTO> Sessions { get; set; } = new();
    }

    public class LoginSessionWithActivitiesDTO
    {
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public List<ActivityEntryDTO> Activities { get; set; } = new();
    }

    public class ActivityEntryDTO
    {
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string EntityTitle { get; set; }
        public DateTime Time { get; set; }
    }


}
