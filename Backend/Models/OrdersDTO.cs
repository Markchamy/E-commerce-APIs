using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class OrdersDTO
    {
        [Key]
        public long orderid { get; set; }

        public long app_id { get; set; }
        public string browser_ip { get; set; }
        public bool buyer_accepts_marketing { get; set; }
        public string cancel_reason { get; set; }
        public DateTime? cancelled_at { get; set; }
        public string cart_token { get; set; }

        public long checkout_id { get; set; }
        public string checkout_token { get; set; }
        public DateTime? closed_at { get; set; }

        public string order_status { get; set; }
        public string confirmation_number { get; set; }
        public bool confirmed { get; set; }
        public string contact_email { get; set; }
        public DateTime? created_at { get; set; }
        public string currency { get; set; }

        public double current_subtotal_price { get; set; }
        public double current_total_additional_fees_set { get; set; }
        public double current_total_discounts { get; set; }
        public double current_total_duties_set { get; set; }
        public double current_total_price { get; set; }
        public double current_total_tax { get; set; }

        public string customer_local { get; set; }
        public long? device_id { get; set; }
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
        public int number { get; set; }
        public int order_number { get; set; }
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
        public string tags { get; set; }
        public bool tax_exempt { get; set; }
        public bool taxes_included { get; set; }
        public bool test { get; set; }
        public string token { get; set; }
        public double total_discounts { get; set; }
        public double total_line_items_price { get; set; }
        public double total_outstanding { get; set; }
        public double total_price { get; set; }
        public double total_taxe { get; set; }
        public double total_tip_received { get; set; }
        public int total_weight { get; set; }
        public DateTime? updated_at { get; set; }

        public long? user_id { get; set; }

        public string payment_terms { get; set; }

        public List<LineItemsDTO> LineItems { get; set; }
        public List<TaxLinesDTO> TaxLines { get; set; }
        public List<PriceSetDTO> priceSet { get; set; }
        public List<CurrentSubTotalPriceDTO> currentSubtotalPrice { get; set; }
        public List<CurrentTotalPriceDTO> currentTotalPrice { get; set; }
        public List<TotalDiscountDTO> totalDiscount { get; set; }
        public List<TotalTaxDTO> totalTax { get; set; }
        public List<TotalLineDTO> totalLine { get; set; }
        public List<TotalShippingDTO> totalShipping { get; set; }
        public List<FulfillmentDTO> fulfillment { get; set; }
        public List<DiscountCodeDTO> discount_code { get; set; }
        public List<NoteAttributesDTO> note_attributes { get; set; }
        public List<DiscountApplicationsDTO> discount_applications { get; set; }
        public List<BillingAddressDTO> billing_address { get; set; }
        public List<ClientDetailsDTO> client_details { get; set; }
        public List<ShippingAddressDTO> ShippingAddress { get; set; }
        public UserDTO? Customer { get; set; }
        public List<ShippingLineDTO> ShippingLines { get; set; }
    }

    public class LineItemsDTO
    {
        public long lineItemId { get; set; }            // Shopify line item id
        public long? refund_line_item_id { get; set; }
        public string product_fulfilled { get; set; }
        public int current_quantity { get; set; }
        public int fulfillable_quantity { get; set; }
        public string fulfillment_service { get; set; }
        public string fulfillment_status { get; set; }
        public bool gift_card { get; set; }
        public int grams { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public bool product_exists { get; set; }
        public long product_id { get; set; }
        public int quantity { get; set; }
        public bool requires_shipping { get; set; }
        public string sku { get; set; }
        public bool taxable { get; set; }
        public string title { get; set; }
        public double total_discount { get; set; }
        public long variant_id { get; set; }
        public string variant_inventory_management { get; set; }
        public string variant_title { get; set; }
        public string vendor { get; set; }
    }

    public class TaxLinesDTO
    {
        public bool channel_liable { get; set; }
        public string price { get; set; }
        public string rate { get; set; }
        public string title { get; set; }
    }

    public class PriceSetDTO
    {
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class CurrentSubTotalPriceDTO
    {
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class CurrentTotalPriceDTO
    {
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class TotalDiscountDTO
    {
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class TotalTaxDTO
    {
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class TotalLineDTO
    {
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class TotalShippingDTO
    {
        public long orderid { get; set; }
        public string shop_amount { get; set; }
        public string shop_currency_code { get; set; }
        public string presentment_amount { get; set; }
        public string presentment_currency { get; set; }
    }

    public class FulfillmentDTO
    {
        public long? id { get; set; }                   // Shopify fulfillment id (nullable => DB can auto-gen when missing)
        public long location_id { get; set; }
        public string name { get; set; }
        public string service { get; set; }
        public string shipment_status { get; set; }
        public string status { get; set; }
        public string tracking_company { get; set; }
        public string tracking_number { get; set; }
        public string tracking_url { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }

    public class DiscountCodeDTO
    {
        public string code { get; set; }
        public string amount { get; set; }
        public string type { get; set; }
    }

    public class NoteAttributesDTO
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class DiscountApplicationsDTO
    {
        public string target_type { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }
        public string allocation_method { get; set; }
        public string target_selection { get; set; }
        public string code { get; set; }
    }

    public class BillingAddressDTO
    {
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

    public class ClientDetailsDTO
    {
        public string accept_language { get; set; }
        public int browser_height { get; set; }
        public int browser_width { get; set; }
        public string browser_ip { get; set; }
        public string session_hash { get; set; }
        public string user_agent { get; set; }
    }

    public class ShippingAddressDTO
    {
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

    public class ShippingLineDTO
    {
        public string title { get; set; }
        public string price { get; set; }
    }

    public class CancelOrderDTO
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public string CancelReason { get; set; }
    }

    public class OrderCustomerDTO
    {
        public long user_id { get; set; }
        public string name { get; set; }
        public DateTime? created_at { get; set; }
        public string fulfillment_status { get; set; }
        public double total_price { get; set; }
    }

    public class fulfillmentDTO
    {
        public long orderid { get; set; }
        public string fulfillment_status { get; set; }
        public DateTime? updated_at { get; set; }
    }

    public class ShipmentStatusDTO
    {
        public long orderid { get; set; }
        public string shipment_status { get; set; }
    }

    public class BulkFulfillmentDTO
    {
        public List<long> orderids { get; set; }
        public string fulfillment_status { get; set; }
    }

    public class financialDTO
    {
        public long orderid { get; set; }
        public string financial_status { get; set; }
        public DateTime? updated_at { get; set; }
    }

    public class orderTagDTO
    {
        public long orderid { get; set; }
        public string tags { get; set; }
        public DateTime? updated_at { get; set; }
    }

    public class UpdateProductFulfilledDTO
    {
        [Required]
        public long OrderId { get; set; }
    }

    public class UpdateSpecificProductFulfilledDTO
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public List<long> LineItemIds { get; set; } = new List<long>();

        public string FulfillmentStatus { get; set; } = "Fulfilled";
    }

    public class UpdateLineItemFulfillmentDTO
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public List<long> LineItemIds { get; set; } = new List<long>();

        public string FulfillmentStatus { get; set; } = "Fulfilled";
    }
}
