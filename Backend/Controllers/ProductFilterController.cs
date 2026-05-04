using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class ProductFilterController : ControllerBase
    {
        private readonly IProductFilterService _productFilterRepository;
        public ProductFilterController(IProductFilterService productFilterRepository)
        { 
            _productFilterRepository = productFilterRepository;
        }

        [HttpPost("products/filter")]
        public async Task<IActionResult> CreateFilter([FromBody] ProductFilterModel filter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var filters = new ProductFilterModel
                {
                    Name = filter.Name
                };

                await _productFilterRepository.CreateFilter(filter);
                await _productFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter created successfully.", filter });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while creating the filter" + ex.Message);
            }
        }

        [HttpGet("products/filters")]
        public async Task<IActionResult> GetAllFilters()
        {
            try
            {
                var filters = await _productFilterRepository.GetAllFiltersAsync();
                return Ok(filters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while fetching the filters: " + ex.Message);
            }
        }

        [HttpGet("products/sortby")]
        public async Task<IActionResult> GetAllProductSortBy()
        {
            try
            {
                var sortby = await _productFilterRepository.GetAllSortByAsync();
                return Ok(sortby);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while fetching the sortby: " + ex.Message);
            }
        }

        [HttpDelete("products/filter/{id}")]
        public async Task<IActionResult> DeleteFilter(int id)
        {
            try
            {
                // Fetch the filter from the repository by ID
                var filter = await _productFilterRepository.GetFilterById(id);

                // Check if the filter exists
                if (filter == null)
                {
                    return NotFound(new { message = "Filter not found." });
                }

                // Delete the filter
                _productFilterRepository.DeleteFilter(filter);
                await _productFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while deleting the filter: " + ex.Message);
            }
        }

    }
}
