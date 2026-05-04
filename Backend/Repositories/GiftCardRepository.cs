using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class GiftCardRepository : IGiftCardServices
    {
        private readonly MyDbContext _context;

        public GiftCardRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> GiftCardExist(int id)
        {
            return await _context.gift_card.AnyAsync(gift => gift.id == id);
        }

        public async Task<ResponseBase> CreateGiftCard(GiftCardModel gift)
        {
            try
            {
                _context.gift_card.Add(gift);
                await _context.SaveChangesAsync();

                return ResponseBase.Success("Gift card have been created successfully.", gift);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure("Error creating the gift: " + ex.Message);
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<GiftCardModel> GetGiftCardById(int id)
        {
            // Make sure to use `await` with `FirstOrDefaultAsync`
            return await _context.gift_card.FirstOrDefaultAsync(gift => gift.id== id);
        }

        public async Task UpdateGiftCard(GiftCardModel gift)
        {
            _context.Entry(gift).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GiftCardModel>> GetGiftCards(
            int page = 1,
            int pageSize = 50,
            string sortBy = "CreatedDate",
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        )
        {
            var skip = (page - 1) * pageSize;
            IQueryable<GiftCardModel> query = _context.gift_card.AsQueryable();

            // 1. Apply filtering (example: by Status)
            if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
            {
                switch (filter.ToLower())
                {
                    case "Empty":
                        query = query.Where(gc => gc.balance == "0.00" || gc.balance == "0");
                        break;
                    case "Full":
                        query = query.Where(gc => Convert.ToDecimal(gc.initial_value) == Convert.ToDecimal(gc.balance));
                        break;
                    // Add more filter cases as needed.
                    default:
                        query = query.Where(gc => false); // returns no results for unknown filters
                        break;
                }
            }

            // 2. Apply searching (example: search in Code or OwnerName)
            if (!string.IsNullOrEmpty(search))
            {
                string lowerCaseSearch = search.ToLower();
                query = query.Where(gc =>
                    (gc.code != null && gc.code.ToLower().Contains(lowerCaseSearch)) ||
                    (gc.balance != null && gc.balance.ToLower().Contains(lowerCaseSearch))
                );
            }

            // 3. Apply sorting
            switch (sortBy?.ToLower())
            {
                case "Gift code ending":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(gc => gc.last_characters)
                        : query.OrderBy(gc => gc.last_characters);
                    break;
                case "Date created":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(gc => gc.created_at)
                        : query.OrderBy(gc => gc.created_at);
                    break;
                case "Date edited":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(gc => gc.updated_at)
                        : query.OrderBy(gc => gc.updated_at);
                    break;
                case "Expiry date":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(gc => gc.expires_on)
                        : query.OrderBy(gc => gc.expires_on);
                    break;
                case "Total balance":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(gc => gc.balance)
                        : query.OrderBy(gc => gc.balance);
                    break;
                default:
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(gc => gc.id)
                        : query.OrderBy(gc => gc.id);
                    break;
            }

            // 4. Apply pagination
            query = query.Skip(skip).Take(pageSize);

            // 5. Map to GiftCardModel (adjust properties accordingly)
            return await query.Select(gc => new GiftCardModel
            {
                id = gc.id,
                code = gc.code,
                balance = gc.balance,
                initial_value = gc.initial_value,
                created_at = gc.created_at,
                disabled_at = gc.disabled_at,
                expires_on = gc.expires_on,
                last_characters = gc.last_characters,
                note = gc.note,
                user_id = gc.user_id,
                customer_id = gc.customer_id,
                order_id = gc.order_id,
                currency = gc.currency,
                line_item_id = gc.line_item_id,
                updated_at = gc.updated_at,
                template_suffix = gc.template_suffix,
                // Map additional properties as needed.
            }).ToListAsync();
        }


        public async Task<int> GetGiftCardCount()
        {
            return await _context.gift_card.CountAsync();
        }

        public async Task<IEnumerable<GiftCardModel>> SearchGiftCards(GiftCardSearchParams searchParams)
        {
            var query = _context.gift_card.AsQueryable();

            if (searchParams.CreatedAt.HasValue)
            {
                query = query.Where(gc => gc.created_at == searchParams.CreatedAt.Value);
            }
            if (searchParams.UpdatedAt.HasValue)
            {
                query = query.Where(gc => gc.updated_at == searchParams.UpdatedAt.Value);
            }
            if (searchParams.DisabledAt.HasValue)
            {
                query = query.Where(gc => gc.disabled_at == searchParams.DisabledAt.Value);
            }
            if (!string.IsNullOrEmpty(searchParams.LastCharacters))
            {
                query = query.Where(gc => gc.last_characters.EndsWith(searchParams.LastCharacters));
            }

            return await query.ToListAsync();
        }


    }
}
