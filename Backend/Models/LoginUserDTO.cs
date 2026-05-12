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
        public int StoreId { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public EmployeeDetailsDTO? EmployeeDetails { get; set; }
    }

    public class RefreshTokenRequestDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int StoreId { get; set; }
    }

}
