using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class OrderFilterController : ControllerBase
    {
        private readonly IOrderFilterServices _orderFilterRepository;
        public OrderFilterController(IOrderFilterServices orderFilterRepository)
        {
            _orderFilterRepository = orderFilterRepository;
        }

        [HttpPost("orders/filter")]
        public async Task<IActionResult> CreateFilter([FromBody] OrderFilterModel filter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var filters = new OrderFilterModel
                {
                    name = filter.name
                };

                await _orderFilterRepository.CreateFilter(filter);
                await _orderFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter created successfully.", filter });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while creating the filter" + ex.Message);
            }
        }

        [HttpGet("orders/filters")]
        public async Task<IActionResult> GetAllFilters()
        {
            try
            {
                var filters = await _orderFilterRepository.GetAllFiltersAsync();
                return Ok(filters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while fetching the filters: " + ex.Message);
            }
        }

        [HttpGet("order/sortby")]
        public async Task<IActionResult> GetAllSortBy()
        {
            try
            {
                var sortby = await _orderFilterRepository.GetAllSortByAsync();
                return Ok(sortby);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while fetching the sorting: " + ex.Message);
            }
        }

        [HttpDelete("orders/filter/{id}")]
        public async Task<IActionResult> DeleteFilter(int id)
        {
            try
            {
                // Fetch the filter from the repository by ID
                var filter = await _orderFilterRepository.GetFilterById(id);

                // Check if the filter exists
                if (filter == null)
                {
                    return NotFound(new { message = "Filter not found." });
                }

                // Delete the filter
                _orderFilterRepository.DeleteFilter(filter);
                await _orderFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while deleting the filter: " + ex.Message);
            }
        }
    }
}
