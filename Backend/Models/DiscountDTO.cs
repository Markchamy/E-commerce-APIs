namespace Backend.Models
{
    public class DiscountDTO
    {
        public int price_rule_id { get; set; }
        public string code { get; set; }
        public int usage_count { get; set; }
        public DateTime updated_at { get; set; }
    }
}
