namespace Backend.Models
{
    public class GlobalSearchRequest
    {
        public string Query { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        /// <summary>
        /// Optional filters to limit search scope. Supported values: "products", "collections", "orders", "customers"
        /// If null or empty, searches all entity types
        /// </summary>
        public List<string> Filters { get; set; }
    }

    public class GlobalSearchResponse
    {
        public string Query { get; set; }
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public SearchResults Results { get; set; }
    }

    public class SearchResults
    {
        public ProductSearchResults Products { get; set; }
        public OrderSearchResults Orders { get; set; }
        public CollectionSearchResults Collections { get; set; }
        public UserSearchResults Users { get; set; }
    }

    public class ProductSearchResults
    {
        public int Count { get; set; }
        public List<ProductSearchItem> Items { get; set; }
    }

    public class OrderSearchResults
    {
        public int Count { get; set; }
        public List<OrderSearchItem> Items { get; set; }
    }

    public class CollectionSearchResults
    {
        public int Count { get; set; }
        public List<CollectionSearchItem> Items { get; set; }
    }

    public class UserSearchResults
    {
        public int Count { get; set; }
        public List<UserSearchItem> Items { get; set; }
    }

    public class ProductSearchItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string ProductType { get; set; }
        public string Vendor { get; set; }
        public string Status { get; set; }
        public string Tags { get; set; }
        public string Handle { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; }
        public List<string> MatchedFields { get; set; }
    }

    public class OrderSearchItem
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string FinancialStatus { get; set; }
        public string FulfillmentStatus { get; set; }
        public double TotalPrice { get; set; }
        public string Currency { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public List<string> MatchedFields { get; set; }
    }

    public class CollectionSearchItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Handle { get; set; }
        public string BodyHtml { get; set; }
        public bool MenuCategory { get; set; }
        public int? Layer { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ImageUrl { get; set; }
        public List<string> MatchedFields { get; set; }
    }

    public class UserSearchItem
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public List<string> MatchedFields { get; set; }
    }
}
