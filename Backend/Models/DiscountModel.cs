using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("discount_code")]
    public class DiscountModel
    {
        public int id { get; set; }
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
