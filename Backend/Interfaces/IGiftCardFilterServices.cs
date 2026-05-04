using Backend.Models;

namespace Backend.Interfaces
{
    public interface IGiftCardFilterServices
{
        Task<ResponseBase> CreateFilter(GiftCardFilterModel filter);
        Task<IEnumerable<GiftCardFilterModel>> GetAllFiltersAsync();
        Task<IEnumerable<GiftCardSortByModel>> GetAllSortingAsync();
        Task<GiftCardFilterModel> GetFilterById(int id);
        void DeleteFilter(GiftCardFilterModel filter);
        Task SaveChangesAsync();
    }
}
