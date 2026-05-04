using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ProductFilterServiceRepository : IProductFilterService
    {
        private readonly MyDbContext _context;
        public ProductFilterServiceRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<ResponseBase> CreateFilter(ProductFilterModel filter)
        {
            try
            {
                _context.product_filter.Add(filter);
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
        public async Task<IEnumerable<ProductFilterModel>> GetAllFiltersAsync()
        {
            return await _context.product_filter.ToListAsync();
        }

        public async Task<IEnumerable<ProductSortByModel>> GetAllSortByAsync()
        {
            return await _context.product_sort_by.ToListAsync();
        }

        public async Task<ProductFilterModel> GetFilterById(int id)
        {
            return await _context.product_filter.FindAsync(id); // Example using Entity Framework
        }

        public void DeleteFilter(ProductFilterModel filter)
        {
            _context.product_filter.Remove(filter); // Example using Entity Framework
        }

    }
}
