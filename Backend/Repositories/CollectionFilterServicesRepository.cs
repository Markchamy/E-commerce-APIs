using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class CollectionFilterServicesRepository : ICollectionFilterServices
    {
        private readonly MyDbContext _context;
        public CollectionFilterServicesRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<ResponseBase> CreateFilter(CollectionFilterModel filter)
        {
            try
            {
                _context.collection_filter.Add(filter);
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
        public async Task<IEnumerable<CollectionFilterModel>> GetAllFiltersAsync()
        {
            return await _context.collection_filter.ToListAsync();
        }
        public async Task<IEnumerable<CollectionSortByModel>> GetAllSortingAsync()
        {
            return await _context.collection_sort_by.ToListAsync();
        }

        public async Task<CollectionFilterModel?> GetFilterById(int id)
        {
            return await _context.collection_filter.FindAsync(id); // Example using Entity Framework
        }

        public void DeleteFilter(CollectionFilterModel filter)
        {
            _context.collection_filter.Remove(filter); // Example using Entity Framework
        }
    }
}

