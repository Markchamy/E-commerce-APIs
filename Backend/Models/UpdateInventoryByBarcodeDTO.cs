using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    /// <summary>
    /// DTO for updating inventory quantities based on SKU
    /// </summary>
    public class UpdateInventoryBySkuDTO
    {
        [Required]
        public List<SkuInventoryItem> Items { get; set; } = new List<SkuInventoryItem>();
    }

    public class SkuInventoryItem
    {
        [Required]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Response DTO for SKU inventory update operation
    /// </summary>
    public class UpdateInventoryBySkuResponse
    {
        public int TotalItems { get; set; }
        public int UpdatedCount { get; set; }
        public int SkippedCount { get; set; }
        public int NotFoundCount { get; set; }
        public List<string> SkippedSkus { get; set; } = new List<string>();
        public List<string> NotFoundSkus { get; set; } = new List<string>();
        public List<UpdatedVariantInfo> UpdatedVariants { get; set; } = new List<UpdatedVariantInfo>();
    }

    public class UpdatedVariantInfo
    {
        public long VariantId { get; set; }
        public long ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string VariantTitle { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
    }
}
