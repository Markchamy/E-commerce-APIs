using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class InventoryServicesRepository : IInventoryServices
    {
        private readonly MyDbContext _context;
        public InventoryServicesRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<InventorySortByModel>> GetAllSortingAsync()
        {
            return await _context.inventory_sort_by.ToListAsync();
        }

        public async Task<IEnumerable<ProductModel>> GetAllInventoryWithDetails(
             int page = 1,
             int pageSize = 50,
             string sortBy = "Product title",
             string sortDirection = "desc",
             string filter = "All",
             string search = ""
         )
        {
            var skip = (page - 1) * pageSize;

            // Base query with required navigation properties
            IQueryable<ProductModel> query = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Options)
                .Include(p => p.ProductImages)
                .AsNoTracking()
                .AsSplitQuery();

            // 1. Apply filtering
            if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
            {
                switch (filter.ToLower())
                {
                    case "Incoming":
                        break;

                    case "IQOS":
                        query = query.Where(p => p.product_type == "IQOS");
                        break;

                    // Add more custom filters as needed
                    default:
                        query = query.Where(_ => false); // No results for unknown filters
                        break;
                }
            }

            // 2. Apply searching
            if (!string.IsNullOrEmpty(search))
            {
                string lowerCaseSearch = search.ToLower();

                query = query.Where(p =>
                    (p.Title != null && p.Title.ToLower().Contains(lowerCaseSearch)) ||
                    (p.Status != null && p.Status.ToLower().Contains(lowerCaseSearch)) ||
                    (p.product_type != null && p.product_type.ToLower().Contains(lowerCaseSearch)) ||
                    (p.Vendor != null && p.Vendor.ToLower().Contains(lowerCaseSearch)) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(lowerCaseSearch))
                );
            }

            // 3. Apply sorting
            switch (sortBy?.ToLower())
            {
                case "Product title":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Title)
                        : query.OrderBy(p => p.Title);
                    break;

                case "SKU":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Variants.FirstOrDefault().sku)
                        : query.OrderBy(p => p.Variants.FirstOrDefault().sku);
                    break;

                case "Incoming":
                    break;

                case "Unavailable":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Variants.Where(v => v.inventory_quantity == 0).Sum(v => v.inventory_quantity))
                        : query.OrderBy(p => p.Variants.Where(v => v.inventory_quantity == 0).Sum(v => v.inventory_quantity));
                    break;

                case "Committed":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Variants.Sum(v => v.old_inventory_quantity - v.inventory_quantity))
                        : query.OrderBy(p => p.Variants.Sum(v => v.old_inventory_quantity - v.inventory_quantity));
                    break;

                case "Available":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Variants.Where(v => v.inventory_quantity > 0).Sum(v => v.inventory_quantity))
                        : query.OrderBy(p => p.Variants.Where(v => v.inventory_quantity > 0).Sum(v => v.inventory_quantity));
                    break;

                case "On hand":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Variants.FirstOrDefault().old_inventory_quantity)
                        : query.OrderBy(p => p.Variants.FirstOrDefault().old_inventory_quantity);
                    break;

                default:
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Id)
                        : query.OrderBy(p => p.Id);
                    break;
            }

            // 4. Apply pagination
            query = query.Skip(skip).Take(pageSize);

            // 5. Execute the query and map to ProductModel
            return await query.Select(product => new ProductModel
            {
                Id = product.Id,
                Title = product.Title ?? string.Empty,
                body_html = product.body_html ?? string.Empty,
                Vendor = product.Vendor ?? string.Empty,
                product_type = product.product_type ?? string.Empty,
                product_collection = product.product_collection ?? string.Empty,
                smart_collection_id = product.smart_collection_id,
                handle = product.handle ?? string.Empty,
                Tags = product.Tags,
                New = product.New,
                Status = product.Status,
                product_status = product.product_status,
                currency = product.currency,
                product_url = product.product_url,
                max_purchase = product.max_purchase,
                track_quantity = product.track_quantity,
                physical_product = product.physical_product,
                continue_selling = product.continue_selling,
                Variants = product.Variants.Select(variant => new VariantModel
                {
                    id = variant.id,
                    title = variant.title ?? string.Empty,
                    price = variant.price,
                    sku = variant.sku ?? string.Empty,
                    position = variant.position,
                    inventory_policy = variant.inventory_policy,
                    compare_at_price = variant.compare_at_price,
                    fulfillment_service = variant.fulfillment_service,
                    inventory_management = variant.inventory_management,
                    inventory_quantity = variant.inventory_quantity,
                    old_inventory_quantity = variant.old_inventory_quantity,
                    option1 = variant.option1,
                    option2 = variant.option2,
                    option3 = variant.option3,
                    barcode = variant.barcode,
                    grams = variant.grams,
                    weight = variant.weight,
                    weight_unit = variant.weight_unit,
                }).ToList(),
                Options = product.Options.Select(option => new Options
                {
                    id = option.id,
                    name = option.name ?? string.Empty
                }).ToList(),
                ProductImages = product.ProductImages.Select(image => new ProductImages
                {
                    id = image.id,
                    src = image.src ?? string.Empty
                }).ToList()
            }).ToListAsync();
        }

        public async Task<UpdateInventoryBySkuResponse> UpdateInventoryBySkuAsync(UpdateInventoryBySkuDTO request)
        {
            var response = new UpdateInventoryBySkuResponse
            {
                TotalItems = request.Items.Count
            };

            foreach (var item in request.Items)
            {
                // Skip items with empty or null SKUs
                if (string.IsNullOrWhiteSpace(item.Sku))
                {
                    response.SkippedCount++;
                    response.SkippedSkus.Add(item.Sku ?? "(empty)");
                    continue;
                }

                // Find the variant by SKU
                var variant = await _context.Variants
                    .Include(v => v.ProductImages)
                    .FirstOrDefaultAsync(v => v.sku == item.Sku);

                if (variant == null)
                {
                    response.NotFoundCount++;
                    response.NotFoundSkus.Add(item.Sku);
                    continue;
                }

                // Get product information for the response
                var product = await _context.Products
                    .Where(p => p.Id == variant.product_id)
                    .Select(p => new { p.Id, p.Title })
                    .FirstOrDefaultAsync();

                // Store old values for transaction log
                int oldInventory = variant.inventory_quantity;
                int oldReserved = variant.reserved_quantity ?? 0;
                int oldAvailable = oldInventory - oldReserved;

                // CRITICAL: Save current inventory to old_inventory_quantity BEFORE updating
                variant.old_inventory_quantity = variant.inventory_quantity;

                // Update inventory_quantity with value from Brains
                variant.inventory_quantity = item.Quantity;

                // IMPORTANT: DO NOT touch reserved_quantity - it's managed by order system only!

                variant.updated_at = DateTime.UtcNow;

                // Calculate new available quantity
                int newAvailable = variant.inventory_quantity - oldReserved;

                // Log transaction for audit trail
                var transaction = new InventoryTransactionLog
                {
                    variant_id = variant.id,
                    transaction_type = "BRAINS_SYNC",
                    quantity_change = item.Quantity - oldInventory,
                    inventory_before = oldInventory,
                    inventory_after = variant.inventory_quantity,
                    reserved_before = oldReserved,
                    reserved_after = oldReserved, // Unchanged
                    reason = "Brains Sync",
                    performed_by = "BrainsSyncService",
                    created_at = DateTime.UtcNow
                };
                _context.InventoryTransactionLog.Add(transaction);

                // Check for reconciliation issues (negative available quantity)
                if (newAvailable < 0)
                {
                    // Log warning but continue - this needs manual review
                    Console.WriteLine($"⚠️ RECONCILIATION ISSUE: Variant {variant.id} SKU {variant.sku} has negative available quantity! " +
                        $"Inventory={variant.inventory_quantity}, Reserved={oldReserved}, Available={newAvailable}");
                }

                response.UpdatedCount++;
                response.UpdatedVariants.Add(new UpdatedVariantInfo
                {
                    VariantId = variant.id,
                    ProductId = variant.product_id,
                    ProductTitle = product?.Title ?? string.Empty,
                    VariantTitle = variant.title ?? string.Empty,
                    Sku = variant.sku ?? string.Empty,
                    OldQuantity = oldInventory,
                    NewQuantity = item.Quantity
                });
            }

            // Save all changes to the database
            if (response.UpdatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return response;
        }
    }
}
