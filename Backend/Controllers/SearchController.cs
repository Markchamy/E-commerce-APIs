using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchServices _searchServices;

        public SearchController(ISearchServices searchServices)
        {
            _searchServices = searchServices;
        }

        [HttpPost("search/global")]
        public async Task<IActionResult> GlobalSearch([FromBody] GlobalSearchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ResponseBase(false, "Search request is required", null));
                }

                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new ResponseBase(false, "Search query cannot be empty", null));
                }

                var result = await _searchServices.GlobalSearchAsync(request);

                return Ok(new ResponseBase(true, "Search completed successfully", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseBase(false, $"An error occurred during search: {ex.Message}", null));
            }
        }

        [HttpGet("search/global")]
        public async Task<IActionResult> GlobalSearchGet(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] List<string> filters = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new ResponseBase(false, "Search query cannot be empty", null));
                }

                var request = new GlobalSearchRequest
                {
                    Query = query,
                    Page = page,
                    Limit = limit,
                    Filters = filters
                };

                var result = await _searchServices.GlobalSearchAsync(request);

                return Ok(new ResponseBase(true, "Search completed successfully", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseBase(false, $"An error occurred during search: {ex.Message}", null));
            }
        }
    }
}
