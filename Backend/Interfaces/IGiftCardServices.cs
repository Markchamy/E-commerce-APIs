using Backend.Models;

namespace Backend.Interfaces
{
    public interface IGiftCardServices
    {
        Task<bool> GiftCardExist(int id);
        Task<ResponseBase> CreateGiftCard(GiftCardModel gift);
        Task<GiftCardModel> GetGiftCardById(int id);
        Task<IEnumerable<GiftCardModel>> GetGiftCards(
            int page = 1,
            int pageSize = 50,
            string sortBy = "CreatedDate",
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        );
        Task UpdateGiftCard(GiftCardModel gift);
        Task<int> GetGiftCardCount();
        Task<IEnumerable<GiftCardModel>> SearchGiftCards(GiftCardSearchParams searchParams);
        Task SaveChangesAsync();
    }
}
