using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ForceChangePasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string NewPassword { get; set; }

        // Optional: for Cognito session if the user is in FORCE_CHANGE_PASSWORD state
        public string? CognitoUsername { get; set; }
    }

    public class ForceChangePasswordResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public bool CognitoUpdated { get; set; }
        public bool DatabaseUpdated { get; set; }
    }
}