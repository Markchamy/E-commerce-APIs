using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentServices _commentServices;

        public CommentsController(ICommentServices commentServices)
        {
            _commentServices = commentServices;
        }

        // GET /orders/:orderId/comments
        [HttpGet("orders/{orderId}/comments")]
        public async Task<IActionResult> GetCommentsByOrderId(long orderId)
        {
            try
            {
                var (comments, total) = await _commentServices.GetCommentsByOrderId(orderId);

                return Ok(new
                {
                    comments = comments.Select(c => new
                    {
                        id = c.Id,
                        orderId = c.OrderId,
                        userId = c.UserId,
                        userName = c.User != null ? $"{c.User.first_name} {c.User.last_name}".Trim() : "Unknown",
                        content = c.Content,
                        mentions = c.Mentions,
                        createdAt = c.CreatedAt,
                        updatedAt = c.UpdatedAt
                    }),
                    total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error fetching comments: {ex.Message}" });
            }
        }

        // POST /comments
        [HttpPost("comments")]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var mentions = dto.mentions != null ? string.Join(",", dto.mentions) : null;
                var comment = await _commentServices.CreateComment(dto.orderId, dto.userId, dto.content, mentions);

                return Ok(new
                {
                    data = new
                    {
                        id = comment.Id,
                        orderId = comment.OrderId,
                        userId = comment.UserId,
                        userName = comment.User != null ? $"{comment.User.first_name} {comment.User.last_name}".Trim() : "Unknown",
                        content = comment.Content,
                        mentions = comment.Mentions,
                        createdAt = comment.CreatedAt,
                        updatedAt = comment.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error creating comment: {ex.Message}" });
            }
        }

        // PUT /comments/:commentId
        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(long commentId, [FromBody] UpdateCommentDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var comment = await _commentServices.UpdateComment(commentId, dto.content);

                if (comment == null)
                    return NotFound(new { error = "Comment not found" });

                return Ok(new
                {
                    data = new
                    {
                        id = comment.Id,
                        orderId = comment.OrderId,
                        userId = comment.UserId,
                        userName = comment.User != null ? $"{comment.User.first_name} {comment.User.last_name}".Trim() : "Unknown",
                        content = comment.Content,
                        mentions = comment.Mentions,
                        createdAt = comment.CreatedAt,
                        updatedAt = comment.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error updating comment: {ex.Message}" });
            }
        }

        // DELETE /comments/:commentId
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(long commentId)
        {
            try
            {
                var deleted = await _commentServices.DeleteComment(commentId);

                if (!deleted)
                    return NotFound(new { error = "Comment not found" });

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error deleting comment: {ex.Message}" });
            }
        }
    }

    // DTOs
    public class CreateCommentDTO
    {
        public long orderId { get; set; }
        public long userId { get; set; }
        public string content { get; set; }
        public string[]? mentions { get; set; }
    }

    public class UpdateCommentDTO
    {
        public string content { get; set; }
    }
}
