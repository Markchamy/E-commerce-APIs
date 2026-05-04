using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class PriceRuleController : ControllerBase
    {
        private readonly IPriceRuleServices _priceRuleRepository;
        public PriceRuleController(IPriceRuleServices priceRuleRepository)
        {
            _priceRuleRepository = priceRuleRepository;
        }

        [HttpPost("price_rules")]
        public async Task<IActionResult> CreatePriceRule([FromBody] PriceRuleModel priceRule)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingPriceRule = await _priceRuleRepository.PriceRuleExist(priceRule.id);
                if (existingPriceRule)
                {
                    return BadRequest("This price rule is already available.");
                }

                var price = new PriceRuleModel
                {
                    value_type = priceRule.value_type,
                    value = priceRule.value,
                    customer_selection = priceRule.customer_selection,
                    target_type = priceRule.target_type,
                    target_selection = priceRule.target_selection,
                    allocation_method = priceRule.allocation_method,
                    allocation_limit = priceRule.allocation_limit,
                    once_per_customer = priceRule.once_per_customer,
                    usage_limit = priceRule.usage_limit,
                    starts_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    entitled_product_ids = priceRule.entitled_product_ids,
                    entitled_variant_ids = priceRule.entitled_variant_ids,
                    entitled_collection_ids = priceRule.entitled_collection_ids,
                    entitled_country_ids = priceRule.entitled_country_ids,
                    prerequisite_product_ids = priceRule.prerequisite_product_ids,
                    prerequisite_variant_ids = priceRule.prerequisite_variant_ids,
                    prerequisite_collection_ids = priceRule.prerequisite_collection_ids,
                    customer_segment_prerequisite_ids = priceRule.customer_segment_prerequisite_ids,
                    prerequisite_customer_ids = priceRule.prerequisite_customer_ids,
                    prerequisite_subtotal_range = priceRule.prerequisite_subtotal_range,
                    prerequisite_quantity_range = priceRule.prerequisite_quantity_range,
                    prerequisite_shipping_price_range = priceRule.prerequisite_shipping_price_range,
                    entitlement_quantity = priceRule.entitlement_quantity.Select(quantity => new EntitlementQuantityModel
                    {
                        price_rule_id = priceRule.id,
                        prerequisite_quantity = quantity.prerequisite_quantity,
                        entitled_quantity = quantity.entitled_quantity,
                    }).ToList(),
                    entitlement_purchase = priceRule.entitlement_purchase.Select(purchase => new EntitlementPurchaseModel
                    {
                        price_rule_id = priceRule.id,
                        prerequisite_amount = purchase.prerequisite_amount,
                    }).ToList(),
                    title = priceRule.title,
                };

                await _priceRuleRepository.CreatePriceRule(priceRule);
                await _priceRuleRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "price rule was created successfully.", priceRule));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while creating the price rule: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("price_rules")]
        public async Task<IActionResult> GetAllPriceRules()
        {
            try
            {
                var priceRules = await _priceRuleRepository.GetAllPriceRules();
                return Ok(priceRules);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while retrieving all the price rules: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("price_rules/{id}")]
        public async Task<IActionResult> GetPriceRuleById(int id)
        {
            try
            {
                var priceRule = await _priceRuleRepository.GetPriceRuleById(id);
                if (priceRule == null)
                {
                    return NotFound($"Price rule with ID {id} not found.");
                }

                return Ok(priceRule);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while retrieving the price rule: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("price_rules/count")]
        public async Task<IActionResult> GetPriceRuleCount()
        {
            try
            {
                var count = await _priceRuleRepository.GetPriceRuleCount();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while retrieving the count of the price rule: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("price_rule/{id}")]
        public async Task<IActionResult> UpdatePriceRule(int id, [FromBody] PriceRuleModel priceRule)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingPriceRule = await _priceRuleRepository.GetPriceRuleById(id);
                if (existingPriceRule == null)
                {
                    return NotFound($"Price rule with ID {id} not found.");
                }

                existingPriceRule.value_type = priceRule.value_type;
                existingPriceRule.value = priceRule.value;
                existingPriceRule.customer_selection = priceRule.customer_selection;
                existingPriceRule.target_type = priceRule.target_type;
                existingPriceRule.target_selection = priceRule.target_selection;
                existingPriceRule.allocation_method = priceRule.allocation_method;
                existingPriceRule.allocation_limit = priceRule.allocation_limit;
                existingPriceRule.once_per_customer = priceRule.once_per_customer;
                existingPriceRule.usage_limit = priceRule.usage_limit;
                existingPriceRule.starts_at = priceRule.starts_at;
                existingPriceRule.ends_at = priceRule.ends_at;
                existingPriceRule.updated_at = priceRule.updated_at;
                existingPriceRule.entitled_product_ids = priceRule.entitled_product_ids;
                existingPriceRule.entitled_variant_ids = priceRule.entitled_variant_ids;
                existingPriceRule.entitled_collection_ids = priceRule.entitled_collection_ids;
                existingPriceRule.customer_segment_prerequisite_ids = priceRule.customer_segment_prerequisite_ids;
                existingPriceRule.prerequisite_customer_ids = priceRule.prerequisite_customer_ids;
                existingPriceRule.prerequisite_subtotal_range = priceRule.prerequisite_subtotal_range;
                existingPriceRule.prerequisite_quantity_range = priceRule.prerequisite_quantity_range;
                existingPriceRule.prerequisite_shipping_price_range = priceRule.prerequisite_shipping_price_range;
                existingPriceRule.entitlement_quantity = priceRule.entitlement_quantity.Select(quantity => new EntitlementQuantityModel
                {
                    price_rule_id = priceRule.id,
                    prerequisite_quantity = quantity.prerequisite_quantity,
                    entitled_quantity = quantity.entitled_quantity,
                }).ToList();
                existingPriceRule.entitlement_purchase = priceRule.entitlement_purchase.Select(purchase => new EntitlementPurchaseModel
                {
                    price_rule_id = priceRule.id,
                    prerequisite_amount = purchase.prerequisite_amount,
                }).ToList();
                existingPriceRule.title = priceRule.title;

                await _priceRuleRepository.UpdatePriceRule(existingPriceRule);
                await _priceRuleRepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "Price rule was updated successfully.", existingPriceRule));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while updating the price rule: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpDelete("price_rule/{id}")]
        public async Task<IActionResult> DeletePriceRule(int id)
        {
            try
            {
                var existingPriceRule = await _priceRuleRepository.GetPriceRuleById(id);
                if (existingPriceRule == null)
                {
                    return NotFound($"price rule with ID {id} not found.");
                }

                await _priceRuleRepository.DeletePriceRule(id);

                return Ok(new ResponseBase(true, "Price rule deleted successfully."));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while deleting the price rule: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }
    }
}
