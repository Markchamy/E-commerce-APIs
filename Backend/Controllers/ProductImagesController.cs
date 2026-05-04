using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductServices _productRepository;
        private readonly string _baseUrl;

        public ProductImagesController(IProductServices productRepository, IOptions<AppSettings> appSettings)
        {
            _productRepository = productRepository;
            _baseUrl = appSettings.Value.BaseUrl;
        }

        [HttpPost("add/product/{productId}/variant/{variantId}/images")]
        public async Task<IActionResult> AddImageToProductVariant(long productId, long variantId, IFormFile imageFile, int width, int height, int position)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return BadRequest(new ResponseBase(false, "No image file provided."));

                // Define the upload path under the application root where the app has permissions
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", "variants");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var image = new ProductImages
                {
                    width = width,
                    height = height,
                    src = $"{_baseUrl}/uploads/products/variants/{fileName}",
                    position = position,
                    alt = imageFile.FileName,
                    product_id = productId,
                    variant_id = variantId
                };

                // Add the image entity to the repository for the specified variant
                await _productRepository.AddImageToProduct(productId, variantId, image);

                // Save changes to the database
                await _productRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Image added successfully to the product variant.", image));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseBase(false, $"An error occurred while adding the image to the product variant: {ex.Message}"));
            }
        }


        [HttpPost("add/product/{productId}/images")]
        public async Task<IActionResult> AddImage(long productId, IFormFile imageFile, int width, int height, int position)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return BadRequest(new ResponseBase(false, "No image file provided."));

                // Use a directory under the application root where the app has permissions
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var image = new ProductImages
                {
                    width = width,
                    height = height,
                    src = $"{_baseUrl}/uploads/products/{fileName}",
                    position = position,
                    alt = imageFile.FileName,
                    product_id = productId,
                };

                // Add the image entity to the context
                await _productRepository.AddImage(productId, image);

                // Save changes to the database
                await _productRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Image added successfully.", image));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseBase(false, $"An error occurred while adding the image to the product: {ex.Message}"));
            }
        }



        [HttpGet("products/{productId}/images")]
        public async Task<IActionResult> GetImagesByProductId(long productId)
        {
            try
            {
                var images = await _productRepository.GetImagesByProductId(productId);

                if (images == null || images.Count == 0)
                {
                    return NotFound("No images found for the specified product.");
                }

                
                return Ok(new ResponseBase(true, "image retrieved", images));
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, ResponseBase.Failure("An error occurred while retrieving the images."));
            }
        }

        [HttpGet("products/{productId}/images/{imageId}")]
        public async Task<IActionResult> GetImageByProductIdAndImageId(long productId, long imageId)
        {
            try
            {
                var image = await _productRepository.GetImageByProductIdAndImageId(productId, imageId);

                if (image == null)
                {
                    return NotFound($"No image with ID {imageId} found for product ID {productId}.");
                }

                // Optionally convert your image entity to a DTO before returning
                return Ok(new ResponseBase(true, "images retrieved", image));
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, ResponseBase.Failure("An error occurred while retrieving the image."));
            }
        }

        [HttpGet("products/{productId}/images/count")]
        public async Task<IActionResult> GetImagesCountByProductId(long productId)
        {
            try
            {
                var count = await _productRepository.GetImagesCountByProductId(productId);
                return Ok(new { ProductId = productId, Count = count });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, ResponseBase.Failure("An error occurred while retrieving the images count for the specified product."));
            }
        }

        [HttpPut("products/{productId}/images/{imageId}")]
        public async Task<IActionResult> UpdateProductImage(long productId, long imageId, [FromBody] ProductImagesDTO imageUpdateDTO)
        {
            try
            {
                var updatedImage = new ProductImages
                {
                    width = imageUpdateDTO.width,
                    height = imageUpdateDTO.height,
                    src = imageUpdateDTO.src,
                    alt = imageUpdateDTO.alt,
                    
                    updated_at = DateTime.UtcNow,
                };

                var result = await _productRepository.UpdateImage(productId, imageId, imageUpdateDTO);

                if (!result)
                {
                    return NotFound($"No image with ID {imageId} found for product ID {productId}.");
                }

                return Ok(new ResponseBase(true, $"Image with Id {imageId} has been update it", updatedImage));
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, ResponseBase.Failure("An error occurred while updating the image."));
            }
        }

        [HttpPut("products/{productId}/images/{imageId}/position")]
        public async Task<IActionResult> UpdateProductImagePosition(long productId, long imageId, [FromBody] int position)
        {
            try
            {
                var result = await _productRepository.UpdateImagePosition(productId, imageId, position);

                if (!result)
                    return NotFound($"No image with ID {imageId} found for product ID {productId}.");

                return Ok(new ResponseBase(true, $"Image {imageId} position updated to {position}."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure($"An error occurred while updating the image position: {ex.Message}"));
            }
        }

        [HttpDelete("products/{productId}/images/{imageId}")]
        public async Task<IActionResult> DeleteProductImage(long productId, long imageId)
        {
            try
            {
                var result = await _productRepository.DeleteImage(productId, imageId);

                if (!result)
                {
                    return NotFound($"No image with ID {imageId} found for product ID {productId}.");
                }

                return Ok($"Image with id {imageId} has been deleted successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, ResponseBase.Failure("An error occurred while deleting the image."));
            }
        }

        /// <summary>
        /// Retrieves a summary of all products with their product-level images.
        /// </summary>
        /// <remarks>
        /// Returns all products along with their associated images (where variant_id is null).
        /// This endpoint is useful for:
        /// - Checking which products have images uploaded
        /// - Identifying products with missing images
        /// - Reconciling image uploads after connection failures
        /// - Auditing product image coverage
        ///
        /// Each product in the response includes:
        /// - Product ID and title
        /// - Total count of product-level images
        /// - Full image details (id, src, position, dimensions, alt text)
        ///
        /// Images are ordered by position.
        /// </remarks>
        /// <response code="200">Returns the list of products with their images summary</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("products/images/summary")]
        public async Task<IActionResult> GetProductsImagesSummary()
        {
            try
            {
                var products = await _productRepository.GetAllProducts();

                var summary = products.Select(p => new
                {
                    product_id = p.Id,
                    title = p.Title,
                    image_count = p.ProductImages?.Count(img => img.variant_id == null) ?? 0,
                    images = p.ProductImages?
                        .Where(img => img.variant_id == null)
                        .OrderBy(img => img.position)
                        .Select(img => new
                        {
                            id = img.id,
                            src = img.src,
                            position = img.position,
                            width = img.width,
                            height = img.height,
                            alt = img.alt
                        })
                        .ToList()
                }).ToList();

                return Ok(new ResponseBase(true, $"Retrieved {summary.Count} products with images summary", summary));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure($"An error occurred while retrieving products images summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves a summary of all product variants with their variant-specific images.
        /// </summary>
        /// <remarks>
        /// Returns all product variants along with their associated images (where variant_id is not null).
        /// This endpoint is useful for:
        /// - Checking which variants have images uploaded
        /// - Identifying variants with missing images
        /// - Reconciling variant image uploads after connection failures
        /// - Auditing variant image coverage across all products
        ///
        /// Each variant in the response includes:
        /// - Product ID and product title (parent product)
        /// - Variant ID, title, and SKU
        /// - Total count of variant-specific images
        /// - Full image details (id, src, position, dimensions, alt text)
        ///
        /// Images are ordered by position.
        /// Variants without images will still be included with image_count = 0 and empty images array.
        /// </remarks>
        /// <response code="200">Returns the list of variants with their images summary</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("products/variants/images/summary")]
        public async Task<IActionResult> GetVariantsImagesSummary()
        {
            try
            {
                var products = await _productRepository.GetAllProducts();

                var variantsSummary = new List<object>();

                foreach (var product in products)
                {
                    var variants = await _productRepository.GetVariantsByProductId(product.Id);

                    foreach (var variant in variants)
                    {
                        var variantImages = product.ProductImages?
                            .Where(img => img.variant_id == variant.id)
                            .OrderBy(img => img.position)
                            .Select(img => new
                            {
                                id = img.id,
                                src = img.src,
                                position = img.position,
                                width = img.width,
                                height = img.height,
                                alt = img.alt
                            })
                            .ToList();

                        variantsSummary.Add(new
                        {
                            product_id = product.Id,
                            product_title = product.Title,
                            variant_id = variant.id,
                            variant_title = variant.title,
                            variant_sku = variant.sku,
                            image_count = variantImages?.Count ?? 0,
                            images = variantImages
                        });
                    }
                }

                return Ok(new ResponseBase(true, $"Retrieved {variantsSummary.Count} variants with images summary", variantsSummary));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure($"An error occurred while retrieving variants images summary: {ex.Message}"));
            }
        }


    }
}
