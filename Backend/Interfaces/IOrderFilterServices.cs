using Backend.Models;

namespace Backend.Interfaces
{
    public interface IOrderFilterServices
    {
        Task<ResponseBase> CreateFilter(OrderFilterModel filter);
        Task<IEnumerable<OrderFilterModel>> GetAllFiltersAsync();
        Task<IEnumerable<OrderSortBy>> GetAllSortByAsync();
        Task<OrderFilterModel> GetFilterById(int id);
        void DeleteFilter(OrderFilterModel filter);
        Task SaveChangesAsync();
    }
}
