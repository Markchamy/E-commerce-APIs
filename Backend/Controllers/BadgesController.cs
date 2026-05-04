using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class BadgesController : ControllerBase
    {
        private readonly IBadgesService _badgesRepository;
        public BadgesController(IBadgesService badgesRepository)
        {
            _badgesRepository = badgesRepository;
        }
        [HttpPost("badges")]
        public async Task<IActionResult> CreateBadges([FromBody] BadgesModel badges)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var badge = new BadgesModel
                {
                    body_html = badges.body_html
                };

                await _badgesRepository.CreateBadges(badge);
                await _badgesRepository.SaveChangesAsync();

                return Ok(new { message = "Badges created successfully.", badge });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occured while creating the badge" + ex.Message);
            }
        }

        [HttpGet("badges")]
        public async Task<IActionResult> GetAllBadges()
        {
            try
            {
                var badges = await _badgesRepository.GetAllBadgesAsync();
                return Ok(badges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred while fetching the badges: " + ex.Message);
            }
        }

        [HttpGet("badges/{id}")]
        public async Task<IActionResult> GetBadgeById(int id)
        {
            try
            {
                var badge = await _badgesRepository.GetBadgeByIdAsync(id);

                if (badge == null)
                {
                    return NotFound($"Badge with ID {id} not found.");
                }

                return Ok(badge);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal error occurred while fetching the badge: {ex.Message}");
            }
        }


        [HttpDelete("badges/{id}")]
        public async Task<IActionResult> DeleteBadge(int id)
        {
            try
            {
                // Fetch the badge by ID
                var badge = await _badgesRepository.GetBadgeByIdAsync(id);

                // Check if the badge exists
                if (badge == null)
                {
                    return NotFound(new { message = $"Badge with ID {id} not found." });
                }

                // Delete the badge
                await _badgesRepository.DeleteBadgeAsync(badge);
                await _badgesRepository.SaveChangesAsync();

                // Return a success response
                return Ok(new { message = $"Badge with ID {id} has been deleted successfully." });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, "An internal error occurred while deleting the badge: " + ex.Message);
            }
        }

    }
}
