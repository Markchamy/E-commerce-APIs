using Backend.Models;

namespace Backend.Interfaces
{
    public interface IProductFilterService
    {
        Task<ResponseBase> CreateFilter(ProductFilterModel filter);
        Task<IEnumerable<ProductFilterModel>> GetAllFiltersAsync();
        Task<IEnumerable<ProductSortByModel>> GetAllSortByAsync();
        Task<ProductFilterModel> GetFilterById(int id);
        void DeleteFilter(ProductFilterModel filter);
        Task SaveChangesAsync();
    }
}
