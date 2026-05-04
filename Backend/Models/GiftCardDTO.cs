namespace Backend.Models
{
    public class GiftCardDTO
    {
        public string balance { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string currency { get; set; }
        public string initial_value { get; set; }
        public DateTime? disabled_at { get; set; }
        public int line_item_id { get; set; }
        public int user_id { get; set; }
        public int customer_id { get; set; }
        public string note { get; set; }
        public DateTime expires_on { get; set; }
        public string template_suffix { get; set; }
        public string last_characters { get; set; }
        public int order_id { get; set; }
        public string code { get; set; }
    }
}
