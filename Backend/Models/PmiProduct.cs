using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    [Table("pmiproducts")]
    public class PmiProduct : IStoreScoped
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        [Column("name")]
        public string? Name { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }
    }
}
