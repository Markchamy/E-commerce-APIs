using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;

namespace Backend.Models
{
    public class ProductModel
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? body_html { get; set; }
        public string? Vendor { get; set; }
        public string? product_type { get; set; }
        public string? product_collection { get; set; }
        public int? smart_collection_id { get; set; }
        public DateTime created_at { get; set; }
        public string? handle { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime published_at { get; set; }
        public string? template_suffix { get; set; }
        public string? published_scope { get; set; }
        public string? Tags { get; set; }
        public bool New { get; set; }
        public int? badge_id { get; set; }
        public string? Status { get; set; }
        public string? product_status { get; set; }
        public string? currency { get; set; }
        public string? product_url { get; set; }
        public int? max_purchase { get; set; }
        public Boolean track_quantity { get; set; }
        public Boolean physical_product { get; set; }
        public Boolean continue_selling { get; set; }

        public List<VariantModel> Variants { get; set; } = new List<VariantModel>();
        public List<Options>? Options { get; set; } = new List<Options>();
        public ICollection<ProductImages> ProductImages { get; set; } = new List<ProductImages>();

        public ProductModel()
        {
            created_at = DateTime.Now;
            updated_at = DateTime.Now;
            published_at = DateTime.Now;
            //published_at = DateTime.Now;
        }
    }
    public class VariantModel
    {
        public long id { get; set; }
        public long product_id { get; set; }
        public string? title { get; set; }
        public string? price { get; set; }
        public string? sku { get; set; }
        public int? position { get; set; }
        public string? inventory_policy { get; set; }
        public string? compare_at_price { get; set; }
        public string? fulfillment_service { get; set; }
        public string? inventory_management { get; set; }
        public int inventory_quantity { get; set; }
        public int? reserved_quantity { get; set; }
        public int? old_inventory_quantity { get; set; }

        // Computed property: available quantity for customers
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int available_quantity => inventory_quantity - (reserved_quantity ?? 0);

        public string? option1 { get; set; }
        public string? option2 { get; set; }
        public string? option3 { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Boolean taxable { get; set; }
        public string? barcode { get; set; }
        public int grams { get; set; }
        public long image_id { get; set; }
        public double weight { get; set; }
        public string? weight_unit { get; set; }
        public int inventory_id { get; set; }
        public Boolean? requires_shipping { get; set; }
        public VariantModel()
        {
            created_at = DateTime.Now;
            updated_at = DateTime.Now;
        }
        public List<ProductImages> ProductImages { get; set; }
    }
    
    public class Options
    {
        public long id { get; set; }
        public long product_id { get; set; }
        public string? name { get; set; }
        public int? position { get; set; }
        public List<string>? product_values { get; set; }

    }

    public class ProductImages
    {
        public long id { get; set; }
        public long product_id { get; set; }
        [Column("variantId")]
        public long? variant_id { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int position { get; set; }
        public string? alt { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string? src { get; set; }
        public ProductImages()
        {
            created_at = DateTime.Now;
            updated_at = DateTime.Now;
        }
    }

    public class CsvProductRecord
    {
        [Name("Product ID")]
        public string? ProductID { get; set; }

        [Name("Handle")]
        public string? Handle { get; set; }

        [Name(" Title")] // Notice the leading space
        public string? Title { get; set; }

        [Name(" Body Html")]
        public string? BodyHtml { get; set; }

        [Name(" Vendor")]
        public string? Vendor { get; set; }

        [Name(" Type")]
        public string? Type { get; set; }

        [Name(" Collection")]
        public string? Collection { get; set; }

        [Name(" Tags")]
        public string? Tags { get; set; }

        [Name(" Status")]
        public string? Status { get; set; }

        [Name(" Product Status")]
        public string? ProductStatus { get; set; }

        [Name(" Option Name")]
        public string? OptionName { get; set; }

        [Name(" Option Value")]
        public List<string>? OptionValue { get; set; }

        [Name(" Variant SKU")]
        public string? VariantSKU { get; set; }

        [Name(" Variant Grams")]
        public string? VariantGrams { get; set; }

        [Name(" Variant Inventory Management")]
        public string? VariantInventoryManagement { get; set; }

        [Name(" Variant Inventory Quantity")]
        public string? VariantInventoryQuantity { get; set; }

        [Name(" Variant Inventory Policy")]
        public string? VariantInventoryPolicy { get; set; }

        [Name(" Variant Fulfillment Service")]
        public string? VariantFulfillmentService { get; set; }

        [Name(" Variant Price")]
        public string? VariantPrice { get; set; }

        [Name(" Variant Weight Unit")]
        public string? VariantWeightUnit { get; set; }

        [Name(" Image Source")]
        public string? ImageSource { get; set; }
    }
}
