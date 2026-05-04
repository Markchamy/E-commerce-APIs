using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("pmiorderedmachines")]
    public class PmiOrderedMachine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [Column("serialNum")]
        public string? SerialNum { get; set; }

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual PmiOrder? Order { get; set; }
    }
}
