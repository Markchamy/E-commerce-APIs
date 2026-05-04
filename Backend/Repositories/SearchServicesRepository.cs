using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Repositories
{
    public class SearchServicesRepository : ISearchServices
    {
        private readonly IServiceProvider _serviceProvider;

        public SearchServicesRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<GlobalSearchResponse> GlobalSearchAsync(GlobalSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return new GlobalSearchResponse
                {
                    Query = request.Query,
                    TotalResults = 0,
                    Page = request.Page,
                    Limit = request.Limit,
                    Results = new SearchResults
                    {
                        Products = new ProductSearchResults { Count = 0, Items = new List<ProductSearchItem>() },
                        Orders = new OrderSearchResults { Count = 0, Items = new List<OrderSearchItem>() },
                        Collections = new CollectionSearchResults { Count = 0, Items = new List<CollectionSearchItem>() },
                        Users = new UserSearchResults { Count = 0, Items = new List<UserSearchItem>() }
                    }
                };
            }

            var searchTerm = request.Query.ToLower().Trim();

            // Determine which entities to search based on filters
            var filters = request.Filters?.Select(f => f.ToLower()).ToList() ?? new List<string>();
            var searchAll = filters.Count == 0;
            var searchProducts = searchAll || filters.Contains("products");
            var searchOrders = searchAll || filters.Contains("orders");
            var searchCollections = searchAll || filters.Contains("collections");
            var searchCustomers = searchAll || filters.Contains("customers");

            // Execute searches in parallel with separate DbContext instances to avoid threading issues
            // Only search the entity types specified in filters
            var tasks = new List<Task>();

            Task<ProductSearchResults> productsTask = null;
            if (searchProducts)
            {
                productsTask = Task.Run(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        return await SearchProductsAsync(context, searchTerm, request.Limit);
                    }
                });
                tasks.Add(productsTask);
            }

            Task<OrderSearchResults> ordersTask = null;
            if (searchOrders)
            {
                ordersTask = Task.Run(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        return await SearchOrdersAsync(context, searchTerm, request.Limit);
                    }
                });
                tasks.Add(ordersTask);
            }

            Task<CollectionSearchResults> collectionsTask = null;
            if (searchCollections)
            {
                collectionsTask = Task.Run(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        return await SearchCollectionsAsync(context, searchTerm, request.Limit);
                    }
                });
                tasks.Add(collectionsTask);
            }

            Task<UserSearchResults> usersTask = null;
            if (searchCustomers)
            {
                usersTask = Task.Run(async () =>
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                        return await SearchUsersAsync(context, searchTerm, request.Limit);
                    }
                });
                tasks.Add(usersTask);
            }

            // Wait for all active search tasks to complete
            await Task.WhenAll(tasks);

            var results = new SearchResults
            {
                Products = searchProducts ? await productsTask : new ProductSearchResults { Count = 0, Items = new List<ProductSearchItem>() },
                Orders = searchOrders ? await ordersTask : new OrderSearchResults { Count = 0, Items = new List<OrderSearchItem>() },
                Collections = searchCollections ? await collectionsTask : new CollectionSearchResults { Count = 0, Items = new List<CollectionSearchItem>() },
                Users = searchCustomers ? await usersTask : new UserSearchResults { Count = 0, Items = new List<UserSearchItem>() }
            };

            var totalResults = results.Products.Count + results.Orders.Count +
                             results.Collections.Count + results.Users.Count;

            return new GlobalSearchResponse
            {
                Query = request.Query,
                TotalResults = totalResults,
                Page = request.Page,
                Limit = request.Limit,
                Results = results
            };
        }

        private async Task<ProductSearchResults> SearchProductsAsync(MyDbContext context, string searchTerm, int limit)
        {
            // Use LIKE pattern for case-insensitive search
            var likePattern = $"%{searchTerm}%";

            var products = await context.Products
                .AsNoTracking() // Performance: Don't track entities since we're read-only
                .Where(p =>
                    (p.Title != null && EF.Functions.Like(p.Title.ToLower(), likePattern)) ||
                    (p.body_html != null && EF.Functions.Like(p.body_html.ToLower(), likePattern)) ||
                    (p.Vendor != null && EF.Functions.Like(p.Vendor.ToLower(), likePattern)) ||
                    (p.product_type != null && EF.Functions.Like(p.product_type.ToLower(), likePattern)) ||
                    (p.product_collection != null && EF.Functions.Like(p.product_collection.ToLower(), likePattern)) ||
                    (p.Tags != null && EF.Functions.Like(p.Tags.ToLower(), likePattern)) ||
                    (p.handle != null && EF.Functions.Like(p.handle.ToLower(), likePattern)) ||
                    p.Id.ToString().Contains(searchTerm) ||
                    p.Variants.Any(v =>
                        (v.sku != null && EF.Functions.Like(v.sku.ToLower(), likePattern)) ||
                        (v.barcode != null && EF.Functions.Like(v.barcode.ToLower(), likePattern)) ||
                        (v.title != null && EF.Functions.Like(v.title.ToLower(), likePattern))
                    )
                )
                .OrderByDescending(p => p.created_at)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.product_type,
                    p.Vendor,
                    p.Status,
                    p.Tags,
                    p.handle,
                    p.created_at,
                    p.body_html,
                    p.product_collection,
                    ImageUrl = p.ProductImages.FirstOrDefault() != null ? p.ProductImages.FirstOrDefault().src : null,
                    Variants = p.Variants.Select(v => new { v.sku, v.barcode, v.title }).ToList()
                })
                .ToListAsync();

            var items = products.Select(p => new ProductSearchItem
            {
                Id = p.Id,
                Title = p.Title,
                ProductType = p.product_type,
                Vendor = p.Vendor,
                Status = p.Status,
                Tags = p.Tags,
                Handle = p.handle,
                CreatedAt = p.created_at,
                ImageUrl = p.ImageUrl,
                MatchedFields = GetProductMatchedFieldsOptimized(p.Title, p.body_html, p.Vendor, p.product_type,
                    p.product_collection, p.Tags, p.handle, p.Id.ToString(), p.Variants, searchTerm)
            }).ToList();

            return new ProductSearchResults
            {
                Count = items.Count,
                Items = items
            };
        }

        private async Task<OrderSearchResults> SearchOrdersAsync(MyDbContext context, string searchTerm, int limit)
        {
            var likePattern = $"%{searchTerm}%";

            var orders = await context.Orders
                .AsNoTracking() // Performance: Don't track entities since we're read-only
                .Where(o =>
                    o.orderid.ToString().Contains(searchTerm) ||
                    EF.Functions.Like(o.name.ToLower(), likePattern) ||
                    EF.Functions.Like(o.email.ToLower(), likePattern) ||
                    EF.Functions.Like(o.contact_email.ToLower(), likePattern) ||
                    o.order_number.ToString().Contains(searchTerm) ||
                    EF.Functions.Like(o.confirmation_number.ToLower(), likePattern) ||
                    EF.Functions.Like(o.phone, likePattern) ||
                    EF.Functions.Like(o.financial_status.ToLower(), likePattern) ||
                    EF.Functions.Like(o.fulfillment_status.ToLower(), likePattern) ||
                    EF.Functions.Like(o.tags.ToLower(), likePattern) ||
                    (o.Customer != null && (
                        EF.Functions.Like(o.Customer.first_name.ToLower(), likePattern) ||
                        EF.Functions.Like(o.Customer.last_name.ToLower(), likePattern)
                    )) ||
                    o.billing_address.Any(b =>
                        EF.Functions.Like(b.first_name.ToLower(), likePattern) ||
                        EF.Functions.Like(b.last_name.ToLower(), likePattern) ||
                        EF.Functions.Like(b.city.ToLower(), likePattern)
                    )
                )
                .OrderByDescending(o => o.created_at)
                .Take(limit)
                .Select(o => new
                {
                    o.orderid,
                    o.order_number,
                    o.name,
                    o.email,
                    o.contact_email,
                    o.confirmation_number,
                    o.phone,
                    o.financial_status,
                    o.fulfillment_status,
                    o.total_price,
                    o.currency,
                    o.created_at,
                    o.tags,
                    CustomerFirstName = o.Customer != null ? o.Customer.first_name : null,
                    CustomerLastName = o.Customer != null ? o.Customer.last_name : null,
                    BillingFirstName = o.billing_address.FirstOrDefault() != null ? o.billing_address.FirstOrDefault().first_name : null,
                    BillingLastName = o.billing_address.FirstOrDefault() != null ? o.billing_address.FirstOrDefault().last_name : null
                })
                .ToListAsync();

            var items = orders.Select(o => new OrderSearchItem
            {
                OrderId = o.orderid,
                OrderNumber = o.order_number.ToString(),
                Name = o.name,
                Email = o.email,
                FinancialStatus = o.financial_status,
                FulfillmentStatus = o.fulfillment_status,
                TotalPrice = o.total_price,
                Currency = o.currency,
                CreatedAt = o.created_at,
                CustomerName = !string.IsNullOrEmpty(o.CustomerFirstName) ? $"{o.CustomerFirstName} {o.CustomerLastName}" :
                              !string.IsNullOrEmpty(o.BillingFirstName) ? $"{o.BillingFirstName} {o.BillingLastName}" : null,
                MatchedFields = GetOrderMatchedFieldsOptimized(o.orderid.ToString(), o.name, o.email, o.order_number.ToString(),
                    o.confirmation_number, o.phone, o.financial_status, o.fulfillment_status, o.tags, searchTerm)
            }).ToList();

            return new OrderSearchResults
            {
                Count = items.Count,
                Items = items
            };
        }

        private async Task<CollectionSearchResults> SearchCollectionsAsync(MyDbContext context, string searchTerm, int limit)
        {
            var likePattern = $"%{searchTerm}%";

            var collections = await context.Collection
                .AsNoTracking() // Performance: Don't track entities since we're read-only
                .Where(c =>
                    EF.Functions.Like(c.title.ToLower(), likePattern) ||
                    EF.Functions.Like(c.handle.ToLower(), likePattern) ||
                    EF.Functions.Like(c.body_html.ToLower(), likePattern) ||
                    c.Id.ToString().Contains(searchTerm)
                )
                .OrderByDescending(c => c.updated_at)
                .Take(limit)
                .Select(c => new
                {
                    c.Id,
                    c.title,
                    c.handle,
                    c.body_html,
                    c.menu_category,
                    c.Layer,
                    c.updated_at,
                    ImageUrl = c.CollectionImages.FirstOrDefault() != null ? c.CollectionImages.FirstOrDefault().src : null
                })
                .ToListAsync();

            var items = collections.Select(c => new CollectionSearchItem
            {
                Id = c.Id,
                Title = c.title,
                Handle = c.handle,
                BodyHtml = c.body_html,
                MenuCategory = c.menu_category,
                Layer = c.Layer,
                UpdatedAt = c.updated_at,
                ImageUrl = c.ImageUrl,
                MatchedFields = GetCollectionMatchedFieldsOptimized(c.title, c.handle, c.body_html, c.Id.ToString(), searchTerm)
            }).ToList();

            return new CollectionSearchResults
            {
                Count = items.Count,
                Items = items
            };
        }

        private async Task<UserSearchResults> SearchUsersAsync(MyDbContext context, string searchTerm, int limit)
        {
            var likePattern = $"%{searchTerm}%";

            var users = await context.Users
                .AsNoTracking() // Performance: Don't track entities since we're read-only
                .Where(u =>
                    EF.Functions.Like(u.first_name.ToLower(), likePattern) ||
                    EF.Functions.Like(u.last_name.ToLower(), likePattern) ||
                    EF.Functions.Like(u.email.ToLower(), likePattern) ||
                    EF.Functions.Like(u.phone_number, likePattern) ||
                    EF.Functions.Like(u.role.ToLower(), likePattern) ||
                    u.Id.ToString().Contains(searchTerm)
                )
                .OrderByDescending(u => u.Id)
                .Take(limit)
                .Select(u => new
                {
                    u.Id,
                    u.first_name,
                    u.last_name,
                    u.email,
                    u.phone_number,
                    u.role
                })
                .ToListAsync();

            var items = users.Select(u => new UserSearchItem
            {
                Id = u.Id,
                FirstName = u.first_name,
                LastName = u.last_name,
                Email = u.email,
                PhoneNumber = u.phone_number,
                Role = u.role,
                MatchedFields = GetUserMatchedFieldsOptimized(u.first_name, u.last_name, u.email, u.phone_number, u.role, u.Id.ToString(), searchTerm)
            }).ToList();

            return new UserSearchResults
            {
                Count = items.Count,
                Items = items
            };
        }

        // Optimized helper methods to identify which fields matched the search (in-memory processing)
        private List<string> GetProductMatchedFieldsOptimized(string title, string bodyHtml, string vendor,
            string productType, string productCollection, string tags, string handle, string id,
            IEnumerable<dynamic> variants, string searchTerm)
        {
            var matchedFields = new List<string>();

            if (!string.IsNullOrEmpty(title) && title.ToLower().Contains(searchTerm))
                matchedFields.Add("Title");
            if (!string.IsNullOrEmpty(bodyHtml) && bodyHtml.ToLower().Contains(searchTerm))
                matchedFields.Add("Description");
            if (!string.IsNullOrEmpty(vendor) && vendor.ToLower().Contains(searchTerm))
                matchedFields.Add("Vendor");
            if (!string.IsNullOrEmpty(productType) && productType.ToLower().Contains(searchTerm))
                matchedFields.Add("Product Type");
            if (!string.IsNullOrEmpty(productCollection) && productCollection.ToLower().Contains(searchTerm))
                matchedFields.Add("Collection");
            if (!string.IsNullOrEmpty(tags) && tags.ToLower().Contains(searchTerm))
                matchedFields.Add("Tags");
            if (!string.IsNullOrEmpty(handle) && handle.ToLower().Contains(searchTerm))
                matchedFields.Add("Handle");
            if (id.Contains(searchTerm))
                matchedFields.Add("ID");
            if (variants != null && variants.Any(v =>
                (!string.IsNullOrEmpty(v.sku) && v.sku.ToLower().Contains(searchTerm)) ||
                (!string.IsNullOrEmpty(v.barcode) && v.barcode.ToLower().Contains(searchTerm)) ||
                (!string.IsNullOrEmpty(v.title) && v.title.ToLower().Contains(searchTerm))))
                matchedFields.Add("Variant");

            return matchedFields;
        }

        private List<string> GetOrderMatchedFieldsOptimized(string orderId, string name, string email,
            string orderNumber, string confirmationNumber, string phone, string financialStatus,
            string fulfillmentStatus, string tags, string searchTerm)
        {
            var matchedFields = new List<string>();

            if (orderId.Contains(searchTerm))
                matchedFields.Add("Order ID");
            if (!string.IsNullOrEmpty(name) && name.ToLower().Contains(searchTerm))
                matchedFields.Add("Order Name");
            if (!string.IsNullOrEmpty(email) && email.ToLower().Contains(searchTerm))
                matchedFields.Add("Email");
            if (orderNumber.Contains(searchTerm))
                matchedFields.Add("Order Number");
            if (!string.IsNullOrEmpty(confirmationNumber) && confirmationNumber.ToLower().Contains(searchTerm))
                matchedFields.Add("Confirmation Number");
            if (!string.IsNullOrEmpty(phone) && phone.Contains(searchTerm))
                matchedFields.Add("Phone");
            if (!string.IsNullOrEmpty(financialStatus) && financialStatus.ToLower().Contains(searchTerm))
                matchedFields.Add("Financial Status");
            if (!string.IsNullOrEmpty(fulfillmentStatus) && fulfillmentStatus.ToLower().Contains(searchTerm))
                matchedFields.Add("Fulfillment Status");
            if (!string.IsNullOrEmpty(tags) && tags.ToLower().Contains(searchTerm))
                matchedFields.Add("Tags");

            return matchedFields;
        }

        private List<string> GetCollectionMatchedFieldsOptimized(string title, string handle, string bodyHtml,
            string id, string searchTerm)
        {
            var matchedFields = new List<string>();

            if (!string.IsNullOrEmpty(title) && title.ToLower().Contains(searchTerm))
                matchedFields.Add("Title");
            if (!string.IsNullOrEmpty(handle) && handle.ToLower().Contains(searchTerm))
                matchedFields.Add("Handle");
            if (!string.IsNullOrEmpty(bodyHtml) && bodyHtml.ToLower().Contains(searchTerm))
                matchedFields.Add("Description");
            if (id.Contains(searchTerm))
                matchedFields.Add("ID");

            return matchedFields;
        }

        private List<string> GetUserMatchedFieldsOptimized(string firstName, string lastName, string email,
            string phoneNumber, string role, string id, string searchTerm)
        {
            var matchedFields = new List<string>();

            if (!string.IsNullOrEmpty(firstName) && firstName.ToLower().Contains(searchTerm))
                matchedFields.Add("First Name");
            if (!string.IsNullOrEmpty(lastName) && lastName.ToLower().Contains(searchTerm))
                matchedFields.Add("Last Name");
            if (!string.IsNullOrEmpty(email) && email.ToLower().Contains(searchTerm))
                matchedFields.Add("Email");
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Contains(searchTerm))
                matchedFields.Add("Phone Number");
            if (!string.IsNullOrEmpty(role) && role.ToLower().Contains(searchTerm))
                matchedFields.Add("Role");
            if (id.Contains(searchTerm))
                matchedFields.Add("ID");

            return matchedFields;
        }
    }
}
