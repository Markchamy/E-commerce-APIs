using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryServices _inventoryRepository;
        public InventoryController(IInventoryServices inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        [HttpGet("inventory/sortby")]
        public async Task<IActionResult> GetAllSortingAsync()
        {
            try
            {
                var sortby = await _inventoryRepository.GetAllSortingAsync();
                return Ok(sortby);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while returning the sortby: " + ex.Message);
            }
        }

        [HttpGet("products/details/all/inventory")]
        public async Task<IActionResult> GetAllProductsWithDetails(
            int page = 1,                   // Pagination: Current page
            int pageSize = 50,              // Pagination: Number of products per page
            string sortBy = "Product title",         // Default sort column
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
                var products = await _inventoryRepository.GetAllInventoryWithDetails(page, pageSize, sortBy, sortDirection, filter, search);

                // Return products or an empty list if none found
                return Ok(new ResponseBase(true, "Inventory products with details retrieved successfully", products ?? new List<ProductModel>()));
            }
            catch (Exception ex)
            {
                // Return 500 in case of an internal error
                return StatusCode(500, new ResponseBase(false, $"An internal error occurred: {ex.Message}"));
            }
        }

        [HttpPost("inventory/update-by-sku")]
        public async Task<IActionResult> UpdateInventoryBySku([FromBody] UpdateInventoryBySkuDTO request)
        {
            try
            {
                // Validate the request
                if (request == null || request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new ResponseBase(false, "Request must contain at least one item"));
                }

                // Call the repository method to update inventory
                var result = await _inventoryRepository.UpdateInventoryBySkuAsync(request);

                // Prepare response message
                string message = $"Processed {result.TotalItems} items: " +
                                $"{result.UpdatedCount} updated, " +
                                $"{result.SkippedCount} skipped (empty SKU), " +
                                $"{result.NotFoundCount} not found";

                return Ok(new ResponseBase(true, message, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseBase(false, $"An internal error occurred: {ex.Message}"));
            }
        }
    }
}
