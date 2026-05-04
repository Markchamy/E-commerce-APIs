namespace Backend.Models
{
    public class UserDTO
    {
        public long Id { get; set; }
        public string Role { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public DateOnly Birthday { get; set; }
        public CustomerDTO Customer { get; set; } = null!;
        public EmployeeDTO Employee { get; set; } = null!;
        public string PasswordResetToken { get; set; } = null!;
        public DateTime? ResetTokenExpiry { get; set; }
    }

    public class UserCustomerDTO
    {
        public long Id { get; set; }
        public string Role { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public DateOnly Birthday { get; set; }
        public CustomerDTO Customer { get; set; } = null!;
    }

    public class PasswordResetRequestDTO
    {
        public string Email { get; set; } = null!;
    }

    public class PasswordResetDTO
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }


}
