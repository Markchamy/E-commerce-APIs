using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace Backend.Repositories
{
    public class CollectionServicesRepository : ICollectionServices
    {
        private readonly MyDbContext _context;
        public CollectionServicesRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CollectionExist(int Id)
        {
            return await _context.Collection.AnyAsync(collection => collection.Id == Id);
        }
        public async Task<ResponseBase> AddCollection(CollectionModel collection)
        {
            try
            {
                _context.Collection.Add(collection);
                await _context.SaveChangesAsync();

                return ResponseBase.Success("Product added successfully.", collection);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure("Error adding collection: " + ex.Message);
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<CollectionModel> GetCollectionById(int id)
        {
            return await _context.Collection.FirstOrDefaultAsync(collection => collection.Id == id);
        }
        public async Task<CollectionModel> GetCollectionWithDetailsById(int smart_collection_id)
        {
            return await _context.Collection
                                 .Include(collection => collection.Rules)
                                 .Include(collection => collection.CollectionImages)
                                 .Include(collection => collection.Products)
                                    .ThenInclude(product => product.ProductImages)
                                 .FirstOrDefaultAsync(collection => collection.Id == smart_collection_id);
        }
        public async Task<CollectionModel> GetCollectionWithDetailsAndRelatedById(int smart_collection_id)
        {
            return await _context.Collection
                                 .Include(collection => collection.Rules)
                                 .Include(collection => collection.CollectionImages)
                                 .FirstOrDefaultAsync(collection => collection.Id == smart_collection_id);
        }
        public async Task<int> GetCollectionCount()
        {
            return await _context.Collection.CountAsync();
        }
        public async Task<CollectionModel> GetCollectionByIdIncludingRulesAndImages(int smart_collection_id)
        {
            return await _context.Collection
                .Include(collection => collection.Rules)
                .Include(collection => collection.CollectionImages)
                .FirstOrDefaultAsync(collection => collection.Id == smart_collection_id);
        }
        public async Task DeleteRules(IEnumerable<RulesModel> rules)
        {
            _context.Rules.RemoveRange(rules);
        }
        public async Task DeleteImages(IEnumerable<CollectionImageModel> images)
        {
            _context.CollectionImages.RemoveRange(images);
        }

        public async Task<IEnumerable<RulesModel>> GetRulesByCollectionId(int smart_collection_id)
        {
            return await _context.Rules.Where(rule => rule.smart_collection_id == smart_collection_id).ToListAsync();
        }
        public async Task<IEnumerable<CollectionImageModel>> GetImagesByCollectionId(int smart_collection_id)
        {
            return await _context.CollectionImages.Where(image => image.smart_collection_id == smart_collection_id).ToListAsync();
        }
        public async Task DeleteCollection(CollectionModel colleciton)
        {
            _context.Collection.Remove(colleciton);
            await SaveChangesAsync();
        }

        public async Task<IEnumerable<CollectionModel>> GetAllCollections(
            int page,
            int pageSize,
            string sortBy,
            string sortDirection,
            string filter,
            string search
        )
        {
            // Start the query
            IQueryable<CollectionModel> query = _context.Collection
                .Include(c => c.Rules)
                .Include(c => c.CollectionImages)
                .Include(c => c.Products);

            // 1. Apply filtering
            if (!string.IsNullOrEmpty(filter) && !filter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.title != null && c.title.Contains(filter));
            }

            // 2. Apply searching
            if (!string.IsNullOrEmpty(search))
            {
                string lowerCaseSearch = search.ToLower();
                query = query.Where(c =>
                    (c.title != null && c.title.ToLower().Contains(lowerCaseSearch)) ||
                    (c.body_html != null && c.body_html.ToLower().Contains(lowerCaseSearch)) ||
                    (c.Products.Any(p => p.Title != null && p.Title.ToLower().Contains(lowerCaseSearch)))
                );
            }

            // 3. Apply sorting
            switch (sortBy.ToLower())
            {
                case "Collection title":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(c => c.title)
                        : query.OrderBy(c => c.title);
                    break;

                case "Updated":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(c => c.updated_at)
                        : query.OrderBy(c => c.updated_at);
                    break;

                default:
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(c => c.title) // Default sorting by title
                        : query.OrderBy(c => c.title);
                    break;
            }

            // 4. Apply pagination
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            // 5. Execute query and map to CollectionModel
            var result = await query.Select(c => new CollectionModel
            {
                Id = c.Id,
                title = c.title ?? "Default Title",
                Layer = c.Layer ?? 0,
                body_html = c.body_html ?? string.Empty,
                Rules = c.Rules.Select(r => new RulesModel
                {
                    Id = r.Id,
                    column_name = r.column_name,
                    relation = r.relation,
                    condition_text = r.condition_text
                }).ToList(),
                CollectionImages = c.CollectionImages.Select(img => new CollectionImageModel
                {
                    id = img.id,
                    src = img.src ?? string.Empty,
                    alt = img.alt ?? string.Empty
                }).ToList(),
                Products = c.Products.Select(p => new ProductModel
                {
                    Id = p.Id,
                    Title = p.Title ?? "Unnamed Product",
                }).ToList()
            }).ToListAsync();

            return result;
        }

        public async Task<List<CollectionModel>> GetCollectionsByTitle(string title)
        {
            return await _context.Collection
                .Where(collection => collection.title.Trim().ToLower() == title)  // Ensure case-insensitive and trimmed match
                .ToListAsync();
        }

        public async Task<List<CollectionModel>> GetCollectionsByIdList(List<int> collectionIds)
        {
            return await _context.Collection
                .Where(collection => collectionIds.Contains(collection.Id))
                .ToListAsync();
        }


        public async Task AddRulesToCollection(int smart_collection_id, RulesModel rules)
        {
            var collection = await _context.Collection.Include(collection => collection.Rules).FirstOrDefaultAsync(collections => collections.Id == smart_collection_id);
            if (collection != null)
            {
                collection.Rules.Add(rules);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Collection not found");
            }
        }

        public async Task AddImageToCollection(int smart_collection_id, CollectionImageModel images)
        {
            var collection = await _context.Collection.Include(collection => collection.CollectionImages).FirstOrDefaultAsync(collections => collections.Id == smart_collection_id);
            if (collection != null)
            {
                collection.CollectionImages.Add(images);
                await _context.SaveChangesAsync();
            } else
            {
                throw new KeyNotFoundException("collection not found");
            }
        }

        public async Task<IEnumerable<CollectionModel>> GetAllCollection()
        {
            return await _context.Collection
                                 .Include(collection => collection.Rules)
                                 .Include(collection => collection.CollectionImages)
                                 .Include(collection => collection.Products)
                                 .Select(c => new CollectionModel
                                 {
                                     Id = c.Id,
                                     title = c.title ?? "Default Title",
                                     Layer = c.Layer ?? 0,
                                     body_html = c.body_html ?? string.Empty,
                                     Rules = c.Rules.Select(r => new RulesModel
                                     {
                                         Id = r.Id,
                                         column_name = r.column_name,
                                         condition_text = r.condition_text
                                     }).ToList(),
                                     CollectionImages = c.CollectionImages.Select(img => new CollectionImageModel
                                     {
                                         id = img.id,
                                         src = img.src ?? string.Empty,
                                         alt = img.alt ?? string.Empty
                                     }).ToList(),
                                     Products = c.Products.Select(p => new ProductModel
                                     {
                                         Id = p.Id,
                                         Title = p.Title ?? "Unnamed Product",
                                     }).ToList()
                                 })
                                 .ToListAsync();
        }

        public async Task<List<CollectionModel>> GetProductsByCollection(string title)
        {
            var allCollections = await GetAllCollection();
            return allCollections.Where(collection => collection.title.Equals(title, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<IEnumerable<ProductModel>> GetProductsByCollectionTitle(string title)
        {
            // Fetch all products and filter to handle comma-separated collections
            var allProducts = await _context.Products
                                 .Include(variant => variant.Variants)
                                 .Include(options => options.Options)
                                 .Include(image => image.ProductImages)
                                 .ToListAsync();

            // Filter products that have the specified title in their comma-separated collections
            return allProducts
                .Where(p =>
                {
                    if (string.IsNullOrWhiteSpace(p.product_collection))
                        return false;

                    // Split product collections by comma and trim whitespace
                    var productCollections = p.product_collection
                        .Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();

                    // Check if any product collection matches the title
                    return productCollections.Contains(title, StringComparer.OrdinalIgnoreCase);
                })
                .ToList();
        }

        public async Task<IEnumerable<CollectionModel>> GetMenyCategoryCollections()
        {
            return await _context.Collection.Where(collection => collection.menu_category).ToListAsync();
        }

        public async Task<Dictionary<int, CollectionModel>> GetAllCollectionsWithRulesAsDictionary()
        {
            var collections = await _context.Collection
                .Include(c => c.Rules)
                .Include(c => c.CollectionImages)
                .ToListAsync();

            return collections.ToDictionary(c => c.Id, c => c);
        }

        public async Task<(List<ProductModel> Products, int TotalCount)> GetRelatedProductsByRulesAsync(List<string> ruleTitles, int skip, int take)
        {
            // Fetch all products and filter in memory to properly handle comma-separated collections
            var allProducts = await _context.Products
                .Where(p => p.Status == "active")
                .Include(p => p.Variants)
                .Include(p => p.Options)
                .Include(p => p.ProductImages)
                .ToListAsync();

            // Filter products that have any of the rule titles in their comma-separated collections
            var filteredProducts = allProducts
                .Where(p =>
                {
                    if (string.IsNullOrWhiteSpace(p.product_collection))
                        return false;

                    // Split product collections by comma and trim whitespace
                    var productCollections = p.product_collection
                        .Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();

                    // Check if any product collection matches any rule title
                    return productCollections.Any(pc => ruleTitles.Contains(pc, StringComparer.OrdinalIgnoreCase));
                })
                .ToList();

            var totalCount = filteredProducts.Count;

            var products = filteredProducts
                .Skip(skip)
                .Take(take)
                .ToList();

            return (products, totalCount);
        }

        public async Task<(List<ProductModel> Products, int TotalCount)> GetRelatedProductsToCollections(
    int smartCollectionId, int skip, int take,
    string? productType = null,
    string? productCollection = null,
    string? vendor = null,
    string? availability = null,
    decimal? priceMin = null,
    decimal? priceMax = null,
    string? sort = null)
        {
            var ruleTitles = await _context.Rules
                .Where(r => r.smart_collection_id == smartCollectionId &&
                            (r.column_name == "product_collection" || r.column_name == "Product Tags") &&
                            (r.relation == "is equal to" || r.relation == "equals"))
                .Select(r => r.condition_text)
                .ToListAsync();

            if (!ruleTitles.Any())
                return (new List<ProductModel>(), 0);

            // Fetch all active products with their related data
            var allProducts = await _context.Products
                .Where(p => p.Status == "active")
                .Include(p => p.Variants)
                .Include(p => p.ProductImages)
                .ToListAsync();

            // Filter products that have any of the rule titles in their comma-separated collections
            var filteredProducts = allProducts
                .Where(p =>
                {
                    if (string.IsNullOrWhiteSpace(p.product_collection))
                        return false;

                    // Split product collections by comma and trim whitespace
                    var productCollections = p.product_collection
                        .Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();

                    // Check if any product collection matches any rule title (case-insensitive)
                    return productCollections.Any(pc => ruleTitles.Any(rt => rt.Equals(pc, StringComparison.OrdinalIgnoreCase)));
                })
                .ToList();

            // 🧠 Filter logic here:
            if (!string.IsNullOrEmpty(productType))
                filteredProducts = filteredProducts.Where(p => p.product_type == productType).ToList();

            if (!string.IsNullOrEmpty(productCollection))
            {
                // Handle productCollection filter for comma-separated values
                filteredProducts = filteredProducts.Where(p =>
                {
                    if (string.IsNullOrWhiteSpace(p.product_collection))
                        return false;

                    var productCollections = p.product_collection
                        .Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();
                    return productCollections.Any(pc => pc.Equals(productCollection, StringComparison.OrdinalIgnoreCase));
                }).ToList();
            }

            if (!string.IsNullOrEmpty(vendor))
                filteredProducts = filteredProducts.Where(p => p.Vendor == vendor).ToList();

            if (!string.IsNullOrEmpty(availability))
            {
                if (availability == "in")
                    filteredProducts = filteredProducts.Where(p => p.Variants != null && p.Variants.Any() && p.Variants.First().inventory_quantity > 0).ToList();
                else if (availability == "out")
                    filteredProducts = filteredProducts.Where(p => p.Variants != null && p.Variants.Any() && p.Variants.First().inventory_quantity == 0).ToList();
            }

            if (priceMin.HasValue)
                filteredProducts = filteredProducts.Where(p => p.Variants != null && p.Variants.Any() && Convert.ToDecimal(p.Variants.First().price) >= priceMin.Value).ToList();

            if (priceMax.HasValue)
                filteredProducts = filteredProducts.Where(p => p.Variants != null && p.Variants.Any() && Convert.ToDecimal(p.Variants.First().price) <= priceMax.Value).ToList();

            var totalCount = filteredProducts.Count;

            // Load positions for Featured sort
            var filteredProductIds = filteredProducts.Select(p => p.Id).ToList();
            Dictionary<long, int> positionMap;
            try
            {
                positionMap = await _context.CollectionProducts
                    .Where(cp => cp.smart_collection_id == smartCollectionId && filteredProductIds.Contains(cp.product_id))
                    .ToDictionaryAsync(cp => cp.product_id, cp => cp.position);
            }
            catch
            {
                positionMap = new Dictionary<long, int>();
            }

            // 🔀 Sorting logic
            IEnumerable<ProductModel> sortedProducts = sort switch
            {
                "Featured" => filteredProducts.OrderBy(p => positionMap.ContainsKey(p.Id) ? positionMap[p.Id] : int.MaxValue).ThenBy(p => p.Id),
                //"Best Selling" => filteredProducts.OrderByDescending(p => p.total_sales), // ✅ make sure this field exists or replace it
                "Alphabetically, A–Z" => filteredProducts.OrderBy(p => p.Title),
                "Alphabetically, Z–A" => filteredProducts.OrderByDescending(p => p.Title),
                "Price, low to high" => filteredProducts.Where(p => p.Variants != null && p.Variants.Any()).OrderBy(p => Convert.ToDecimal(p.Variants.First().price)),
                "Price, high to low" => filteredProducts.Where(p => p.Variants != null && p.Variants.Any()).OrderByDescending(p => Convert.ToDecimal(p.Variants.First().price)),
                "Date, old to new" => filteredProducts.OrderBy(p => p.created_at),
                "Date, new to old" => filteredProducts.OrderByDescending(p => p.created_at),
                _ => filteredProducts.OrderBy(p => p.Id)
            };


            var products = sortedProducts
                .Skip(skip)
                .Take(take)
                .ToList();


            return (products, totalCount);
        }

        public async Task UpsertCollectionProductPositions(int smartCollectionId, List<(long productId, int position)> positions)
        {
            var productIds = positions.Select(p => p.productId).ToList();

            var existing = await _context.CollectionProducts
                .Where(cp => cp.smart_collection_id == smartCollectionId && productIds.Contains(cp.product_id))
                .ToListAsync();

            foreach (var (productId, position) in positions)
            {
                var existing_entry = existing.FirstOrDefault(cp => cp.product_id == productId);
                if (existing_entry != null)
                {
                    existing_entry.position = position;
                }
                else
                {
                    _context.CollectionProducts.Add(new CollectionProduct
                    {
                        smart_collection_id = smartCollectionId,
                        product_id = productId,
                        position = position
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

    }
}
