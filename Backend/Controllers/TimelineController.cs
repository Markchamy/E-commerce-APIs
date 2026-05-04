using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class TimelineController : ControllerBase
    {
        private readonly ITimelineServices _timelineServices;

        public TimelineController(ITimelineServices timelineServices)
        {
            _timelineServices = timelineServices;
        }

        /// <summary>
        /// GET /admin/api/2024-01/orders/{orderId}/timeline
        /// Retrieves all timeline events for a specific order
        /// </summary>
        [HttpGet("orders/{orderId}/timeline")]
        public async Task<IActionResult> GetTimelineEventsByOrderId(long orderId)
        {
            try
            {
                var (events, total) = await _timelineServices.GetTimelineEventsByOrderId(orderId);

                return Ok(new
                {
                    events = events.Select(e => new
                    {
                        id = e.Id,
                        orderId = e.OrderId,
                        userId = e.UserId,
                        userName = e.UserName,
                        eventType = e.EventType,
                        description = e.Description,
                        metadata = e.Metadata,
                        createdAt = e.CreatedAt
                    }),
                    total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error fetching timeline events: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST /admin/api/2024-01/orders/{orderId}/timeline
        /// Creates a new timeline event for an order
        /// </summary>
        [HttpPost("orders/{orderId}/timeline")]
        public async Task<IActionResult> CreateTimelineEvent(long orderId, [FromBody] CreateTimelineEventDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Override orderId from route parameter
                dto.OrderId = orderId;

                var timelineEvent = await _timelineServices.CreateTimelineEvent(dto);

                return Ok(new
                {
                    data = new
                    {
                        id = timelineEvent.Id,
                        orderId = timelineEvent.OrderId,
                        userId = timelineEvent.UserId,
                        userName = timelineEvent.UserName,
                        eventType = timelineEvent.EventType,
                        description = timelineEvent.Description,
                        metadata = timelineEvent.Metadata,
                        createdAt = timelineEvent.CreatedAt
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error creating timeline event: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /admin/api/2024-01/timeline/{eventId}
        /// Retrieves a specific timeline event by ID
        /// </summary>
        [HttpGet("timeline/{eventId}")]
        public async Task<IActionResult> GetTimelineEventById(int eventId)
        {
            try
            {
                var timelineEvent = await _timelineServices.GetTimelineEventById(eventId);

                if (timelineEvent == null)
                    return NotFound(new { error = "Timeline event not found" });

                return Ok(new
                {
                    data = new
                    {
                        id = timelineEvent.Id,
                        orderId = timelineEvent.OrderId,
                        userId = timelineEvent.UserId,
                        userName = timelineEvent.UserName,
                        eventType = timelineEvent.EventType,
                        description = timelineEvent.Description,
                        metadata = timelineEvent.Metadata,
                        createdAt = timelineEvent.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error fetching timeline event: {ex.Message}" });
            }
        }

        /// <summary>
        /// DELETE /admin/api/2024-01/timeline/{eventId}
        /// Deletes a timeline event (admin only)
        /// </summary>
        [HttpDelete("timeline/{eventId}")]
        public async Task<IActionResult> DeleteTimelineEvent(int eventId)
        {
            try
            {
                var deleted = await _timelineServices.DeleteTimelineEvent(eventId);

                if (!deleted)
                    return NotFound(new { error = "Timeline event not found" });

                return Ok(new
                {
                    success = true,
                    message = "Timeline event deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error deleting timeline event: {ex.Message}" });
            }
        }
    }
}
