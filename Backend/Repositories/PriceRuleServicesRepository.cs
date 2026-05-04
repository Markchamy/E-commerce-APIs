using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PriceRuleServicesRepository : IPriceRuleServices
    {
        private readonly MyDbContext _context;
        public PriceRuleServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PriceRuleExist(int id)
        {
            return await _context.price_rule.AnyAsync(price => price.id == id);
        }

        public async Task<ResponseBase> CreatePriceRule(PriceRuleModel price)
        {
            try
            {
                _context.price_rule.Add(price);
                await _context.SaveChangesAsync();

                return ResponseBase.Success("Price Rule have been created successfully.", price);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure("Error occured while creating the Price Rule." + ex.Message);
            }
        }
        public async Task<List<PriceRuleModel>> GetAllPriceRules()
        {
            return await _context.price_rule
                .Include(priceRule => priceRule.entitlement_purchase)
                .Include(priceRule => priceRule.entitlement_quantity)
                .Include(priceRule => priceRule.discount)
                .ToListAsync();
        }
        public async Task<PriceRuleModel> GetPriceRuleById(int id)
        {
            return await _context.price_rule
                                 .Include(priceRule => priceRule.entitlement_quantity)
                                 .Include(priceRule => priceRule.entitlement_purchase)
                                 .Include(priceRule => priceRule.discount)
                                 .FirstOrDefaultAsync(priceRule => priceRule.id == id);
        }
        public async Task<int> GetPriceRuleCount()
        {
            return await _context.price_rule.CountAsync();
        }
        public async Task UpdatePriceRule(PriceRuleModel priceRule)
        {
            _context.price_rule.Update(priceRule);
            await _context.SaveChangesAsync();
        }
        public async Task DeletePriceRule(int id)
        {
            var priceRule = await _context.price_rule.FindAsync(id);
            if (priceRule != null)
            {
                _context.price_rule.Remove(priceRule);
                await _context.SaveChangesAsync();
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
