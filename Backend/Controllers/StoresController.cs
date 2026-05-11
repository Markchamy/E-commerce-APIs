using System.Security.Claims;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    [Authorize]
    public class StoresController : ControllerBase
    {
        private static readonly string[] SuperAdminRoles = { "admin", "manager" };

        private readonly MyDbContext _db;

        public StoresController(MyDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Lists stores visible to the current user.
        ///   - Super-admins (role=admin/manager): all active stores.
        ///   - Everyone else: just the one store from their JWT store_id claim.
        /// This is the data backing the CMS store-switcher dropdown.
        /// </summary>
        [HttpGet("stores")]
        public async Task<IActionResult> GetAll()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var isSuperAdmin = role != null
                && SuperAdminRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

            var query = _db.Stores.Where(s => s.IsActive);

            if (!isSuperAdmin)
            {
                var storeIdClaim = User.FindFirst("store_id")?.Value;
                if (!int.TryParse(storeIdClaim, out var storeId) || storeId <= 0)
                {
                    return Ok(Array.Empty<object>());
                }
                query = query.Where(s => s.Id == storeId);
            }

            var stores = await query
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    slug = s.Slug,
                    domain = s.Domain,
                    logoUrl = s.LogoUrl,
                    primaryColor = s.PrimaryColor,
                    currency = s.Currency,
                    locale = s.Locale,
                })
                .ToListAsync();

            return Ok(stores);
        }
    }
}
