namespace Backend.Models
{
    public class UserModel
    {
        public long Id { get; set; }
        public string? role { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set;}
        public string? email { get; set; }
        public string? password { get; set; }
        public string? phone_number { get; set; }

        public DateOnly? Birthday { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public virtual CustomerModel Customer { get; set; } = null!;
        public virtual EmployeeModel Employee { get; set; } = null!;
        // Navigation property to Orders
        public ICollection<OrdersModel> Orders { get; set; } = new List<OrdersModel>();
        public ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
    }
}
