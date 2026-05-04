using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace Backend.Models
{
    public class ProductDTO
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? body_html { get; set; }
        public string? vendor { get; set; }
        public string? product_type { get; set; }
        public string? product_collection { get; set; }
        public int? smart_collection_id { get; set; }
        public string? handle { get; set; }
        public string? published_scope { get; set; }
        public string?tags { get; set; }
        public bool New { get; set; }
        public int? badge_id { get; set; }
        public string? status { get; set; }
        public string? product_status { get; set; }
        public string? currency { get; set; }
        public string? product_url { get; set; }
        public int? max_purchase { get; set; }
        public Boolean track_quantity { get; set; }
        public Boolean physical_product { get; set; }
        public Boolean continue_selling { get; set; }

        public List<VariantDTO>? Variants { get; set; }
        public List<OptionDTO>? Options { get; set; }

    }

    public class VariantDTO
    {
        public long Id { get; set; }
        public string? title { get; set; }
        public string? price { get; set; }
        public string? sku { get; set; }
        public int position { get; set; }
        public string? inventory_policy { get; set; }
        public string? compare_at_price { get; set; }
        public string? fulfillment_service { get; set; }
        public string? inventory_management { get; set; }
        public int inventory_quantity { get; set; }
        public int? reserved_quantity { get; set; }
        public int available_quantity { get; set; }
        public int old_inventory_quantity { get; set; }
        public string? option1 { get; set; }
        public string? option2 { get; set; }
        public string? option3 { get; set; }
        public Boolean taxable { get; set; }
        public string? barcode { get; set; }
        public int grams { get; set; }
        public long image_id { get; set; }
        public double weight { get; set; }
        public string? weight_unit { get; set; }
        public int inventory_id { get; set; }
        public Boolean requires_shipping { get; set; }
    }

    public class OptionDTO
    {
        public long id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public List<string>? product_values { get; set; }
    }

    public class ProductImagesDTO
    {
        public long? variant_id { get; set; }
        public long product_id { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int position { get; set; }
        public string alt { get; set; }
        public string src { get; set; }
    }

    public class VariantPositionUpdateDTO
    {
        public long variant_id { get; set; }
        public int position { get; set; }
    }

    public class UpdateBodyHtmlDTO
    {
        public string? body_html { get; set; }
    }

}
