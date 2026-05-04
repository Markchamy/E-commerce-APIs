using Backend.Models;

namespace Backend.Interfaces
{
    public interface IBadgesService
    {
        Task<ResponseBase> CreateBadges(BadgesModel badges);
        Task SaveChangesAsync();
        Task<IEnumerable<BadgesModel>> GetAllBadgesAsync();
        Task<BadgesModel?> GetBadgeByIdAsync(int id);
        Task DeleteBadgeAsync(BadgesModel badge);
    }
}
