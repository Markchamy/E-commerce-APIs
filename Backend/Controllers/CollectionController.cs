using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionServices _collectionRepository;
        private readonly IConfiguration _configuration;

        public CollectionController(ICollectionServices collectionRepository, IConfiguration configuration)
        {
            _collectionRepository = collectionRepository;
            _configuration = configuration;
        }

        [HttpPost("create/collection")]
        public async Task<IActionResult> CreateCollection([FromBody] CollectionDTO collectionDTO)
        {
            try
            {
                // Log the incoming data for debugging
                Console.WriteLine("Incoming Data: ", collectionDTO);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if the collection already exists
                var collectionExist = await _collectionRepository.CollectionExist(collectionDTO.Id);
                if (collectionExist)
                {
                    return BadRequest("This collection already exists");
                }

                // Create the collection
                var collection = new CollectionModel
                {
                    handle = collectionDTO.handle,
                    title = collectionDTO.title,
                    body_html = collectionDTO.body_html,
                    sort_order = collectionDTO.sort_order,
                    template_suffix = collectionDTO.template_suffix,
                    disjunctive = collectionDTO.disjunctive,
                    published_scope = collectionDTO.published_scope,
                    menu_category = collectionDTO.menu_category,
                    Layer = collectionDTO.Layer,
                    Rules = collectionDTO.Rules.Select(rules => new RulesModel
                    {
                        column_name = rules.column_name,
                        relation = rules.relation,
                        condition_text = rules.condition_text,
                    }).ToList()
                };

                // Log the collection data before adding to the DB
                Console.WriteLine("Saving Collection: ", collection);

                // Add the collection to the repository and save
                await _collectionRepository.AddCollection(collection);
                await _collectionRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Collection created successfully.", collection));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, new { message = $"An internal error occurred: {innerMessage}" });
            }
        }

        [HttpPost("smart_collections/{smart_collection_id}/images")]
        public async Task<IActionResult> CreateCollectionImage(int smart_collection_id, IFormFile imageFile)
        {
            try
            {
                // Check if the image file is provided
                if (imageFile == null || imageFile.Length == 0)
                    return BadRequest(new ResponseBase(false, "No image file provided."));

                // Set up the path to save the uploaded image file
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Generate a unique file name for the image
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save the image file to the file system
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Create a new CollectionImageModel instance with image details
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
                var image = new CollectionImageModel
                {
                    src = $"{baseUrl}/uploads/products/{fileName}",
                    //width = width,
                    //height = height,
                    //alt = alt,
                };

                // Add the image to the collection in the database
                await _collectionRepository.AddImageToCollection(smart_collection_id, image);

                await _collectionRepository.SaveChangesAsync();

                // Return success response
                return Ok(new ResponseBase(true, "Image added to the collection successfully.", image));
            }
            catch (Exception ex)
            {
                // Return a server error if something goes wrong
                return StatusCode(500, new ResponseBase(false, $"An error occurred while adding the image to the collection: {ex.Message}"));
            }
        }

        [HttpPost("smart_collection/{smart_collection_id}/rule")]
        public async Task<IActionResult> CreateRulesCollection([FromBody] RuleDTO rules, int smart_collection_id)
        {
            try
            {
                var rule = new RulesModel
                {
                    column_name = rules.column_name,
                    relation = rules.relation,
                    condition_text = rules.condition_text,
                };
                await _collectionRepository.AddRulesToCollection(smart_collection_id, rule);
                return Ok(new ResponseBase(true, "rules added to the collection successfully.", rule));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while trying to add the rules to the collections."));
            }
        }

        [HttpGet("smart_collections")]
        public async Task<IActionResult> GetAllCollections(
                int page = 1,                  // Current page for pagination
                int pageSize = 50,             // Number of collections per page
                string sortBy = "Collection title",        // Default sort column
                string sortDirection = "desc",  // Default sort direction
                string filter = "All",            // Filtering criteria
                string search = ""             // Search term
            )
        {
            try
            {
                // Validate pagination parameters
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new ResponseBase(false, "Page and pageSize must be greater than 0."));
                }

                // Fetch the collections with sorting, filtering, and searching
                var collections = await _collectionRepository.GetAllCollections(page, pageSize, sortBy, sortDirection, filter, search);

                if (collections == null || !collections.Any())
                {
                    return Ok(new List<object>());
                }

                return Ok(new ResponseBase(true, "Collections retrieved successfully.", collections));
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"Error: {ex.Message}, StackTrace: {ex.StackTrace}");

                return StatusCode(500, ResponseBase.Failure("An internal error occurred while retrieving the collections."));
            }
        }


        [HttpGet("smart_collection/{smart_collection_id}")]
        public async Task<IActionResult> GetCollectionWihDetails(int smart_collection_id)
        {
            try
            {
                var collection = await _collectionRepository.GetCollectionWithDetailsById(smart_collection_id);
                if(collection == null)
                {
                    return NotFound($"Collection with ID {smart_collection_id} not found.");
                }

                return Ok(new ResponseBase(true, "Collection retrieved successfully.", collection));

            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An internal error occured while retrieving the collection."));
            }
        }

        [HttpGet("smart_collection/title/{title}")]
        public async Task<IActionResult> GetCollectionByTitle(string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return BadRequest(new ResponseBase(false, "Collection title cannot be empty."));
                }

                // Get collections by title (may return multiple if titles aren't unique)
                var collections = await _collectionRepository.GetCollectionsByTitle(title.Trim().ToLower());

                if (collections == null || !collections.Any())
                {
                    return NotFound(new ResponseBase(false, $"Collection with title '{title}' not found."));
                }

                // Get the first matching collection
                var collectionId = collections.First().Id;

                // Get full collection details (same as GetCollectionWihDetails)
                var collection = await _collectionRepository.GetCollectionWithDetailsById(collectionId);

                if (collection == null)
                {
                    return NotFound(new ResponseBase(false, $"Collection with title '{title}' not found."));
                }

                return Ok(new ResponseBase(true, "Collection retrieved successfully.", collection));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseBase(false, $"An internal error occurred while retrieving the collection: {ex.Message}"));
            }
        }

        [HttpGet("smart_collection/{smartCollectionId}/related_products")]
        public async Task<IActionResult> GetRelatedProducts(
    int smartCollectionId,
    int page = 1,
    int pageSize = 16,
    string? productType = null,
    string? productCollection = null,
    string? vendor = null,
    string? availability = null,
    decimal? priceMin = null,
    decimal? priceMax = null,
    string? sort = null
)
        {
            var skip = (page - 1) * pageSize;

            var (products, totalCount) = await _collectionRepository.GetRelatedProductsToCollections(
                smartCollectionId, skip, pageSize,
                productType, productCollection, vendor, availability, priceMin, priceMax , sort
            );

            var response = new
            {
                Products = products.Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.badge_id,
                    p.Vendor,
                    p.product_collection,
                    p.product_type,
                    p.Status,
                    VariantId = p.Variants?.FirstOrDefault()?.id ?? 0,
                    Price = p.Variants?.FirstOrDefault()?.price ?? "0",
                    CompareAtPrice = p.Variants?.FirstOrDefault()?.compare_at_price ?? "0",
                    InventoryQuantity = p.Variants?.FirstOrDefault()?.inventory_quantity ?? 0,
                    ImageId = (p.ProductImages?.FirstOrDefault(img => img.position == 1) ?? p.ProductImages?.FirstOrDefault())?.id ?? 0,
                    ImageSrc = (p.ProductImages?.FirstOrDefault(img => img.position == 1) ?? p.ProductImages?.FirstOrDefault())?.src ?? "No image available"
                })
            };

            return Ok(new ResponseBase(true, "Filtered products retrieved successfully.", response));
        }

        [HttpGet("smart_collection/title/{title}/related_products")]
        public async Task<IActionResult> GetRelatedProductsByTitle(
            string title,
            int page = 1,
            int pageSize = 16,
            string? productType = null,
            string? productCollection = null,
            string? vendor = null,
            string? availability = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            string? sort = null
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return BadRequest(new ResponseBase(false, "Collection title cannot be empty."));
                }

                // Get collections by title
                var collections = await _collectionRepository.GetCollectionsByTitle(title.Trim().ToLower());

                if (collections == null || !collections.Any())
                {
                    return NotFound(new ResponseBase(false, $"Collection with title '{title}' not found."));
                }

                // Get the first matching collection's ID
                var smartCollectionId = collections.First().Id;

                // Use the existing logic to get related products
                var skip = (page - 1) * pageSize;

                var (products, totalCount) = await _collectionRepository.GetRelatedProductsToCollections(
                    smartCollectionId, skip, pageSize,
                    productType, productCollection, vendor, availability, priceMin, priceMax, sort
                );

                var response = new
                {
                    Products = products.Select(p => new
                    {
                        p.Id,
                        p.Title,
                        p.badge_id,
                        p.Vendor,
                        p.product_collection,
                        p.product_type,
                        p.Status,
                        VariantId = p.Variants?.FirstOrDefault()?.id ?? 0,
                        Price = p.Variants?.FirstOrDefault()?.price ?? "0",
                        CompareAtPrice = p.Variants?.FirstOrDefault()?.compare_at_price ?? "0",
                        InventoryQuantity = p.Variants?.FirstOrDefault()?.inventory_quantity ?? 0,
                        ImageId = (p.ProductImages?.FirstOrDefault(img => img.position == 1) ?? p.ProductImages?.FirstOrDefault())?.id ?? 0,
                        ImageSrc = (p.ProductImages?.FirstOrDefault(img => img.position == 1) ?? p.ProductImages?.FirstOrDefault())?.src ?? "No image available"
                    })
                };

                return Ok(new ResponseBase(true, "Filtered products retrieved successfully.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseBase(false, $"An internal error occurred: {ex.Message}"));
            }
        }


        [HttpGet("menu_category_and_related_collections")]
        public async Task<IActionResult> GetMenuCategoryAndRelatedCollections()
        {
            try
            {
                // OPTIMIZATION: Load ALL collections with rules in a SINGLE query
                var allCollectionsDict = await _collectionRepository.GetAllCollectionsWithRulesAsDictionary();

                // Get menu category collections (already in memory, no additional query)
                var menuCategoryCollections = allCollectionsDict.Values
                    .Where(c => c.menu_category)
                    .ToList();

                if (!menuCategoryCollections.Any())
                {
                    return NotFound("No menu category collections found.");
                }

                // Build a title-to-collections lookup for O(1) access
                var collectionsByTitle = allCollectionsDict.Values
                    .Where(c => !string.IsNullOrWhiteSpace(c.title))
                    .GroupBy(c => c.title!.Trim().ToLower())
                    .ToDictionary(g => g.Key, g => g.ToList());

                // In-memory recursive function (no more database calls!)
                Dictionary<int, object> GetRelatedCollectionsWithDepth(string columnName, string conditionText, HashSet<int> processedIds)
                {
                    var relatedCollections = new Dictionary<int, object>();
                    List<CollectionModel> collections = new List<CollectionModel>();

                    // Resolve collections from in-memory dictionary
                    if (columnName == "Collection Title" || columnName == "collection_title")
                    {
                        var normalizedTitle = conditionText?.Trim().ToLower();
                        if (!string.IsNullOrEmpty(normalizedTitle) && collectionsByTitle.ContainsKey(normalizedTitle))
                        {
                            collections = collectionsByTitle[normalizedTitle];
                        }
                    }
                    else if (columnName == "Collection ID" || columnName == "collection_id")
                    {
                        var collectionIds = conditionText
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList();

                        collections = collectionIds
                            .Where(id => allCollectionsDict.ContainsKey(id))
                            .Select(id => allCollectionsDict[id])
                            .ToList();
                    }

                    foreach (var collection in collections)
                    {
                        if (processedIds.Contains(collection.Id))
                        {
                            continue; // Skip duplicates
                        }

                        processedIds.Add(collection.Id);

                        // Process nested rules (all data already in memory)
                        var nestedRelatedCollectionsDict = new Dictionary<int, object>();

                        var nestedRules = collection.Rules?
                            .Where(rule =>
                                (rule.column_name == "Collection Title" || rule.column_name == "collection_title" ||
                                 rule.column_name == "Collection ID" || rule.column_name == "collection_id") &&
                                (rule.relation == "is equal to" || rule.relation == "equals"))
                            .ToList() ?? new List<RulesModel>();

                        foreach (var nestedRule in nestedRules)
                        {
                            var nestedCollections = GetRelatedCollectionsWithDepth(
                                nestedRule.column_name,
                                nestedRule.condition_text,
                                processedIds
                            );

                            foreach (var kvp in nestedCollections)
                            {
                                if (!nestedRelatedCollectionsDict.ContainsKey(kvp.Key))
                                {
                                    nestedRelatedCollectionsDict[kvp.Key] = kvp.Value;
                                }
                            }
                        }

                        relatedCollections[collection.Id] = new
                        {
                            collection.Id,
                            collection.title,
                            image_src = collection.CollectionImages?.FirstOrDefault()?.src,
                            NestedRelatedCollections = nestedRelatedCollectionsDict.Values.ToList()
                        };
                    }

                    return relatedCollections;
                }

                // Process each menu category
                var menuCategoryWithRelatedCollections = new List<object>();

                foreach (var menuCategory in menuCategoryCollections)
                {
                    var relevantRules = menuCategory.Rules?
                        .Where(rule =>
                            (rule.column_name == "Collection Title" || rule.column_name == "collection_title" ||
                             rule.column_name == "Collection ID" || rule.column_name == "collection_id") &&
                            (rule.relation == "is equal to" || rule.relation == "equals"))
                        .ToList() ?? new List<RulesModel>();

                    if (!relevantRules.Any())
                    {
                        continue;
                    }

                    var processedIds = new HashSet<int>();
                    var relatedCollectionsDict = new Dictionary<int, object>();

                    foreach (var rule in relevantRules)
                    {
                        var nestedRelatedCollections = GetRelatedCollectionsWithDepth(
                            rule.column_name,
                            rule.condition_text,
                            processedIds
                        );

                        foreach (var kvp in nestedRelatedCollections)
                        {
                            if (!relatedCollectionsDict.ContainsKey(kvp.Key))
                            {
                                relatedCollectionsDict[kvp.Key] = kvp.Value;
                            }
                        }
                    }

                    menuCategoryWithRelatedCollections.Add(new
                    {
                        MenuCategory = new { menuCategory.Id, menuCategory.title, image_src = menuCategory.CollectionImages?.FirstOrDefault()?.src },
                        RelatedCollections = relatedCollectionsDict.Values.ToList()
                    });
                }

                var response = new
                {
                    MenuCategoryCollections = menuCategoryCollections
                        .Select(mc => new { mc.Id, mc.title, mc.Layer, image_src = mc.CollectionImages?.FirstOrDefault()?.src })
                        .ToList(),
                    MenuCategoryWithRelatedCollections = menuCategoryWithRelatedCollections
                };

                return Ok(new ResponseBase(true, "Menu category collections and related collections retrieved successfully.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred: {ex.Message}"));
            }
        }


        [HttpPut("smart_collection/{smartCollectionId}/products/reorder")]
        public async Task<IActionResult> ReorderCollectionProducts(int smartCollectionId, [FromBody] List<CollectionProductPositionDto> positions)
        {
            if (positions == null || !positions.Any())
                return BadRequest(ResponseBase.Failure("Positions list cannot be empty."));

            var mapped = positions.Select(p => (p.product_id, p.position)).ToList();
            await _collectionRepository.UpsertCollectionProductPositions(smartCollectionId, mapped);

            return Ok(new ResponseBase(true, "Product positions updated successfully."));
        }

        [HttpGet("smart_collections/count")]
        public async Task<IActionResult> GetCollectionCount()
        {
            try
            {
                var count = await _collectionRepository.GetCollectionCount();
                return Ok(new ResponseBase(true, "Count retrieved successfully.", count));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An internal error occured while retrieving the products count."));
            }
        }

        [HttpPut("smart_collections/{smart_collection_id}")]
        public async Task<IActionResult> UpdateCollection(int smart_collection_id, [FromBody] CollectionDTO collection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Fetch the collection by ID, including related rules and images
                var existingCollection = await _collectionRepository.GetCollectionByIdIncludingRulesAndImages(smart_collection_id);
                if (existingCollection == null)
                {
                    return NotFound("Collection not found.");
                }

                // Update basic collection properties
                existingCollection.title = collection.title;
                existingCollection.handle = collection.handle;
                existingCollection.body_html = collection.body_html;
                existingCollection.sort_order = collection.sort_order;
                existingCollection.template_suffix = collection.template_suffix;
                existingCollection.disjunctive = collection.disjunctive;
                existingCollection.updated_at = DateTime.Now;
                existingCollection.published_scope = collection.published_scope;
                existingCollection.Layer = collection.Layer;
                existingCollection.menu_category = collection.menu_category;

                foreach (var ruleDTO in collection.Rules)
                {
                    // If rule ID exists, update the rule; otherwise, add a new rule
                    if (ruleDTO.Id > 0) // Update existing rule if it has a valid Id
                    {
                        var existingRule = existingCollection.Rules.FirstOrDefault(rule => rule.Id == ruleDTO.Id);

                        if (existingRule != null)
                        {
                            // Update existing rule
                            existingRule.column_name = ruleDTO.column_name;
                            existingRule.relation = ruleDTO.relation;
                            existingRule.condition_text = ruleDTO.condition_text;
                        }
                    }
                    else
                    {
                        // Add new rule if it doesn't have a valid Id (new rule)
                        existingCollection.Rules.Add(new RulesModel
                        {
                            column_name = ruleDTO.column_name,
                            relation = ruleDTO.relation,
                            condition_text = ruleDTO.condition_text,
                        });
                    }
                }

                // Remove rules that are not in the DTO
                var rulesToRemove = existingCollection.Rules
                    .Where(rule => !collection.Rules.Any(r => r.Id == rule.Id))
                    .ToList();

                foreach (var rule in rulesToRemove)
                {
                    existingCollection.Rules.Remove(rule);
                }

                // Handle collection images (update if they exist)
                foreach (var collectionImageDTO in collection.CollectionImages)
                {
                    var existingImage = existingCollection.CollectionImages.FirstOrDefault(image => image.id == collectionImageDTO.Id);
                    if (existingImage != null)
                    {
                        existingImage.src = collectionImageDTO.src;
                    }
                }

                // Save changes
                await _collectionRepository.SaveChangesAsync();

                return Ok(ResponseBase.Success("Collection has been updated", collection));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An internal error occurred while updating the collection."));
            }
        }

        [HttpDelete("smart_collections/{smart_collection_id}")]
        public async Task<IActionResult> DeleteCollection(int smart_collection_id)
        {
            try
            {
                var collection = await _collectionRepository.GetCollectionById(smart_collection_id);
                if (collection == null)
                {
                    return NotFound($"Collection with ID {smart_collection_id} not found.");
                }

                var rules = await _collectionRepository.GetRulesByCollectionId(smart_collection_id);
                var images = await _collectionRepository.GetImagesByCollectionId(smart_collection_id);

                if (rules != null)
                {
                    await _collectionRepository.DeleteRules(rules);
                }
                if (images != null)
                {
                    await _collectionRepository.DeleteImages(images);
                }

                await _collectionRepository.DeleteCollection(collection);
                await _collectionRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, $"Collection with ID {smart_collection_id} deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while deleting the collection: " + ex.Message);
            }
        }
    }
}
