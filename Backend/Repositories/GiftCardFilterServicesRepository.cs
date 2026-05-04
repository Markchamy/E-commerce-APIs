using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class GiftCardFilterServicesRepository : IGiftCardFilterServices
    {
        private readonly MyDbContext _context;
        public GiftCardFilterServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseBase> CreateFilter(GiftCardFilterModel filter)
        {
            try
            {
                _context.giftcard_filter.Add(filter);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Filter created successfully.");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error creating the filter: {ex.Message}");
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<GiftCardFilterModel>> GetAllFiltersAsync()
        {
            return await _context.giftcard_filter.ToListAsync();
        }
        public async Task<IEnumerable<GiftCardSortByModel>> GetAllSortingAsync()
        {
            return await _context.giftCard_sort_by.ToListAsync();
        }

        public async Task<GiftCardFilterModel> GetFilterById(int id)
        {
            return await _context.giftcard_filter.FindAsync(id); // Example using Entity Framework
        }

        public void DeleteFilter(GiftCardFilterModel filter)
        {
            _context.giftcard_filter.Remove(filter); // Example using Entity Framework
        }
    }
}
