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

        private bool IsSuperAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return role != null && SuperAdminRoles.Any(r =>
                string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
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
            var isSuperAdmin = IsSuperAdmin();

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
                    isActive = s.IsActive,
                })
                .ToListAsync();

            return Ok(stores);
        }

        public class StoreUpsertDTO
        {
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string? Domain { get; set; }
            public string? LogoUrl { get; set; }
            public string? PrimaryColor { get; set; }
            public string? Currency { get; set; }
            public string? Locale { get; set; }
            public bool IsActive { get; set; } = true;
        }

        /// <summary>Create a new tenant. Super-admin only.</summary>
        [HttpPost("stores")]
        public async Task<IActionResult> Create([FromBody] StoreUpsertDTO dto)
        {
            if (!IsSuperAdmin()) return Forbid();
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
            if (string.IsNullOrWhiteSpace(dto.Slug)) return BadRequest("Slug is required.");

            var slugTaken = await _db.Stores.AnyAsync(s => s.Slug == dto.Slug);
            if (slugTaken) return Conflict($"Slug '{dto.Slug}' is already in use.");

            var store = new StoreModel
            {
                Name = dto.Name.Trim(),
                Slug = dto.Slug.Trim().ToLowerInvariant(),
                Domain = string.IsNullOrWhiteSpace(dto.Domain) ? null : dto.Domain.Trim(),
                LogoUrl = dto.LogoUrl,
                PrimaryColor = dto.PrimaryColor,
                Currency = dto.Currency,
                Locale = dto.Locale,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _db.Stores.Add(store);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = store.Id }, new
            {
                id = store.Id,
                name = store.Name,
                slug = store.Slug,
            });
        }

        /// <summary>Update a tenant. Super-admin only.</summary>
        [HttpPut("stores/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StoreUpsertDTO dto)
        {
            if (!IsSuperAdmin()) return Forbid();

            var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (store == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Slug) && dto.Slug != store.Slug)
            {
                var taken = await _db.Stores.AnyAsync(s => s.Slug == dto.Slug && s.Id != id);
                if (taken) return Conflict($"Slug '{dto.Slug}' is already in use.");
                store.Slug = dto.Slug.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(dto.Name)) store.Name = dto.Name.Trim();
            store.Domain = string.IsNullOrWhiteSpace(dto.Domain) ? null : dto.Domain.Trim();
            store.LogoUrl = dto.LogoUrl;
            store.PrimaryColor = dto.PrimaryColor;
            store.Currency = dto.Currency;
            store.Locale = dto.Locale;
            store.IsActive = dto.IsActive;
            store.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Soft-delete a tenant by deactivating it. Hard delete is intentionally
        /// not exposed: the FK constraints on every IStoreScoped entity use
        /// ON DELETE RESTRICT, so deleting a store with any data is rejected
        /// by MySQL anyway. Use this to hide a tenant from the switcher.
        /// </summary>
        [HttpDelete("stores/{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (!IsSuperAdmin()) return Forbid();

            var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (store == null) return NotFound();

            store.IsActive = false;
            store.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
