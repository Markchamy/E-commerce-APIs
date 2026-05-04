namespace Backend.Models
{
    using CsvHelper.Configuration.Attributes;

    public class CsvProductModel
    {
        public string Handle { get; set; }
        public string Title { get; set; }

        [Name("Body (HTML)")]
        public string BodyHTML { get; set; } // Map "Body (HTML)" to BodyHTML

        public string Vendor { get; set; }

        [Name("Product Category")]
        public string ProductCategory { get; set; }

        public string Type { get; set; }
        public string Tags { get; set; }
        public string Status { get; set; }

        [Name("Option1 Name")]
        public string Option1Name { get; set; }

        [Name("Option1 Value")]
        public string Option1Value { get; set; }

        [Name("Variant SKU")]
        public string VariantSKU { get; set; }

        [Name("Variant Grams")]
        public string VariantGrams { get; set; }

        [Name("Variant Inventory Tracker")]
        public string VariantInventoryManagement { get; set; }

        [Name("Variant Inventory Qty")]
        public string VariantInventoryQuantity { get; set; }

        [Name("Variant Inventory Policy")]
        public string VariantInventoryPolicy { get; set; }

        [Name("Variant Fulfillment Service")]
        public string VariantFulfillmentService { get; set; }

        [Name("Variant Price")]
        public string VariantPrice { get; set; }

        [Name("Variant Weight Unit")]
        public string VariantWeightUnit { get; set; }

        [Name("Image Src")]
        public string ImageSource { get; set; }
    }


}
