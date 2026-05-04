namespace Backend.Models
{
    public class RegisterEmployeeDTO
    {
        public EmployeeUserDTO User { get; set; }
    }

    public class EmployeeUserDTO
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public EmployeeDetailsDTO Employee { get; set; }
    }

    public class EmployeeDetailsDTO
    {
        public List<string>? AccessControl { get; set; } // Example: Specify page access level or permissions
    }

}
