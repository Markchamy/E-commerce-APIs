using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class OrdersModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]   // Shopify order id
        public long orderid { get; set; }

        public long app_id { get; set; }
        public string browser_ip { get; set; }
        public bool buyer_accepts_marketing { get; set; }
        public string cancel_reason { get; set; }
        public DateTime? cancelled_at { get; set; }
        public string cart_token { get; set; }
        public long checkout_id { get; set; }
        public string checkout_token { get; set; }

        public List<ClientDetailsModel> client_details { get; set; }
        public DateTime? closed_at { get; set; }
        public string confirmation_number { get; set; }
        public bool confirmed { get; set; }
        public string contact_email { get; set; }
        public DateTime? created_at { get; set; }
        public string currency { get; set; }

        public double current_subtotal_price { get; set; }
        public List<SubTotalPriceModel> subtotal_price_set { get; set; }
        public double current_total_additional_fees_set { get; set; }
        public double current_total_discounts { get; set; }
        public List<TotalDiscountModel> TotalDiscount { get; set; }
        public double current_total_duties_set { get; set; }
        public double current_total_price { get; set; }
        public List<CurrentTotalPriceModel> CurrentTotalPrice { get; set; }
        public double current_total_tax { get; set; }
        public List<TotalTaxModel> TotalTax { get; set; }

        public string customer_local { get; set; }
        public long? device_id { get; set; }
        public List<DiscountCodeModel> discount_code { get; set; }
        public string email { get; set; }
        public bool estimated_taxes { get; set; }
        public string financial_status { get; set; }
        public string fulfillment_status { get; set; }
        public string landing_site { get; set; }
        public string landing_site_ref { get; set; }
        public long? location_id { get; set; }
        public string merchant_of_record_app_id { get; set; }
        public string name { get; set; }
        public string note { get; set; }
        public List<NoteAttributesModel> note_attributes { get; set; }
        public int number { get; set; }
        public int order_number { get; set; }
        public string order_status { get; set; }
        public string original_total_additional_fees_set { get; set; }
        public string original_total_duties_set { get; set; }
        public List<string> payment_gatewaynames { get; set; }
        public string phone { get; set; }
        public string po_number { get; set; }
        public string presentment_currency { get; set; }
        public DateTime? processed_at { get; set; }
        public string reference { get; set; }
        public string referring_site { get; set; }
        public string source_identifier { get; set; }
        public string source_name { get; set; }
        public string source_url { get; set; }
        public double subtotal_price { get; set; }
        public List<PriceSetModel> priceSet { get; set; }
        public string tags { get; set; }
        public bool tax_exempt { get; set; }
        public List<TaxLinesModel> taxLines { get; set; }
        public bool taxes_included { get; set; }
        public bool test { get; set; }
        public string token { get; set; }
        public double total_discounts { get; set; }
        public double total_line_items_price { get; set; }
        public List<TotalLineModel> LineModels { get; set; }
        public double total_outstanding { get; set; }
        public double total_price { get; set; }
        public List<TotalShippingModel> totalShipping { get; set; }
        public double total_taxe { get; set; }
        public double total_tip_received { get; set; }
        public int total_weight { get; set; }
        public DateTime? updated_at { get; set; }
        public long? user_id { get; set; }
        public List<BillingAddressModel> billing_address { get; set; }

        [ForeignKey("user_id")]
        public UserModel? Customer { get; set; }

        public List<DiscountApplicationsModel> discount_applications { get; set; }
        public List<FulfillmentsModel> fulfillment { get; set; }
        public List<LineItemsModel> LineItems { get; set; }
        public string payment_terms { get; set; }
        public List<ShippingAddressModel> ShippingAddress { get; set; }
        public List<ShippingLineModel> ShippingLines { get; set; }
        public ICollection<RefundModel> Refunds { get; set; }
        public List<TransactionsModel> transactions { get; set; }
        public ICollection<CommentModel> Comments { get; set; }
    }

    public class LineItemsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]   // Shopify line item id
        public long lineItemId { get; set; }

        public long orderid { get; set; }
        public long? refund_line_id { get; set; }
        public string? product_fulfilled { get; set; }
        public int current_quantity { get; set; }
        public int fulfillable_quantity { get; set; }
        public string? fulfillment_service { get; set; }
        public string? fulfillment_status { get; set; }
        public bool gift_card { get; set; }
        public int grams { get; set; }
        public string? name { get; set; }
        public string? price { get; set; }
        public bool product_exists { get; set; }
        public long product_id { get; set; }
        public int quantity { get; set; }
        public bool requires_shipping { get; set; }
        public string? sku { get; set; }
        public bool taxable { get; set; }
        public string? title { get; set; }
        public double total_discount { get; set; }
        public long variant_id { get; set; }
        public string? variant_inventory_management { get; set; }
        public string? variant_title { get; set; }
        public string? vendor { get; set; }

        public ICollection<RefundLineItemsModel> RefundLineItems { get; set; }
    }

    public class TaxLinesModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public bool channel_liable { get; set; }
        public string price { get; set; }
        public string rate { get; set; }
        public string title { get; set; }
    }

    [Table("price_set")]
    public class PriceSetModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    [Table("current_subtotal_price")]
    public class SubTotalPriceModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    [Table("current_total_price_set")]
    public class CurrentTotalPriceModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    [Table("total_discount_set")]
    public class TotalDiscountModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    [Table("total_tax_set")]
    public class TotalTaxModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    [Table("total_line_items_price_set")]
    public class TotalLineModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    [Table("total_shipping_price_set")]
    public class TotalShippingModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class FulfillmentsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   // make DB-generated; if you need Shopify id, add a separate field
        public long id { get; set; }

        public long orderid { get; set; }
        public DateTime? created_at { get; set; }
        public long? location_id { get; set; }
        public string? name { get; set; }
        public string? service { get; set; }
        public string? shipment_status { get; set; }
        public string? status { get; set; }
        public string? tracking_company { get; set; }
        public string? tracking_number { get; set; }
        public string? tracking_url { get; set; }
        public DateTime? updated_at { get; set; }
    }

    [Table("discount_codes")]
    public class DiscountCodeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string code { get; set; }
        public string amount { get; set; }
        public string type { get; set; }
    }

    public class NoteAttributesModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

    public class DiscountApplicationsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string target_type { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }
        public string allocation_method { get; set; }
        public string target_selection { get; set; }
        public string code { get; set; }
    }

    [Table("billing_address")]
    public class BillingAddressModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string first_name { get; set; }
        public string address1 { get; set; }
        public string phone { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string last_name { get; set; }
        public string address2 { get; set; }
        public string company { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
        public string province_code { get; set; }
    }

    public class ClientDetailsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string accept_language { get; set; }
        public int browser_height { get; set; }
        public int browser_width { get; set; }
        public string browser_ip { get; set; }
        public string session_hash { get; set; }
        public string user_agent { get; set; }
    }

    [Table("shipping_address")]
    public class ShippingAddressModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string first_name { get; set; }
        public string address1 { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string last_name { get; set; }
        public string address2 { get; set; }
        public string company { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
        public string province_code { get; set; }
    }

    [Table("shipping_lines")]
    public class ShippingLineModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public long orderid { get; set; }
        public string title { get; set; }
        public string price { get; set; }
    }
}
