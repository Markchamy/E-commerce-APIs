using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class OrderFilterServicesRepository : IOrderFilterServices
    {
        private readonly MyDbContext _context;
        public OrderFilterServicesRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<ResponseBase> CreateFilter (OrderFilterModel filter)
        {
            try
            {
                _context.order_filter.Add(filter);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Filter created successfully.");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error creating the filter: { ex.Message}");
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<OrderFilterModel>> GetAllFiltersAsync()
        {
            return await _context.order_filter.ToListAsync();
        }
        public async Task<IEnumerable<OrderSortBy>> GetAllSortByAsync()
        {
            return await _context.order_sort_by.ToListAsync();
        }

        public async Task<OrderFilterModel> GetFilterById(int id)
        {
            return await _context.order_filter.FindAsync(id); // Example using Entity Framework
        }

        public void DeleteFilter(OrderFilterModel filter)
        {
            _context.order_filter.Remove(filter); // Example using Entity Framework
        }
    }
}
