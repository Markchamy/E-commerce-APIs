using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductServices _productRepository;
        public ProductVariantsController(IProductServices productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpPost("products/{product_id}/variants")]
        public async Task<IActionResult> AddVariantToProduct(long product_id, [FromBody] VariantDTO variant)
        {
            try
            {
                Console.WriteLine($"🔵 Adding variant to product {product_id}");
                Console.WriteLine($"🔵 Variant title: {variant.title}, price: {variant.price}, position: {variant.position}");

                var variants = new VariantModel
                {
                    title = variant.title,
                    price = variant.price,
                    sku = variant.sku,
                    position = variant.position,
                    compare_at_price = variant.compare_at_price,
                    option1 = variant.option1,
                    taxable = variant.taxable,
                    barcode = variant.barcode,
                    grams = variant.grams,
                    weight = variant.weight,
                    weight_unit = variant.weight_unit,

                    // ✅ NOW ACCEPTING THESE FROM FRONTEND:
                    product_id = product_id,  // Use path parameter, not request body
                    inventory_quantity = variant.inventory_quantity,
                    old_inventory_quantity = variant.old_inventory_quantity,
                    inventory_policy = variant.inventory_policy,
                    fulfillment_service = variant.fulfillment_service,
                    inventory_management = variant.inventory_management,
                    requires_shipping = variant.requires_shipping,
                    created_at = DateTime.UtcNow,  // Override with server time
                    updated_at = DateTime.UtcNow   // Override with server time
                };

                Console.WriteLine($"🔵 Calling repository to add variant...");
                await _productRepository.AddVariantToProduct(product_id, variants);

                Console.WriteLine($"✅ Variant added successfully!");
                return Ok(new ResponseBase(true, "the variant has been added to the product", variant));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine($"❌ INNER EXCEPTION: {ex.InnerException?.Message}");
                Console.WriteLine($"❌ STACK: {ex.StackTrace}");

                return StatusCode(500, ResponseBase.Failure($"Error: {ex.InnerException?.Message ?? ex.Message}"));
            }
        }

        [HttpGet("products/{product_id}/variants")]
        public async Task<IActionResult> GetVariantByProductId(long product_id)
        {
            try
            {
                var variants = await _productRepository.GetVariantByProductId(product_id);
                
                if (variants == null || variants.Count == 0)
                {
                    return NotFound("No variants found for this specific product.");
                }

                return Ok(new ResponseBase(true, "variants retrieved.", variants));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while retrieving the variants." + ex.Message));
            }
        }

        [HttpGet("products/{productId}/variants/{variantId}")]
        public async Task<IActionResult> GetVariantByProductIdAndVariantId(long productId, long variantId)
        {
            try
            {
                var variant = await _productRepository.GetVariantByProductIdAndVariantId(productId, variantId);

                if (variant == null)
                {
                    return NotFound($"No variant with ID {variantId} found for product ID {productId}");
                }

                return Ok(new ResponseBase(true, "variant retrieved.", variant));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while retrieving the variant."));
            }
        }

        [HttpGet("products/{productId}/variants/count")]
        public async Task<IActionResult> GetVariantsCountByProductId(long productId)
        {
            try
            {
                var count = await _productRepository.GetVariantsCountByProductId(productId);
                return Ok(new { ProductId = productId, Count = count});
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while retrieving the variants for the specified product."));
            }
        }

        [HttpPut("products/{productId}/variant/{variantId}")]
        public async Task<IActionResult> UpdateVariant(long productId, long variantId, [FromBody] VariantDTO variantDTO)
        {
            try
            {
                var updateVariant = new VariantModel
                {
                title = variantDTO.title,
                price = variantDTO.price,
                sku = variantDTO.sku,
                position = variantDTO.position,
                inventory_policy = variantDTO.inventory_policy,
                compare_at_price = variantDTO.compare_at_price,
                fulfillment_service = variantDTO.fulfillment_service,
                inventory_management = variantDTO.inventory_management,
                option1 = variantDTO.option1,
                taxable = variantDTO.taxable,
                barcode = variantDTO.barcode,
                grams = variantDTO.grams,
                image_id = variantDTO.image_id,
                weight = variantDTO.weight,
                weight_unit = variantDTO.weight_unit,
                inventory_id = variantDTO.inventory_id,
                requires_shipping = variantDTO.requires_shipping,
                };

                var result = await _productRepository.UpdateVariant(productId, variantId, variantDTO);

                if (!result)
                {
                    return NotFound($"No variant with ID {variantId} found for productId {productId}.");
                }

                return Ok(new ResponseBase(true, $"Variant with Id {variantId} has been updated successfully.", updateVariant));
            } 
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while updating the variant."));
            }
        }

        [HttpDelete("products/{productId}/variants/{variantId}")]
        public async Task<IActionResult> DeleteProductVariant(long productId, long variantId)
        {
            try
            {
                var result = await _productRepository.DeleteVariant(productId, variantId);

                if (!result)
                {
                    return NotFound($"No variant with ID {variantId} found for product ID {productId}");
                }

                return Ok($"Variant with id {variantId} has been deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while deleting the variant."));
            }
        }

        [HttpPut("products/{productId}/variants/positions")]
        public async Task<IActionResult> UpdateVariantPositions(long productId, [FromBody] List<VariantPositionUpdateDTO> variantPositions)
        {
            try
            {
                if (variantPositions == null || variantPositions.Count == 0)
                {
                    return BadRequest(ResponseBase.Failure("Variant positions list cannot be empty."));
                }

                var result = await _productRepository.UpdateVariantPositions(productId, variantPositions);

                if (!result)
                {
                    return NotFound($"Failed to update variant positions for product ID {productId}. Ensure all variants belong to this product.");
                }

                return Ok(new ResponseBase(true, "Variant positions updated successfully.", variantPositions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occurred while updating variant positions."));
            }
        }
    }
}
