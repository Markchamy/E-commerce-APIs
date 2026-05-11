using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    [Table("pmicustomers")]
    public class PmiCustomer : IStoreScoped
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        [Column("name")]
        public string? Name { get; set; }

        [Column("lastName")]
        public string? LastName { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        // Navigation property
        public virtual ICollection<PmiOrder>? Orders { get; set; }
    }
}
