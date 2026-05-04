using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("price_rules")]
    public class PriceRuleModel
    {
        public int id { get; set; }
        public string value_type { get; set; }
        public string value { get; set; }
        public string customer_selection { get; set; }
        public string target_type { get; set; }
        public string target_selection { get; set; }
        public string allocation_method { get; set; }
        public int? allocation_limit { get; set; }
        public Boolean once_per_customer { get; set; }
        public int? usage_limit { get; set; }
        public DateTime starts_at { get; set; }
        public DateTime ends_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public List<string> entitled_product_ids { get; set; }
        public List<string> entitled_variant_ids { get; set; }
        public List<string> entitled_collection_ids { get; set; }
        public List<string> entitled_country_ids { get; set; }
        public List<string> prerequisite_product_ids { get; set; }
        public List<string> prerequisite_variant_ids { get; set; }
        public List<string> prerequisite_collection_ids { get; set; }
        public List<string> customer_segment_prerequisite_ids { get; set; }
        public List<string> prerequisite_customer_ids { get; set; }
        public List<string> prerequisite_subtotal_range { get; set; }
        public List<string> prerequisite_quantity_range { get; set; }
        public List<string> prerequisite_shipping_price_range { get; set; }
        public List<EntitlementQuantityModel> entitlement_quantity { get; set; }
        public List<EntitlementPurchaseModel> entitlement_purchase { get; set; }
        public string title { get; set; }
        public List<DiscountModel> discount { get; set; }
        public PriceRuleModel()
        {
            created_at = DateTime.Now;
        }
    }

    [Table("entitlement_quantity_ratio")]
    public class EntitlementQuantityModel
    {
        public int id { get; set; }
        [ForeignKey("price_rule_id")]
        public int price_rule_id { get; set; }
        public int prerequisite_quantity { get; set; }
        public int entitled_quantity { get; set; }

    }

    [Table("entitlement_purchase")]
    public class EntitlementPurchaseModel
    {
        public int id { get; set; }
        [ForeignKey("price_rule_id")]
        public int price_rule_id { get; set; }
        public string prerequisite_amount { get; set; }
    }
}
