namespace Area.DTOs
{
    public class ProductOwnerDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int ProjectsThisMonth { get; set; }
        public int RemainingProjects { get; set; }
        public int maxProjectsPerMonth { get; set; }
    }
}
