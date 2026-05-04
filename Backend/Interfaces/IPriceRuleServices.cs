using Backend.Models;

namespace Backend.Interfaces
{
    public interface IPriceRuleServices
    {
        Task<bool> PriceRuleExist(int id);
        Task<ResponseBase> CreatePriceRule(PriceRuleModel priceRule);
        Task<List<PriceRuleModel>> GetAllPriceRules();
        Task<PriceRuleModel> GetPriceRuleById(int id);
        Task<int> GetPriceRuleCount();
        Task UpdatePriceRule(PriceRuleModel priceRule);
        Task DeletePriceRule(int id);
        Task SaveChangesAsync();
    }
}
