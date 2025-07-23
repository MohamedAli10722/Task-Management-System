using Area.Enums;

namespace Area.DTOs
{
    public class EmployeeDTO
    {
        public string NationalNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public DateTime Date_Of_Birth { get; set; }
        public string Nationality { get; set; }
        public string Address { get; set; }
        public string Discriminator { get; set; }
        public string job { get; set; }
    }
}
