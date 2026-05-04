using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("inventory_transaction_log")]
    public class InventoryTransactionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long variant_id { get; set; }

        [Required]
        [MaxLength(50)]
        public string transaction_type { get; set; } // BRAINS_SYNC, ORDER_RESERVE, FULFILLMENT, CANCELLATION, ADJUSTMENT

        public int quantity_change { get; set; }

        public int inventory_before { get; set; }
        public int inventory_after { get; set; }

        public int reserved_before { get; set; }
        public int reserved_after { get; set; }

        [MaxLength(500)]
        public string? reason { get; set; }

        public long? order_id { get; set; }
        public long? line_item_id { get; set; }

        [Required]
        [MaxLength(100)]
        public string performed_by { get; set; }

        public DateTime created_at { get; set; }

        [Column(TypeName = "text")]
        public string? additional_data { get; set; }

        public InventoryTransactionLog()
        {
            created_at = DateTime.UtcNow;
        }
    }
}
