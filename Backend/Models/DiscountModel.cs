using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    [Table("discount_code")]
    public class DiscountModel : IStoreScoped
    {
        public int id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;
        public int price_rule_id { get; set; }
        public string code { get; set; }
        public int usage_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public DiscountModel ()
        {
            created_at = DateTime.Now;
        }
    }

}
