using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class CommentServicesRepository : ICommentServices
    {
        private readonly MyDbContext _context;

        public CommentServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<(List<CommentModel> comments, int total)> GetCommentsByOrderId(long orderId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.OrderId == orderId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return (comments, comments.Count);
        }

        public async Task<CommentModel> CreateComment(long orderId, long userId, string content, string? mentions)
        {
            var comment = new CommentModel
            {
                OrderId = orderId,
                UserId = userId,
                Content = content,
                Mentions = mentions,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload with user information
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);
        }

        public async Task<CommentModel> UpdateComment(long commentId, string content)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return null;

            comment.Content = content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<bool> DeleteComment(long commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CommentModel> GetCommentById(long commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }
    }
}
