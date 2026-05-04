using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class GiftCardFilterController : ControllerBase
    {
        private readonly IGiftCardFilterServices _giftCardFilterRepository;
        public GiftCardFilterController(IGiftCardFilterServices giftCardFilterRepository)
        {
            _giftCardFilterRepository = giftCardFilterRepository;
        }

        [HttpPost("giftCard/filter")]
        public async Task<IActionResult> CreateFilter([FromBody] GiftCardFilterModel filter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var filters = new GiftCardFilterModel
                {
                    Name = filter.Name,
                };

                await _giftCardFilterRepository.CreateFilter(filter);
                await _giftCardFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter created successfully.", filter });
            }
            catch (Exception ex) {
                return StatusCode(500, "An internal error occured while creating the filter");
            }
        }

        [HttpGet("giftCard/filters")]
        public async Task<IActionResult> GetAllFilters()
        {
            try
            {
                var filters = await _giftCardFilterRepository.GetAllFiltersAsync();
                return Ok(filters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while fetching the filters: " + ex.Message);
            }
        }

        [HttpGet("giftCard/sortby")]
        public async Task<IActionResult> GetAllSortBy()
        {
            try
            {
                var sortby = await _giftCardFilterRepository.GetAllSortingAsync();
                return Ok(sortby);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "an internal error occured while fetching the sortby: " + ex.Message);
            }
        }

        [HttpDelete("giftCard/filter/{id}")]
        public async Task<IActionResult> DeleteFilter(int id)
        {
            try
            {
                // Fetch the filter from the repository by ID
                var filter = await _giftCardFilterRepository.GetFilterById(id);

                // Check if the filter exists
                if (filter == null)
                {
                    return NotFound(new { message = "Filter not found." });
                }

                // Delete the filter
                _giftCardFilterRepository.DeleteFilter(filter);
                await _giftCardFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while deleting the filter: " + ex.Message);
            }
        }
    }
}
