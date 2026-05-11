using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    public class CommentModel : IStoreScoped
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        [Required]
        public long OrderId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        public string? Mentions { get; set; } // Comma-separated usernames or JSON array

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual OrdersModel Order { get; set; }

        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; }
    }
}
