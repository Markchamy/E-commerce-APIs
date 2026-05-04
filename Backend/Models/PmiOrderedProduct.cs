using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("pmiorderedproduct")]
    public class PmiOrderedProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [Column("productId")]
        public long ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual PmiOrder? Order { get; set; }
    }
}
