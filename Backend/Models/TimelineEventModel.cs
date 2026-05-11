using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    /// <summary>
    /// Represents a timeline event that tracks changes and actions performed on an order
    /// </summary>
    [Table("timeline_events")]
    public class TimelineEventModel : IStoreScoped
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        [Required]
        public long OrderId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; }

        [Required]
        public string Description { get; set; }

        /// <summary>
        /// JSON string containing additional metadata about the event
        /// </summary>
        public string? Metadata { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual OrdersModel Order { get; set; }

        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; }
    }
}
