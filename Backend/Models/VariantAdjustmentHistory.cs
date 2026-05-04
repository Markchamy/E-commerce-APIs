using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class VariantAdjustmentHistory
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("variant_id")]
        public long VariantId { get; set; }

        [Required]
        [Column("product_id")]
        public long ProductId { get; set; }

        // Activity Information
        [Required]
        [MaxLength(50)]
        [Column("activity_type")]
        public string ActivityType { get; set; } = string.Empty; // order_created, order_fulfilled, order_edited, order_cancelled, manual_adjustment, transfer, return

        [MaxLength(255)]
        [Column("activity_reference")]
        public string? ActivityReference { get; set; } // Order ID, Transfer ID, etc.

        [Column("activity_description", TypeName = "TEXT")]
        public string? ActivityDescription { get; set; }

        // User Information
        [Required]
        [MaxLength(255)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty; // Username or system identifier

        [Column("created_by_id")]
        public long? CreatedById { get; set; } // User ID if applicable

        // Inventory Quantities (snapshot after the change)
        [Column("unavailable")]
        public int Unavailable { get; set; } = 0;
        [Column("committed")]
        public int Committed { get; set; } = 0;
        [Column("available")]
        public int Available { get; set; } = 0;
        [Column("on_hand")]
        public int OnHand { get; set; } = 0;
        [Column("incoming")]
        public int Incoming { get; set; } = 0;

        // Change Amounts (delta values)
        [Column("unavailable_change")]
        public int UnavailableChange { get; set; } = 0;
        [Column("committed_change")]
        public int CommittedChange { get; set; } = 0;
        [Column("available_change")]
        public int AvailableChange { get; set; } = 0;
        [Column("on_hand_change")]
        public int OnHandChange { get; set; } = 0;
        [Column("incoming_change")]
        public int IncomingChange { get; set; } = 0;

        // Metadata
        [Column("notes", TypeName = "TEXT")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("VariantId")]
        public virtual VariantModel? Variant { get; set; }

        [ForeignKey("ProductId")]
        public virtual ProductModel? Product { get; set; }
    }
}
