using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]

    public class DiscountController : ControllerBase
    {
        private readonly IDiscountServices _discountService;
        public DiscountController(IDiscountServices discountService)
        {
            _discountService = discountService;
        }

        [HttpPost("price_rule/{price_rule_id}/discount_code")]
        public async Task<IActionResult> CreateDiscountCode([FromBody] DiscountModel discount, int price_rule_id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existedDiscountCode = await _discountService.DiscountCodeExist(discount.id);
                if (existedDiscountCode)
                {
                    return BadRequest("This discount code is already available");
                }

                var discounts = new DiscountModel
                {
                    price_rule_id = price_rule_id,
                    code = discount.code,
                    usage_count = discount.usage_count
                };

                await _discountService.CreateDiscountCode(discount);
                await _discountService.SaveChangesAsync();

                return Ok(new ResponseBase(true, "discount code created successfully."));

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner Exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while creating the discount code: {ex.Message}; Inner Exception: {innerMessage}"));
            }
        }

        [HttpGet("price_rule/{price_rule_id}/discount_code")]
        public async Task<IActionResult> GetAllDiscounts(int price_rule_id)
        {
            try
            {
                var discounts = await _discountService.GetAllDiscounts(price_rule_id);
                if(discounts == null)
                {
                    return NotFound($"No discounts found for the price rule ID {price_rule_id}");
                }
                return Ok(discounts);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving discounts: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("price_rule/{price_rule_id}/discount_code/{id}")]
        public async Task<IActionResult> GetDiscountById(int id, int price_rule_id)
        {
            try
            {
                var discount = await _discountService.GetDiscountById(id, price_rule_id);
                if (discount == null)
                {
                    return NotFound($"Discount code with ID {id} and price rule ID {price_rule_id} not found.");
                }

                return Ok(discount);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving the discount code: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("price_rule/{price_rule_id}/discount_code/{id}")]
        public async Task<IActionResult> UpdateDiscountCode(int id, [FromBody] DiscountModel discount, int price_rule_id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingDiscount = await _discountService.GetDiscountById(id, price_rule_id);
                if (existingDiscount == null)
                {
                    return NotFound($"Discount code with ID {id} and price rule ID {price_rule_id} not found.");
                }

                existingDiscount.price_rule_id = discount.price_rule_id;
                existingDiscount.code = discount.code;
                existingDiscount.usage_count = discount.usage_count;

                existingDiscount.updated_at = DateTime.Now;

                await _discountService.UpdateDiscountCode(existingDiscount);

                return Ok(new ResponseBase(true, "Discount code updated successfully."));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while updating the discount code: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpDelete("price_rule/{price_rule_id}/discount_code/{id}")]
        public async Task<IActionResult> DeleteDiscountCode(int id, int price_rule_id)
        {
            try
            {
                var existingDiscount = await _discountService.GetDiscountById(id, price_rule_id);
                if (existingDiscount == null)
                {
                    return NotFound($"Discount code with ID {id} and price rule ID {price_rule_id} not found.");
                }

                await _discountService.DeleteDiscountCode(id);

                return Ok(new ResponseBase(true, "Discount code deleted successfully."));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while deleting the discount code: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("discount_code/count")]
        public async Task<IActionResult> GetDiscountCodeCount()
        {
            try
            {
                var count = await _discountService.GetDiscountCodeCount();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving the discount code count: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }
    }
}
