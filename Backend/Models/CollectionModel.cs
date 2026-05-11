using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Backend.Interfaces;

namespace Backend.Models
{
    [Table("smart_collections")]
    public class CollectionModel : IStoreScoped
    {
        public int Id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;
        public string handle { get; set; }
        public string title { get; set; }
        public DateTime updated_at { get; set; }
        public string? body_html { get; set; }
        public DateTime published_at { get; set; }
        public string sort_order { get; set; }
        public string? template_suffix { get; set; }
        public bool disjunctive { get; set; }
        public string published_scope { get; set; }
        public bool menu_category { get; set; }
        public int? Layer { get; set; }
        public ICollection<RulesModel>? Rules { get; set; }  // Make navigation properties nullable
        public ICollection<CollectionImageModel>? CollectionImages { get; set; }
        public ICollection<ProductModel>? Products { get; set; }
        public CollectionModel()
        {
            updated_at = DateTime.Now;
            published_at = DateTime.Now;
        }

    }
    [Table("collection_rules")]
    public class RulesModel
    {
        public int Id { get; set; }
        public int smart_collection_id { get; set; }
        public string? column_name { get; set; }
        public string? relation { get; set; }
        public string? condition_text { get; set; }
    }
    [Table("collection_images")]
    public class CollectionImageModel
    {
        public int id { get; set; }
        public int smart_collection_id { get; set; }
        public DateTime created_at { get; set; }
        public string? alt { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string? src { get; set; }
        public CollectionImageModel()
        {
            created_at = DateTime.Now;
        }
    }
    public class CollectionProductPositionDto
    {
        public long product_id { get; set; }
        public int position { get; set; }
    }

    [Table("collection_products")]
    public class CollectionProduct
    {
        public int id { get; set; }
        public int smart_collection_id { get; set; }
        public long product_id { get; set; }
        public int position { get; set; }
    }

    [Table("smart_collections")]
    public class CollectionViewModel
    {
        public int Id { get; set; }
        public string Handle { get; set; }
        public string Title { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? BodyHtml { get; set; }
        public DateTime PublishedAt { get; set; }
        public string SortOrder { get; set; }
        public string? TemplateSuffix { get; set; }
        public bool Disjunctive { get; set; }
        public string PublishedScope { get; set; }
        public bool MenuCategory { get; set; }
        public List<RulesModel> Rules { get; set; }
        public List<CollectionImageModel> CollectionImages { get; set; }
        public List<ProductModel> Products { get; set; }
        public List<CollectionViewModel> RelatedCollections { get; set; } = new List<CollectionViewModel>();
    }

}
