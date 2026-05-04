using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class BadgesServicesRepository : IBadgesService
    {
        private readonly MyDbContext _context;
        public BadgesServicesRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<ResponseBase> CreateBadges(BadgesModel badges)
        {
            try
            {
                _context.badges.Add(badges);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Badge created successfully.");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error creating the badge: {ex.Message}");
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BadgesModel>> GetAllBadgesAsync()
        {
            return await _context.badges.ToListAsync();
        }

        public async Task<BadgesModel?> GetBadgeByIdAsync(int id)
        {
            return await _context.badges.FirstOrDefaultAsync(badges => badges.Id == id);
        }

        public Task DeleteBadgeAsync(BadgesModel badge)
        {
            _context.badges.Remove(badge);
            return Task.CompletedTask;
        }

    }
}
