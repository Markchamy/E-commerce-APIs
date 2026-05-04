using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductServices _productRepository;
        public ProductsController(IProductServices productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpPost("create/products")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var productExists = await _productRepository.ProductTitleExists(productDTO.Title);
                if (productExists)
                {
                    return BadRequest("A product with this title already exists.");
                }

                int? smartCollectionId = null; // Initialize as null to handle products without a collection

                if (!string.IsNullOrWhiteSpace(productDTO.product_collection))
                {
                    // Split multiple collections by comma (like tags)
                    var collectionTitles = productDTO.product_collection.Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();

                    // Validate that all collections exist
                    foreach (var collectionTitle in collectionTitles)
                    {
                        var smartCollection = await _productRepository.GetCollectionByTitle(collectionTitle);
                        if (smartCollection == null)
                        {
                            return BadRequest($"The specified product collection '{collectionTitle}' does not exist.");
                        }

                        // Set smart_collection_id to the first collection for backwards compatibility
                        if (smartCollectionId == null)
                        {
                            smartCollectionId = smartCollection.Id;
                        }
                    }
                }

                var product = new ProductModel
                {
                    Id = productDTO.Id,
                    Title = productDTO.Title,
                    body_html = productDTO.body_html,
                    Vendor = productDTO.vendor,
                    product_type = productDTO.product_type,
                    handle = productDTO.handle,
                    //published_scope = productDTO.published_scope,
                    product_collection = productDTO.product_collection,
                    Tags = productDTO.tags,
                    Status = productDTO.status,
                    smart_collection_id = smartCollectionId,
                    product_status = productDTO.product_status,
                    currency = productDTO.currency,
                    product_url = productDTO.product_url,
                    //max_purchase = productDTO.max_purchase,
                    track_quantity = productDTO.track_quantity,
                    physical_product = productDTO.physical_product,
                    continue_selling = productDTO.continue_selling,
                    New = productDTO.New,
                    badge_id = productDTO.badge_id,
                    Variants = (productDTO.Variants ?? new List<VariantDTO>()).Select(variant => new VariantModel
                    {
                        id = variant.Id,
                        title = variant.title,
                        price = variant.price ?? "0",
                        sku = variant.sku ?? "",
                        position = variant.position,
                        //inventory_policy = variant.inventory_policy,
                        compare_at_price = variant.compare_at_price ?? "0",
                        //fulfillment_service = variant.fulfillment_service,
                        //inventory_management = variant.inventory_management,
                        inventory_quantity = variant.inventory_quantity,
                        old_inventory_quantity = variant.old_inventory_quantity,
                        option1 = variant.option1,
                        //option2 = variant.option2,
                        //option3 = variant.option3,
                        taxable = variant.taxable,
                        barcode = variant.barcode,
                        grams = variant.grams,
                        weight = variant.weight,
                        weight_unit = variant.weight_unit ?? "kg",
                        //requires_shipping = variant.requires_shipping,
                    }).ToList(),
                    Options = (productDTO.Options ?? new List<OptionDTO>())
                        .Select(o => new Options
                        {
                            id = o.id,                       // keep incoming option id (long)
                            product_id = productDTO.Id,      // FK to product (long)
                            name = o.name ?? "",
                            position = o.position,
                            product_values = o.product_values ?? new List<string>() // stays as List<string>
                        })
                        .ToList(),
                };

                await _productRepository.AddProduct(product);
                await _productRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Product created successfully.", product));
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging purposes
                // Consider using a logging framework or service
                return StatusCode(500, ResponseBase.Failure("An internal error occurred while adding the product." + ex.Message));
            }
        }

        [HttpPost("import/products")]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            var products = new List<ProductModel>();

            try
            {
                using (var stream = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => args.Header.Trim().Replace(" ", string.Empty), // Trim headers and remove spaces
                    HeaderValidated = null // Disable header validation to avoid exceptions
                }))
                {
                    var records = csv.GetRecords<CsvProductRecord>().ToList();
                    foreach (var record in records)
                    {
                        var product = new ProductModel
                        {
                            Id = int.Parse(record.ProductID),
                            handle = record.Handle,
                            Title = record.Title,
                            body_html = record.BodyHtml,
                            Vendor = record.Vendor,
                            product_type = record.Type,
                            product_collection = record.Collection,
                            Tags = record.Tags,
                            Status = record.Status,
                            product_status = record.ProductStatus
                        };

                        var option = new Options
                        {
                            name = record.OptionName,
                            product_values = record.OptionValue,
                        };

                        if (product.Options == null)
                            product.Options = new List<Options>();

                        product.Options.Add(option);

                        var variant = new VariantModel
                        {
                            sku = record.VariantSKU,
                            grams = int.Parse(record.VariantGrams),
                            inventory_management = record.VariantInventoryManagement,
                            inventory_quantity = int.Parse(record.VariantInventoryQuantity),
                            inventory_policy = record.VariantInventoryPolicy,
                            fulfillment_service = record.VariantFulfillmentService,
                            price = record.VariantPrice,
                            weight_unit = record.VariantWeightUnit
                        };

                        if (product.Variants == null)
                            product.Variants = new List<VariantModel>();

                        product.Variants.Add(variant);

                        var image = new ProductImages
                        {
                            src = record.ImageSource
                        };

                        if (product.ProductImages == null)
                            product.ProductImages = new List<ProductImages>();

                        product.ProductImages.Add(image);

                        products.Add(product);
                    }
                }

                await _productRepository.ImportProductsAsync(products);
                return Ok("Products imported successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error importing products: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("import/products/shopify")]
        public async Task<IActionResult> ImportShopifyProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            var products = new List<CsvProductModel>();

            try
            {
                using (var stream = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => args.Header.Trim().Replace(" ", string.Empty),
                    HeaderValidated = null
                }))
                {
                    // Map CSV fields to CsvProductModel properties
                    var records = csv.GetRecords<CsvProductModel>().ToList();

                    foreach (var record in records)
                    {
                        var product = new CsvProductModel
                        {
                            Handle = record.Handle,
                            Title = record.Title,
                            BodyHTML = record.BodyHTML,
                            Vendor = record.Vendor,
                            ProductCategory = record.ProductCategory,
                            Type = record.Type,
                            Tags = record.Tags,
                            Status = record.Status,
                            Option1Name = record.Option1Name,
                            Option1Value = record.Option1Value,
                            VariantPrice = record.VariantPrice,
                            VariantSKU = record.VariantSKU,
                            VariantGrams = record.VariantGrams,
                            VariantInventoryManagement = record.VariantInventoryManagement,
                            VariantInventoryQuantity = record.VariantInventoryQuantity,
                            VariantInventoryPolicy = record.VariantInventoryPolicy,
                            VariantFulfillmentService = record.VariantFulfillmentService,
                            VariantWeightUnit = record.VariantWeightUnit,
                            ImageSource = record.ImageSource
                        };

                        products.Add(product);
                    }
                }

                // Implement database saving logic here
                await _productRepository.ImportShopifyProductsAsync(products);

                return Ok("Shopify products imported successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing Shopify products: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("export/all-products")]
        public async Task<IActionResult> ExportProducts()
        {
            try
            {
                var products = await _productRepository.GetAllProducts();  // Ensure you have a method to retrieve all products

                var csvBuilder = new StringBuilder();
                var headers = "Product ID,Handle, Title, Body Html, Vendor, Type, Collection, Tags, Status, Product Status, Option Name, Option Value, Variant SKU," +
                    "Variant Grams, Variant Inventory Management, Variant Inventory Quantity, Variant Inventory Policy, Variant Fulfillment Service, Variant Price," +
                    "Image Source, Variant Weight Unit";
                csvBuilder.AppendLine(headers);

                foreach (var product in products)
                {
                    foreach (var variant in product.Variants)
                    {
                        foreach (var option in product.Options)
                        {
                            foreach (var image in product.ProductImages)
                            {
                                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{18},{19},{20}",
                                    product.Id,
                                    product.handle,
                                    product.Title.Replace(",", ";"),  // Replace commas to avoid CSV issues
                                    product.body_html.Replace("\n", " ").Replace("\r", " ").Replace(",", ";"), // Remove new lines and carriage returns, replace commas
                                    product.Vendor.Replace(",", ";"),
                                    product.product_type,
                                    product.product_collection.Replace(",", ";"),
                                    product.Tags.Replace(",", ";"),  // Correct handling of multiple tags
                                    product.Status,
                                    product.product_status.Replace(",", ";"),
                                    option.name,
                                    string.Join(";", option.product_values.Select(productvalue => productvalue.Replace(",", ";"))),
                                    variant.sku,
                                    variant.grams,
                                    variant.inventory_management,
                                    variant.inventory_quantity,
                                    variant.inventory_policy,
                                    variant.fulfillment_service,
                                    variant.price,
                                    image.src,
                                    variant.weight_unit);
                                csvBuilder.AppendLine(newLine);
                            }
                        }
                    }
                }

                var byteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var stream = new MemoryStream(byteArray);

                return File(stream, "text/csv", "product_export.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("Failed to export products: " + ex.Message));
            }
        }

        //[HttpGet("export/products")]
        //public async Task<IActionResult> ExportSelectedProducts([FromQuery] string ids)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(ids))
        //        {
        //            return BadRequest("No product IDs provided.");
        //        }

        //        var productIds = ids.Split(',').Select(int.Parse).ToList();

        //        var products = await _productRepository.GetProductsByIds(productIds);

        //        var csvBuilder = new StringBuilder();
        //        var headers = "Product ID,Handle, Title, Body Html, Vendor, Type, Collection, Tags, Status, Product Status, Option Name, Option Value, Variant SKU," +
        //                      "Variant Grams, Variant Inventory Management, Variant Inventory Quantity, Variant Inventory Policy, Variant Fulfillment Service, Variant Price," +
        //                      "Image Source, Variant Weight Unit";
        //        csvBuilder.AppendLine(headers);

        //        foreach (var product in products)
        //        {
        //            foreach (var variant in product.Variants)
        //            {
        //                foreach (var option in product.Options)
        //                {
        //                    foreach (var image in product.ProductImages)
        //                    {
        //                        var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{18},{19},{20}",
        //                            product.Id,
        //                            product.handle,
        //                            product.Title.Replace(",", ";"),
        //                            product.body_html.Replace("\n", " ").Replace("\r", " ").Replace(",", ";"),
        //                            product.Vendor.Replace(",", ";"),
        //                            product.product_type,
        //                            product.product_collection.Replace(",", ";"),
        //                            product.Tags.Replace(",", ";"),
        //                            product.Status,
        //                            product.product_status.Replace(",", ";"),
        //                            option.name,
        //                            string.Join(";", option.product_values.Select(productvalue => productvalue.Replace(",", ";"))),
        //                            variant.sku,
        //                            variant.grams,
        //                            variant.inventory_management,
        //                            variant.inventory_quantity,
        //                            variant.inventory_policy,
        //                            variant.fulfillment_service,
        //                            variant.price,
        //                            image.src,
        //                            variant.weight_unit);
        //                        csvBuilder.AppendLine(newLine);
        //                    }
        //                }
        //            }
        //        }

        //        var byteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        //        var stream = new MemoryStream(byteArray);

        //        return File(stream, "text/csv", "selected_product_export.csv");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ResponseBase.Failure("Failed to export selected products: " + ex.Message));
        //    }
        //}

        [HttpGet("products/new")]
        public async Task<IActionResult> GetNewProducts()
        {
            try
            {
                // Fetch all products where New == true with eager loading
                var newProducts = await _productRepository.GetProductsWhereNewIsTrue();

                if (newProducts == null || !newProducts.Any())
                {
                    return NotFound("No new products found.");
                }

                // Select the required fields only
                var response = new
                {
                    Products = newProducts.Take(16).Select(product => new
                    {
                        product.Id,
                        product.Title,
                        productPrice = product.Variants?.FirstOrDefault()?.price ?? "N/A",
                        productSalePrice = product.Variants?.FirstOrDefault()?.compare_at_price ?? "N/A",
                        productInventory = product.Variants?.FirstOrDefault()?.inventory_quantity,
                        ProductImageSrc = product.ProductImages?.FirstOrDefault()?.src ?? "No image available"
                    }).ToList()
                };

                return Ok(new ResponseBase(true, "New products retrieved successfully.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred: {ex.Message}"));
            }
        }



        [HttpGet("products/count")]
        public async Task<IActionResult> GetProductsCount()
        {
            try
            {
                var count = await _productRepository.GetProductsCount();
                return Ok(new ResponseBase(true, "Count retrieved successfully", count));
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, ResponseBase.Failure("An Internal error occurred while retrieving the products count."));
            }
        }

        [HttpGet("products/title/{title}")]
        public async Task<IActionResult> GetProductsByTitle(string title)
        {
            try
            {
                var products = await _productRepository.GetProductsByTitleAsync(title);
                if (products == null)
                {
                    return NotFound(new ResponseBase(false, $"No products found with title '{title}'.", null));
                }

                return Ok(new ResponseBase(true, "Products retrieved successfully.", products));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving products: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }


        [HttpGet("products/single/{id}")]
        public async Task<IActionResult> GetProductWithDetails(long id)
        {
            try
            {
                var product = await _productRepository.GetProductWithDetailsById(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found.");
                }

                return Ok(new ResponseBase(true, "Product retrieved successfully", product));
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, ResponseBase.Failure("An internal error occurred while retrieving the product." + ex.Message));
            }
        }

        [HttpGet("products/details/all")]
        public async Task<IActionResult> GetAllProductsWithDetails(
            int page = 1,                   // Pagination: Current page
            int pageSize = 50,              // Pagination: Number of products per page
            string sortBy = "Created",         // Default sort column
            string sortDirection = "desc",   // Default sort direction
            string filter = "All",             // Filtering criteria
            string search = ""              // Search term
        )
        {
            try
            {
                // Validate pagination parameters
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new ResponseBase(false, "Page and pageSize must be greater than 0"));
                }

                // Fetch the products with sorting, filtering, and searching
                var products = await _productRepository.GetAllProductsWithDetails(page, pageSize, sortBy, sortDirection, filter, search);

                // Return products or an empty list if none found
                return Ok(new ResponseBase(true, "Products with details retrieved successfully", products ?? new List<ProductModel>()));
            }
            catch (Exception ex)
            {
                // Return 500 in case of an internal error
                return StatusCode(500, new ResponseBase(false, $"An internal error occurred: {ex.Message}"));
            }
        }

        [HttpPut("set-draft/{id}")]
        public async Task<IActionResult> UpdateProductStatusToDraft(long id)
        {
            try
            {
                Console.WriteLine($"Attempting to update product status to draft for product with ID: {id}");

                // Retrieve the product (including related variants and options, if needed)
                var product = await _productRepository.GetProductByIdIncludingVariantsAndOptions(id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound($"Product with ID {id} not found.");
                }

                // Only active products should be updated to draft
                if (!string.Equals(product.Status, "active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only active products can be updated to draft.");
                }

                // Update the product status to draft and update the updated_at field
                product.Status = "draft";
                product.updated_at = DateTime.UtcNow;

                await _productRepository.SaveChangesAsync();

                Console.WriteLine("Product status updated to draft successfully.");
                return Ok(ResponseBase.Success($"Product with ID {id} status updated to draft successfully.", product));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product status to draft: {ex.Message}");
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating the product status: {ex.Message}"));
            }
        }

        [HttpPut("set-active/{id}")]
        public async Task<IActionResult> UpdateProductStatusToActive(long id)
        {
            try
            {
                Console.WriteLine($"Attempting to update product status to active for product with ID: {id}");

                // Retrieve the product (including related variants and options, if needed)
                var product = await _productRepository.GetProductByIdIncludingVariantsAndOptions(id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound($"Product with ID {id} not found.");
                }

                // Only draft products should be updated to active
                if (!string.Equals(product.Status, "draft", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only draft products can be updated to active.");
                }

                // Update the product status to active and update the updated_at field
                product.Status = "active";
                product.updated_at = DateTime.UtcNow;

                await _productRepository.SaveChangesAsync();

                Console.WriteLine("Product status updated to active successfully.");
                return Ok(ResponseBase.Success($"Product with ID {id} status updated to active successfully.", product));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product status to active: {ex.Message}");
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating the product status: {ex.Message}"));
            }
        }


        [HttpPut("update/product/{id}")]
        public async Task<IActionResult> UpdateSingleProduct(long id, [FromBody] ProductDTO productDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Console.WriteLine($"Updating product with ID: {id}");

                var existingProduct = await _productRepository.GetProductByIdIncludingVariantsAndOptions(id);
                if (existingProduct == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound($"Product with ID {id} not found.");
                }

                // Update product fields
                existingProduct.Title = productDTO.Title;
                existingProduct.body_html = productDTO.body_html;
                existingProduct.Vendor = productDTO.vendor;
                existingProduct.product_type = productDTO.product_type;
                existingProduct.product_collection = productDTO.product_collection;
                existingProduct.handle = productDTO.handle;
                existingProduct.Tags = productDTO.tags;
                existingProduct.Status = productDTO.status;
                existingProduct.product_status = productDTO.product_status;
                existingProduct.currency = productDTO.currency;
                existingProduct.product_url = productDTO.product_url;
                existingProduct.track_quantity = productDTO.track_quantity;
                existingProduct.physical_product = productDTO.physical_product;
                existingProduct.continue_selling = productDTO.continue_selling;
                existingProduct.New = productDTO.New;
                existingProduct.badge_id = productDTO.badge_id;
                existingProduct.updated_at = DateTime.UtcNow;

                Console.WriteLine("Updated product fields.");

                // Ensure existingProduct.Variants and existingProduct.Options are initialized
                if (existingProduct.Variants == null)
                    existingProduct.Variants = new List<VariantModel>();

                if (existingProduct.Options == null)
                    existingProduct.Options = new List<Options>();

                // Update Variants
                if (productDTO.Variants != null)
                {
                    foreach (var variantDTO in productDTO.Variants)
                    {
                        var existingVariant = existingProduct.Variants.FirstOrDefault(v => v.id == variantDTO.Id);
                        if (existingVariant != null)
                        {
                            existingVariant.title = variantDTO.title;
                            existingVariant.price = variantDTO.price;
                            existingVariant.sku = variantDTO.sku;
                            existingVariant.position = variantDTO.position;
                            existingVariant.inventory_policy = variantDTO.inventory_policy;
                            existingVariant.compare_at_price = variantDTO.compare_at_price;
                            existingVariant.fulfillment_service = variantDTO.fulfillment_service;
                            existingVariant.inventory_management = variantDTO.inventory_management;
                            existingVariant.inventory_quantity = variantDTO.inventory_quantity;
                            existingVariant.option1 = variantDTO.option1;
                            existingVariant.option2 = variantDTO.option2;
                            existingVariant.option3 = variantDTO.option3;
                            existingVariant.taxable = variantDTO.taxable;
                            existingVariant.barcode = variantDTO.barcode;
                            existingVariant.grams = variantDTO.grams;
                            existingVariant.weight = variantDTO.weight;
                            existingVariant.weight_unit = variantDTO.weight_unit;
                        }
                        else
                        {
                            Console.WriteLine($"Variant with ID {variantDTO.Id} not found.");
                        }
                    }
                }

                // Update Options
                if (productDTO.Options != null)
                {
                    foreach (var optionDTO in productDTO.Options)
                    {
                        var existingOption = existingProduct.Options.FirstOrDefault(o => o.id == optionDTO.id);
                        if (existingOption != null)
                        {
                            existingOption.name = optionDTO.name;
                            existingOption.product_values = optionDTO.product_values;
                        }
                        else
                        {
                            Console.WriteLine($"Option with ID {optionDTO.id} not found.");
                        }
                    }
                }

                // Save changes
                await _productRepository.SaveChangesAsync();

                Console.WriteLine("Product update completed.");
                return Ok(ResponseBase.Success($"Product with ID {id} updated successfully", productDTO));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating the product: {ex.Message}"));
            }
        }

        [HttpPatch("update/product/{id}/body-html")]
        public async Task<IActionResult> UpdateProductBodyHtml(long id, [FromBody] UpdateBodyHtmlDTO dto)
        {
            try
            {
                var product = await _productRepository.GetProductByIdIncludingVariantsAndOptions(id);
                if (product == null)
                    return NotFound(ResponseBase.Failure($"Product with ID {id} not found."));

                product.body_html = dto.body_html;
                product.updated_at = DateTime.UtcNow;

                await _productRepository.SaveChangesAsync();

                return Ok(ResponseBase.Success($"body_html for product {id} updated successfully", new { id, dto.body_html }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating body_html: {ex.Message}"));
            }
        }

        [HttpGet("test/product/{id}")]
        public async Task<IActionResult> GetProduct(long id)
        {
            var product = await _productRepository.GetProductByIdIncludingVariantsAndOptions(id);
            return Ok(product);
        }


        [HttpPut("bulk-update/products")]
        public async Task<IActionResult> BulkUpdateProducts([FromBody] List<ProductDTO> productDTOs)
        {
            try
            {
                if (productDTOs == null || !productDTOs.Any())
                {
                    return BadRequest("No products provided for update.");
                }

                Console.WriteLine($"Received request to bulk update {productDTOs.Count} products.");

                List<string> errors = new List<string>();
                List<long> updatedProductIds = new List<long>();

                foreach (var productDTO in productDTOs)
                {
                    try
                    {
                        var existingProduct = await _productRepository.GetProductByIdIncludingVariantsAndOptions(productDTO.Id);
                        if (existingProduct == null)
                        {
                            errors.Add($"Product with ID {productDTO.Id} not found.");
                            continue;
                        }

                        // Update product fields (assigning plain text directly)
                        existingProduct.Title = productDTO.Title;
                        existingProduct.body_html = string.IsNullOrWhiteSpace(productDTO.body_html)
                            ? ""
                            : System.Net.WebUtility.HtmlDecode(productDTO.body_html);
                        existingProduct.Vendor = productDTO.vendor;
                        existingProduct.product_type = productDTO.product_type;
                        existingProduct.smart_collection_id = productDTO.smart_collection_id;
                        existingProduct.handle = productDTO.handle;
                        existingProduct.Tags = productDTO.tags;
                        // Assign directly as plain text (no JSON serialization)
                        existingProduct.Status = productDTO.status;
                        existingProduct.product_status = productDTO.product_status;
                        existingProduct.currency = productDTO.currency;
                        existingProduct.product_url = productDTO.product_url;
                        existingProduct.track_quantity = productDTO.track_quantity;
                        existingProduct.physical_product = productDTO.physical_product;
                        existingProduct.continue_selling = productDTO.continue_selling;
                        existingProduct.New = productDTO.New;
                        existingProduct.badge_id = productDTO.badge_id;
                        existingProduct.updated_at = DateTime.UtcNow;

                        Console.WriteLine($"Product {productDTO.Id} fields updated.");

                        // Ensure collections are not null
                        if (existingProduct.Variants == null)
                            existingProduct.Variants = new List<VariantModel>();

                        if (existingProduct.Options == null)
                            existingProduct.Options = new List<Options>();

                        // Update Variants
                        if (productDTO.Variants != null)
                        {
                            foreach (var variantDTO in productDTO.Variants)
                            {
                                var existingVariant = existingProduct.Variants.FirstOrDefault(v => v.id == variantDTO.Id);
                                if (existingVariant != null)
                                {
                                    existingVariant.title = variantDTO.title;
                                    existingVariant.price = string.IsNullOrWhiteSpace(variantDTO.price)
                                        ? "0"
                                        : variantDTO.price;
                                    existingVariant.sku = variantDTO.sku;
                                    existingVariant.position = variantDTO.position;
                                    existingVariant.compare_at_price = string.IsNullOrWhiteSpace(variantDTO.compare_at_price)
                                        ? "0"
                                        : variantDTO.compare_at_price;
                                    // Assign these fields directly as plain text
                                    existingVariant.inventory_policy = variantDTO.inventory_policy;
                                    existingVariant.inventory_quantity = variantDTO.inventory_quantity;
                                    existingVariant.option1 = variantDTO.option1;
                                    existingVariant.taxable = variantDTO.taxable;
                                    existingVariant.barcode = variantDTO.barcode;
                                    existingVariant.grams = variantDTO.grams;
                                    existingVariant.weight = variantDTO.weight;
                                    existingVariant.weight_unit = variantDTO.weight_unit;
                                }
                                else
                                {
                                    Console.WriteLine($"Variant with ID {variantDTO.Id} not found for Product {productDTO.Id}.");
                                }
                            }
                        }

                        // Update Options
                        if (productDTO.Options != null)
                        {
                            foreach (var optionDTO in productDTO.Options)
                            {
                                var existingOption = existingProduct.Options.FirstOrDefault(o => o.id == optionDTO.id);
                                if (existingOption != null)
                                {
                                    existingOption.name = optionDTO.name;
                                }
                                else
                                {
                                    Console.WriteLine($"Option with ID {optionDTO.id} not found for Product {productDTO.Id}.");
                                }
                            }
                        }

                        // Add to successful update list
                        updatedProductIds.Add(productDTO.Id);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error updating Product {productDTO.Id}: {ex.Message}");
                    }
                }

                // Save changes if there are updates
                if (updatedProductIds.Any())
                {
                    await _productRepository.SaveChangesAsync();
                    Console.WriteLine("Bulk update saved successfully.");
                }

                // Return response
                if (errors.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Bulk update completed with some errors.",
                        updatedProducts = updatedProductIds,
                        errors = errors
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = true,
                        message = "All products updated successfully.",
                        updatedProducts = updatedProductIds
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bulk update failed: {ex.Message}");
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating the products: {ex.Message}"));
            }
        }


        [HttpDelete("delete/products/{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            try
            {
                var product = await _productRepository.GetProductById(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found.");
                }

                // Assuming you have methods in your repository to get related Variants and Options
                var variants = await _productRepository.GetVariantsByProductId(id);
                var options = await _productRepository.GetOptionsByProductId(id);
                var images = await _productRepository.GetImagesByProductId(id);

                // Delete Variants and Options first
                if (variants != null)
                {
                    await _productRepository.DeleteVariants(variants);
                }
                if (options != null)
                {
                    await _productRepository.DeleteOptions(options);
                }
                if (images != null)
                {
                    await _productRepository.DeleteImages(images);
                }

                // Now delete the Product
                await _productRepository.DeleteProduct(product);
                await _productRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, $"Product with ID {id} deleted successfully"));
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An Internal error occurred while deleting the product: " + ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult> SearchProducts([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "No search query provided" });
            }

            var products = await _productRepository.SearchProductsAsync(query);

            var productInfo = products.Select(product => new
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.body_html,
                product.product_collection,
                Price = product.Variants != null && product.Variants.Any() ? product.Variants.FirstOrDefault().price : null,
                Quantity = product.Variants != null && product.Variants.Any() ? product.Variants.FirstOrDefault().inventory_quantity : 0,
                Image = product.ProductImages !=  null && product.ProductImages.Any() ? product.ProductImages.FirstOrDefault().src : null
            }).ToList();

            var response = new
            {
                success = true,
                message = $"{productInfo.Count} products found",
                products = productInfo
            };

            return Ok(response);
        }

        [HttpGet("products/with-sku")]
        public async Task<IActionResult> GetProductsWithSku([FromQuery] int page = 1, [FromQuery] int limit = 250)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1)
                {
                    return BadRequest(new ResponseBase(false, "Page must be greater than 0", null));
                }

                if (limit < 1 || limit > 1000)
                {
                    return BadRequest(new ResponseBase(false, "Limit must be between 1 and 1000", null));
                }

                var products = await _productRepository.GetProductsWithSkuAsync(page, limit);

                if (products == null || !products.Any())
                {
                    return NotFound(new ResponseBase(false, "No products with SKUs found.", null));
                }

                var response = products.Select(product => new
                {
                    product.Id,
                    product.Title,
                    product.body_html,
                    product.Vendor,
                    product.product_type,
                    product.Status,
                    product.created_at,
                    product.updated_at,
                    Variants = product.Variants?.Select(v => new
                    {
                        v.id,
                        v.title,
                        v.price,
                        v.sku,
                        v.inventory_quantity,
                        v.compare_at_price
                    }).ToList(),
                    ProductImages = product.ProductImages?.Select(img => new
                    {
                        img.id,
                        img.src,
                        img.alt
                    }).ToList()
                }).ToList();

                var result = new
                {
                    page = page,
                    limit = limit,
                    count = response.Count,
                    products = response
                };

                return Ok(new ResponseBase(true, $"{response.Count} products with SKUs retrieved successfully (Page {page}).", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred: {ex.Message}"));
            }
        }



    }



}

