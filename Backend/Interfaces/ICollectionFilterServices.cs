using Backend.Models;

namespace Backend.Interfaces
{
    public interface ICollectionFilterServices
    {
        Task<ResponseBase> CreateFilter(CollectionFilterModel filter);
        Task<IEnumerable<CollectionFilterModel>> GetAllFiltersAsync();
        Task<IEnumerable<CollectionSortByModel>> GetAllSortingAsync();
        Task<CollectionFilterModel?> GetFilterById(int id);
        void DeleteFilter(CollectionFilterModel filter);
        Task SaveChangesAsync();
    }
}
