using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class DiscountServicesRepository : IDiscountServices
    {
        private readonly MyDbContext _context;
        public DiscountServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> DiscountCodeExist(int id)
        {
            return await _context.discount_codes.AnyAsync(discount => discount.id == id);
        }

        public async Task<ResponseBase> CreateDiscountCode(DiscountModel discount)
        {
            try
            {
                _context.discount_codes.Add(discount);
                await _context.SaveChangesAsync();

                return ResponseBase.Success("discount code created successfully.", discount);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure("Error creating the discount code" + ex.Message);
            }
        }
        public async Task<List<DiscountModel>> GetAllDiscounts(int price_rule_id)
        {
            return await _context.discount_codes
                .Where(discount => discount.price_rule_id == price_rule_id)
                .ToListAsync();
        }
        public async Task<DiscountModel> GetDiscountById(int id, int price_rule_id)
        {
            return await _context.discount_codes.
                FirstOrDefaultAsync(discount => discount.id == id && discount.price_rule_id == price_rule_id);
        }

        public async Task UpdateDiscountCode(DiscountModel discount)
        {
            _context.discount_codes.Update(discount);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteDiscountCode(int id)
        {
            var discount = await _context.discount_codes.FindAsync(id);
            if (discount != null)
            {
                _context.discount_codes.Remove(discount);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetDiscountCodeCount()
        {
            return await _context.discount_codes.CountAsync();
        }

    }
}
