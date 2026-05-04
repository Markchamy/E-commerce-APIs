using Backend.Models;

namespace Backend.Interfaces
{
    public interface IDiscountServices
    {
        Task<bool> DiscountCodeExist(int id);
        Task<ResponseBase> CreateDiscountCode(DiscountModel discount);
        Task<List<DiscountModel>> GetAllDiscounts(int price_rule_id);
        Task<DiscountModel> GetDiscountById(int id, int price_rule_id);
        Task UpdateDiscountCode(DiscountModel discount);
        Task DeleteDiscountCode(int id);
        Task<int> GetDiscountCodeCount();
        Task SaveChangesAsync();
    }
}
