using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Backend.Controllers
{
    [Route("admin/api/2024-01/")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly MyDbContext _context;

        public CartController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] CartItem model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new {
                        message = "Invalid model state",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                // Validate input
                if (model.UserId <= 0 || model.ProductId <= 0)
                {
                    return BadRequest(new {
                        message = "UserId and ProductId must be positive values",
                        received = new { model.UserId, model.ProductId }
                    });
                }

                // Check if the product already exists in the cart
                var existingItem = await _context.cart
                    .FirstOrDefaultAsync(x => x.ProductId == model.ProductId && x.UserId == model.UserId);

                if (existingItem != null)
                {
                    // Update quantity if the item already exists
                    existingItem.Quantity += model.Quantity;
                    _context.cart.Update(existingItem);
                }
                else
                {
                    // Add new item to the cart
                    _context.cart.Add(model);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Item added to cart successfully", cartItem = model });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Failed to add item to cart",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpDelete("empty-cart/{userId}")]
        public async Task<IActionResult> EmptyCart(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User ID is required" });
            }

            if (!long.TryParse(userId, out long userIdLong))
            {
                return BadRequest(new { message = $"User ID must be a valid integer. Received: '{userId}'" });
            }

            // Retrieve all cart items for the user
            var cartItems = await _context.cart
                .Where(ci => ci.UserId == userIdLong)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return Ok(new { message = "Cart is already empty" });
            }

            // Remove all items from the cart
            _context.cart.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart emptied successfully" });
        }

        [HttpGet("get-cart/{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User ID is required" });
            }

            if (!long.TryParse(userId, out long userIdLong))
            {
                return BadRequest(new { message = $"User ID must be a valid integer. Received: '{userId}'" });
            }

            // Retrieve all cart items for the user
            var cartItems = await _context.cart
                .Where(ci => ci.UserId == userIdLong)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return Ok(new { message = "Cart is empty", cartItems = new List<object>() });
            }

            // Process the cart items and safely calculate the total price
            var cartResponse = cartItems.Select(ci =>
            {
                decimal price = 0;
                if (!decimal.TryParse(ci.Price, out price))
                {
                    price = 0; // Default to 0 if parsing fails
                }

                return new
                {
                    ci.Id,
                    ci.ProductId,
                    ci.Quantity,
                    ci.Price,
                    TotalPrice = ci.Quantity * price
                };
            }).ToList();

            return Ok(new { message = "Cart retrieved successfully", cartItems = cartResponse });
        }

        [HttpDelete("remove-item/{userId}/{productId}")]
        public async Task<IActionResult> RemoveItem(string userId, string productId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
            {
                return BadRequest(new { message = "User ID and Product ID are required" });
            }

            // Parse userId and productId to long
            if (!long.TryParse(userId, out long userIdLong))
            {
                return BadRequest(new { message = $"User ID must be a valid integer. Received: '{userId}'" });
            }

            if (!long.TryParse(productId, out long productIdLong))
            {
                return BadRequest(new { message = $"Product ID must be a valid integer. Received: '{productId}'" });
            }

            // Find the cart item matching the userId and productId
            var cartItem = await _context.cart
                .FirstOrDefaultAsync(ci => ci.UserId == userIdLong && ci.ProductId == productIdLong);

            if (cartItem == null)
            {
                // Get all cart items for this user to help with debugging
                var userCartItems = await _context.cart
                    .Where(ci => ci.UserId == userIdLong)
                    .Select(ci => new { ci.Id, ci.ProductId, ci.UserId, ci.Quantity })
                    .ToListAsync();

                return NotFound(new {
                    message = $"Item not found in the cart for UserId: {userIdLong}, ProductId: {productIdLong}",
                    searchedFor = new { userId = userIdLong, productId = productIdLong },
                    userCartItems = userCartItems,
                    hint = userCartItems.Any()
                        ? "User has cart items but product ID doesn't match"
                        : "User has no cart items"
                });
            }

            // Remove the item from the cart
            _context.cart.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item removed from the cart successfully" });
        }



    }

}
