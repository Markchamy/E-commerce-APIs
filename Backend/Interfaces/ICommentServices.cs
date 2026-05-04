using Backend.Models;

namespace Backend.Interfaces
{
    public interface ICommentServices
    {
        Task<(List<CommentModel> comments, int total)> GetCommentsByOrderId(long orderId);
        Task<CommentModel> CreateComment(long orderId, long userId, string content, string? mentions);
        Task<CommentModel> UpdateComment(long commentId, string content);
        Task<bool> DeleteComment(long commentId);
        Task<CommentModel> GetCommentById(long commentId);
    }
}
