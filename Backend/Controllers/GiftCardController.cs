using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class GiftCardController : ControllerBase
    {
        private readonly IGiftCardServices _giftcardrepository;
        public GiftCardController(IGiftCardServices giftcardrepository)
        {
            _giftcardrepository = giftcardrepository;
        }

        [HttpPost("gift_cards")]
        public async Task<IActionResult> CreateGiftCard([FromBody] GiftCardModel giftcard)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existGiftCard = await _giftcardrepository.GiftCardExist(giftcard.id);
                if (existGiftCard)
                {
                    return BadRequest("This gift card is already available");
                }

                var gift = new GiftCardModel
                {
                    balance = giftcard.balance,
                    currency = giftcard.currency,
                    initial_value = giftcard.initial_value,
                    line_item_id = giftcard.line_item_id,
                    user_id = giftcard.user_id,
                    customer_id = giftcard.customer_id,
                    note = giftcard.note,
                    template_suffix = giftcard.template_suffix,
                    expires_on = giftcard.expires_on,
                    last_characters = giftcard.last_characters,
                    order_id = giftcard.order_id,
                    code = giftcard.code
                };

                await _giftcardrepository.CreateGiftCard(gift);
                await _giftcardrepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, "gift card created successfully.", gift));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while creating the gift card: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPost("gift_cards/{id}/disable")]
        public async Task<IActionResult> DisableGiftCard(int id)
        {
            try
            {
                var giftCard = await _giftcardrepository.GetGiftCardById(id);
                if (giftCard == null)
                {
                    return NotFound($"gift card with id {id} is not found");
                }

                giftCard.disabled_at = DateTime.Now;

                await _giftcardrepository.UpdateGiftCard(giftCard);
                await _giftcardrepository.SaveChangesAsync();

                return Ok(new ResponseBase(true, $"Gift Card {id} disabled successfully.", giftCard));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while disabling the gift card: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("gift_cards")]
        public async Task<IActionResult> GetGiftCards(
            int page = 1,
            int pageSize = 50,
            string sortBy = "CreatedDate",    // Change to the default property you want (e.g. CreatedDate or Id)
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        )
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new ResponseBase(false, "Page and pageSize must be greater than 0"));
                }

                var giftCards = await _giftcardrepository.GetGiftCards(page, pageSize, sortBy, sortDirection, filter, search);
                if (giftCards == null || !giftCards.Any())
                {
                    return Ok(new ResponseBase(true, "No gift cards found.", new List<GiftCardModel>()));
                }
                return Ok(new ResponseBase(true, "Gift cards retrieved successfully.", giftCards));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving the gift cards: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }


        [HttpGet("gift_cards/{id}")]
        public async Task<IActionResult> GetGiftCardById(int id)
        {
            try
            {
                var giftCard = await _giftcardrepository.GetGiftCardById(id);
                if (giftCard == null)
                {
                    return NotFound("Gift card not found");
                }
                return Ok(new ResponseBase(true, "Gift card retrieved successfully.", giftCard));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving the gift card: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("gift_cards/count")]
        public async Task<IActionResult> GetGiftCardCount()
        {
            try
            {
                var count = await _giftcardrepository.GetGiftCardCount();
                return Ok(new ResponseBase(true, "Gift card count retrieved successfully.", count));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving the gift card count: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("gift_cards/search")]
        public async Task<IActionResult> SearchGiftCards([FromQuery] GiftCardSearchParams searchParams)
        {
            try
            {
                var giftCards = await _giftcardrepository.SearchGiftCards(searchParams);
                if (giftCards == null || !giftCards.Any())
                {
                    return NotFound("No gift cards found matching the search criteria.");
                }
                return Ok(new ResponseBase(true, "Gift cards retrieved successfully.", giftCards));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while searching for the gift cards: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpPut("gift_card/{id}")]
        public async Task<IActionResult> UpdateGiftCard(int id, [FromBody] GiftCardModel gift)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingGiftCard = await _giftcardrepository.GetGiftCardById(id);
                if(existingGiftCard == null)
                {
                    return NotFound($"Gift card with Id {id} not found.");
                }

                existingGiftCard.expires_on = gift.expires_on;
                existingGiftCard.template_suffix = gift.template_suffix;
                existingGiftCard.initial_value = gift.initial_value;
                existingGiftCard.balance = gift.balance;
                existingGiftCard.customer_id = gift.customer_id;
                existingGiftCard.updated_at = DateTime.Now;
                existingGiftCard.currency = gift.currency;
                existingGiftCard.line_item_id = gift.line_item_id;
                existingGiftCard.user_id = gift.user_id;
                existingGiftCard.note = gift.note;
                existingGiftCard.last_characters = gift.last_characters;
                existingGiftCard.order_id = gift.order_id;

                await _giftcardrepository.UpdateGiftCard(existingGiftCard);

                return Ok(new ResponseBase(true, $"Gift Card {id} updated successfully.", existingGiftCard));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while updating the order: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

    }
}
