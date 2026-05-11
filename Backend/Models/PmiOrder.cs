using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    [Table("pmiorders")]
    public class PmiOrder : IStoreScoped
    {
        [Key]
        [Column("orderReference")]
        public string OrderReference { get; set; } = string.Empty;

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        [Column("orderNumber")]
        public string? OrderNumber { get; set; }

        [Column("dateDelivered")]
        public DateTime? DateDelivered { get; set; }

        [Column("customerId")]
        public long? CustomerId { get; set; }

        [Column("dateCreated")]
        public DateTime? DateCreated { get; set; }

        [Column("errorId")]
        public long? ErrorId { get; set; }

        [Column("anonymous")]
        public bool? Anonymous { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual PmiCustomer? Customer { get; set; }

        [ForeignKey("ErrorId")]
        public virtual PmiError? Error { get; set; }

        public virtual ICollection<PmiOrderedProduct>? OrderedProducts { get; set; }

        public virtual ICollection<PmiOrderedMachine>? OrderedMachines { get; set; }
    }
}
