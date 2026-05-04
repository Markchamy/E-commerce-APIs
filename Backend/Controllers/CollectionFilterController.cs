using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class CollectionFilterController : ControllerBase
    {
        private readonly ICollectionFilterServices _collectionFilterRepository;
        public CollectionFilterController(ICollectionFilterServices collectionFilterRepository)
        {
            _collectionFilterRepository = collectionFilterRepository;
        }

        [HttpPost("collection/filter")]
        public async Task<IActionResult> CreateFilter([FromBody] CollectionFilterModel filter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var filters = new CollectionFilterModel
                {
                    Name = filter.Name
                };

                await _collectionFilterRepository.CreateFilter(filter);
                await _collectionFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter created successfully.", filter });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while creating the filter" + ex.Message);
            }
        }

        [HttpGet("collections/filters")]
        public async Task<IActionResult> GetAllFilters()
        {
            try
            {
                var filters = await _collectionFilterRepository.GetAllFiltersAsync();
                return Ok(filters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while fetching the filters: " + ex.Message);
            }
        }

        [HttpGet("collection/sortby")]
        public async Task<IActionResult> GetAllSortBy()
        {
            try
            {
                var sortby = await _collectionFilterRepository.GetAllSortingAsync();
                return Ok(sortby);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "an internal error occured while fetching the sortby: " + ex.Message);
            }
        }

        [HttpDelete("collections/filter/{id}")]
        public async Task<IActionResult> DeleteFilter(int id)
        {
            try
            {
                // Fetch the filter from the repository by ID
                var filter = await _collectionFilterRepository.GetFilterById(id);

                // Check if the filter exists
                if (filter == null)
                {
                    return NotFound(new { message = "Filter not found." });
                }

                // Delete the filter
                _collectionFilterRepository.DeleteFilter(filter);
                await _collectionFilterRepository.SaveChangesAsync();

                return Ok(new { message = "Filter deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while deleting the filter: " + ex.Message);
            }
        }
    }
}
