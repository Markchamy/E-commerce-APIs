namespace Backend.Models
{
    public class LoginUserDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; } // Only for response
        public string LastName { get; set; }  // Only for response
        public string Role { get; set; } // Include role
        public EmployeeDetailsDTO? EmployeeDetails { get; set; }
    }

}
