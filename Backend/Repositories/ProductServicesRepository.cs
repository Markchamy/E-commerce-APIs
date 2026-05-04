using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ProductServicesRepository : IProductServices
    {
        private readonly MyDbContext _context;

        public ProductServicesRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ProductExists(long Id)
        {
            return await _context.Products.AnyAsync(products => products.Id == Id);
        }

        public async Task<bool> ProductTitleExists(string title)
        {
            return await _context.Products.AnyAsync(p => p.Title.ToLower() == title.ToLower());
        }

        public async Task<ResponseBase> AddProduct(ProductModel product)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return ResponseBase.Success("Product added successfully", product);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure("Error adding product: " + ex.Message);
            }
        }
        public async Task<ProductModel> GetProductById(long id)
        {
            return await _context.Products.FirstOrDefaultAsync(product => product.Id == id);
        }
        public async Task DeleteProduct(ProductModel product)
        {
            _context.Products.Remove(product);
            await SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<VariantModel>> GetVariantsByProductId(long productId)
        {
            return await _context.Variants.Where(variant => variant.product_id == productId).ToListAsync();
        }

        public async Task<IEnumerable<Options>> GetOptionsByProductId(long productId)
        {
            return await _context.Options.Where(option => option.product_id == productId).ToListAsync();
        }

        public async Task DeleteVariants(IEnumerable<VariantModel> variants)
        {
            _context.Variants.RemoveRange(variants);
        }

        public async Task DeleteImages(IEnumerable<ProductImages> images)
        {
            _context.ProductImages.RemoveRange(images);
        }
        public async Task DeleteOptions(IEnumerable<Options> options)
        {
            _context.Options.RemoveRange(options);
        }
        public async Task<int> GetProductsCount()
        {
            return await _context.Products.CountAsync();
        }
        public async Task<ProductModel> GetProductsByTitleAsync(string title)
        {
            return await _context.Products
                            .Include(p => p.Variants)
                            .Include(p => p.ProductImages)
                            .Include(p => p.Options)
                            .Where(p => p.Title == title)
                            .AsNoTracking()
                            .AsSplitQuery()
                            .FirstOrDefaultAsync();
        }

        public async Task<ProductModel> GetProductWithDetailsById(long id)
        {
            return await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Options)
                .Include(p => p.ProductImages)
                .Where(p => p.Id == id)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProductModel>> GetAllProductsWithDetails(
     int page = 1,
     int pageSize = 50,
     string sortBy = "Created",
     string sortDirection = "desc",
     string filter = "All",
     string search = ""
 )
        {
            var skip = (page - 1) * pageSize;
            var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

            // Special handling for inventory sorting - requires different query strategy
            if (sortBy?.Equals("inventory", StringComparison.OrdinalIgnoreCase) == true)
            {
                return await GetProductsWithDetailsSortedByInventory(page, pageSize, isDescending, filter, search);
            }

            // Base query without eager loading for initial filtering/sorting
            IQueryable<ProductModel> query = _context.Products.AsNoTracking();

            // 1. Apply filtering
            query = ApplyProductFilters(query, filter);

            // 2. Apply searching using EF.Functions.Like for better performance
            if (!string.IsNullOrEmpty(search))
            {
                var searchPattern = $"%{search}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Title, searchPattern) ||
                    EF.Functions.Like(p.Status, searchPattern) ||
                    EF.Functions.Like(p.product_type, searchPattern) ||
                    EF.Functions.Like(p.Vendor, searchPattern) ||
                    EF.Functions.Like(p.Tags, searchPattern)
                );
            }

            // 3. Apply sorting with null handling
            query = ApplyProductSorting(query, sortBy, isDescending);

            // 4. Get product IDs with pagination (efficient - no joins yet)
            var productIds = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => p.Id)
                .ToListAsync();

            // 5. Load products with related data only for the paginated results
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Variants)
                .Include(p => p.Options)
                .Include(p => p.ProductImages)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            // 6. Maintain the original sort order from the query
            var productDict = products.ToDictionary(p => p.Id);
            return productIds.Select(id => productDict[id]).ToList();
        }

        private async Task<IEnumerable<ProductModel>> GetProductsWithDetailsSortedByInventory(
            int page,
            int pageSize,
            bool isDescending,
            string filter,
            string search)
        {
            var skip = (page - 1) * pageSize;

            // Create a subquery that calculates total inventory per product
            var productsWithInventory = _context.Products
                .AsNoTracking()
                .Select(p => new
                {
                    Product = p,
                    TotalInventory = p.Variants.Sum(v => (int?)v.inventory_quantity) ?? 0
                });

            // Apply filters
            if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
            {
                productsWithInventory = filter.ToLower() switch
                {
                    "active" => productsWithInventory.Where(x => x.Product.product_status == "active"),
                    "draft" => productsWithInventory.Where(x => x.Product.product_status == "draft"),
                    "archived" => productsWithInventory.Where(x => x.Product.product_status == "Archived"),
                    "pmi" => productsWithInventory.Where(x => x.Product.Tags == "PMI"),
                    _ => productsWithInventory.Where(x => false)
                };
            }

            // Apply search
            if (!string.IsNullOrEmpty(search))
            {
                var searchPattern = $"%{search}%";
                productsWithInventory = productsWithInventory.Where(x =>
                    EF.Functions.Like(x.Product.Title, searchPattern) ||
                    EF.Functions.Like(x.Product.Status, searchPattern) ||
                    EF.Functions.Like(x.Product.product_type, searchPattern) ||
                    EF.Functions.Like(x.Product.Vendor, searchPattern) ||
                    EF.Functions.Like(x.Product.Tags, searchPattern)
                );
            }

            // Sort by inventory
            productsWithInventory = isDescending
                ? productsWithInventory.OrderByDescending(x => x.TotalInventory)
                : productsWithInventory.OrderBy(x => x.TotalInventory);

            // Get paginated product IDs
            var productIds = await productsWithInventory
                .Skip(skip)
                .Take(pageSize)
                .Select(x => x.Product.Id)
                .ToListAsync();

            // Load full product details with related data
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Variants)
                .Include(p => p.Options)
                .Include(p => p.ProductImages)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            // Maintain sort order
            var productDict = products.ToDictionary(p => p.Id);
            return productIds.Select(id => productDict[id]).ToList();
        }

        private IQueryable<ProductModel> ApplyProductFilters(IQueryable<ProductModel> query, string filter)
        {
            if (string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }

            return filter.ToLower() switch
            {
                "active" => query.Where(p => p.product_status == "active"),
                "draft" => query.Where(p => p.product_status == "draft"),
                "archived" => query.Where(p => p.product_status == "Archived"),
                "pmi" => query.Where(p => p.Tags == "PMI"),
                _ => query.Where(_ => false)
            };
        }

        private IQueryable<ProductModel> ApplyProductSorting(IQueryable<ProductModel> query, string sortBy, bool isDescending)
        {
            return sortBy?.ToLower() switch
            {
                "product title" => isDescending
                    ? query.OrderByDescending(p => p.Title ?? "")
                    : query.OrderBy(p => p.Title ?? ""),

                "created" => isDescending
                    ? query.OrderByDescending(p => p.created_at)
                    : query.OrderBy(p => p.created_at),

                "updated" => isDescending
                    ? query.OrderByDescending(p => p.updated_at)
                    : query.OrderBy(p => p.updated_at),

                "product type" => isDescending
                    ? query.OrderByDescending(p => p.product_type ?? "")
                    : query.OrderBy(p => p.product_type ?? ""),

                "vendor" => isDescending
                    ? query.OrderByDescending(p => p.Vendor ?? "")
                    : query.OrderBy(p => p.Vendor ?? ""),

                _ => isDescending
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id)
            };
        }

        public async Task<IEnumerable<ProductModel>> GetAllProducts()
        {
            return await _context.Products
                                 .Include(product => product.Variants)
                                 .Include(product => product.Options)
                                 .Include(product => product.ProductImages)
                                 .ToListAsync();
        }

        public async Task<ProductModel> GetProductByIdIncludingVariantsAndOptions(long productId)
        {
            // Load the product without the related collections
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return null;
            }

            // Load related collections separately
            product.Variants = await _context.Variants
                .Where(v => v.product_id == productId)
                .ToListAsync();

            product.Options = await _context.Options
                .Where(o => o.product_id == productId)
                .ToListAsync();

            product.ProductImages = await _context.ProductImages
                .Where(pi => pi.product_id == productId)
                .ToListAsync();

            return product;
        }
        public async Task AddImageToProduct(long productId, long variantId, ProductImages image)
        {
            var product = await _context.Products.Include(product => product.ProductImages).FirstOrDefaultAsync(product => product.Id == productId);
            var variant = await _context.Variants.Include(variants => variants.ProductImages).FirstOrDefaultAsync(variant => variant.id == variantId);
            
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }
            if (variant == null)
            {
                throw new KeyNotFoundException("Variant not found");
            }


            product.ProductImages.Add(image);
            variant.ProductImages.Add(image);
            await _context.SaveChangesAsync();

            variant.image_id = image.id;

            _context.Update(variant);
            await _context.SaveChangesAsync();
        }

        public async Task AddImage(long productId, ProductImages image)
        {
            var product = await _context.Products.Include(product => product.ProductImages).FirstOrDefaultAsync(product => product.Id == productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Product Not Found");
            }

            product.ProductImages.Add(image);

            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductImages>> GetImagesByProductId(long productId)
        {
            return await _context.ProductImages
                                 .Where(image => image.product_id == productId)
                                 .ToListAsync();
        }
        public async Task<ProductImages> GetImageByProductIdAndImageId(long productId, long imageId)
        {
            return await _context.ProductImages
                                 .FirstOrDefaultAsync(image => image.product_id == productId && image.id == imageId);
        }
        public async Task<int> GetImagesCountByProductId(long productId)
        {
            return await _context.ProductImages
                                 .Where(image => image.product_id == productId)
                                 .CountAsync();
        }
        public async Task<bool> UpdateImage(long productId, long imageId, ProductImagesDTO updatedImage)
        {
            var image = await _context.ProductImages
                                      .FirstOrDefaultAsync(image => image.id == imageId && image.product_id == productId);
            if (image == null)
            {
                return false;
            }
            image.width = updatedImage.width;
            image.height = updatedImage.height;
            image.src = updatedImage.src;
            image.alt = updatedImage.alt;

            image.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateImagePosition(long productId, long imageId, int position)
        {
            var image = await _context.ProductImages
                                      .FirstOrDefaultAsync(img => img.id == imageId && img.product_id == productId);
            if (image == null)
                return false;

            image.position = position;
            image.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteImage(long productId, long imageId)
        {
            var image = await _context.ProductImages
                                      .FirstOrDefaultAsync(image => image.id == imageId && image.product_id== productId);
            if (image == null)
            {
                return false;
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddVariantToProduct(long productId, VariantModel variant)
        {
            var product = await _context.Products.Include(product => product.Variants).FirstOrDefaultAsync(product => product.Id == productId);
            if (product != null)
            {
                product.Variants.Add(variant);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Product not found");
            }
        }
        public async Task<List<VariantModel>> GetVariantByProductId(long productId)
        {
            return await _context.Variants
                                 .Where(variant => variant.product_id == productId)
                                 .ToListAsync();
        }
        public async Task<VariantModel> GetVariantByProductIdAndVariantId(long productId, long variantId)
        {
            return await _context.Variants
                                 .FirstOrDefaultAsync(variant => variant.product_id == productId && variant.id == variantId);
        }

        public async Task<int> GetVariantsCountByProductId(long productId)
        {
            return await _context.Variants
                                 .Where(variant => variant.product_id == productId)
                                 .CountAsync();
        }

        public async Task<bool> UpdateVariant(long productId, long variantId, VariantDTO variantDTO)
        {
            var variant = await _context.Variants
                                      .FirstOrDefaultAsync(variant => variant.id == variantId && variant.product_id == productId);
            if (variant == null)
            {
                return false;
            }
            variant.title = variantDTO.title;
            variant.price = variantDTO.price;
            variant.sku = variantDTO.sku;
            variant.position = variantDTO.position;
            variant.inventory_policy = variantDTO.inventory_policy;
            variant.compare_at_price = variantDTO.compare_at_price;
            variant.fulfillment_service = variantDTO.fulfillment_service;
            variant.inventory_management = variantDTO.inventory_management;
            variant.inventory_quantity = variantDTO.inventory_quantity;
            variant.option1 = variantDTO.option1;
            variant.taxable = variantDTO.taxable;
            variant.barcode = variantDTO.barcode;
            variant.grams = variantDTO.grams;
            variant.image_id = variantDTO.image_id;
            variant.weight = variantDTO.weight;
            variant.weight_unit = variantDTO.weight_unit;
            variant.inventory_id = variantDTO.inventory_id;
            variant.requires_shipping = variantDTO.requires_shipping;

            variant.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVariant(long productId, long variantId)
        {
            var variant = await _context.Variants
                                      .FirstOrDefaultAsync(variant => variant.id == variantId && variant.product_id == productId);
            if (variant == null)
            {
                return false;
            }

            _context.Variants.Remove(variant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateVariantPositions(long productId, List<VariantPositionUpdateDTO> variantPositions)
        {
            if (variantPositions == null || variantPositions.Count == 0)
            {
                return false;
            }

            // Get all variant IDs to update
            var variantIds = variantPositions.Select(v => v.variant_id).ToList();

            // Fetch all variants that belong to the specified product
            var variants = await _context.Variants
                                         .Where(v => v.product_id == productId && variantIds.Contains(v.id))
                                         .ToListAsync();

            if (variants == null || variants.Count == 0)
            {
                return false;
            }

            // Update positions
            foreach (var variantPosition in variantPositions)
            {
                var variant = variants.FirstOrDefault(v => v.id == variantPosition.variant_id);
                if (variant != null)
                {
                    variant.position = variantPosition.position;
                    variant.updated_at = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CollectionModel> GetCollectionByTitle(string title)
        {
            return await _context.Collection
                                  .FirstOrDefaultAsync(collection => collection.title== title);
        }

        public async Task ImportProductsAsync(IEnumerable<ProductModel> products)
        {
            // Optional: Use a transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var product in products)
                {
                    // Check if the product already exists by its unique identifier
                    var existingProduct = await _context.Products
                        .Include(product => product.Variants)
                        .Include(product => product.Options)
                        .Include(product => product.ProductImages)
                        .FirstOrDefaultAsync(p => p.Id == product.Id);

                    if (existingProduct != null)
                    {
                        // Update existing product details if necessary
                        _context.Entry(existingProduct).CurrentValues.SetValues(product);
                    }
                    else
                    {
                        // Add new product
                        await _context.Products.AddAsync(product);
                    }
                }

                // Save changes and commit the transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync();
                Console.WriteLine($"Error importing products: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductModel>> GetProductsByIds(IEnumerable<long> productIds)
        {
            return await _context.Products
                                 .Where(product => productIds.Contains(product.Id))
                                 .Include(product => product.Variants)
                                 .Include(product => product.Options)
                                 .Include(product => product.ProductImages)
                                 .ToListAsync();
        }

        public async Task<List<ProductModel>> GetProductsWhereNewIsTrue()
        {
            return await _context.Products
                .Where(p => p.New)
                .Include(p => p.Variants)
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModel>> SearchProductsAsync(string query)
        {
            Console.WriteLine($"Search query received: {query}");

            var products = await _context.Products
                .Where(product => product.Title.ToLower().Contains(query.ToLower()) || product.product_collection.ToLower().Contains(query.ToLower()))
                .Include(product => product.Variants)
                .Include(product => product.ProductImages)
                .ToListAsync();

            Console.WriteLine($"Number of products found: {products.Count}");

            return products;
        }

        public async Task ImportShopifyProductsAsync(List<CsvProductModel> products)
        {
            var dbProducts = new List<ProductModel>();

            foreach (var csvProduct in products)
            {
                // Map CsvProductModel to ProductModel or other entities
                var product = new ProductModel
                {
                    handle = csvProduct.Handle,
                    Title = csvProduct.Title,
                    body_html = csvProduct.BodyHTML,
                    Vendor = csvProduct.Vendor,
                    product_type = csvProduct.Type,
                    Tags = csvProduct.Tags,
                    Status = csvProduct.Status,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    Variants = new List<VariantModel>
            {
                new VariantModel
                {
                    sku = csvProduct.VariantSKU,
                    
                    // Safely parse VariantGrams, defaulting to 0 if parsing fails
                    grams = int.TryParse(csvProduct.VariantGrams, out var grams) ? grams : 0,

                    inventory_management = csvProduct.VariantInventoryManagement,
                    
                    // Safely parse VariantInventoryQuantity
                    inventory_quantity = int.TryParse(csvProduct.VariantInventoryQuantity, out var qty) ? qty : 0,

                    inventory_policy = csvProduct.VariantInventoryPolicy,
                    fulfillment_service = csvProduct.VariantFulfillmentService,
                    price = csvProduct.VariantPrice,
                    weight_unit = csvProduct.VariantWeightUnit,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            },
                    ProductImages = new List<ProductImages>
            {
                new ProductImages
                {
                    src = csvProduct.ImageSource,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                }
            }
                };

                dbProducts.Add(product);
            }

            await _context.Set<ProductModel>().AddRangeAsync(dbProducts);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductModel>> GetProductsWithSkuAsync(int page = 1, int limit = 250)
        {
            // Calculate skip value for pagination
            var skip = (page - 1) * limit;

            return await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.ProductImages)
                .Include(p => p.Options)
                .Where(p => p.Variants.Any(v => !string.IsNullOrEmpty(v.sku)))
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(limit)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();
        }

    }
}
