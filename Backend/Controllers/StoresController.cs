using System.Security.Claims;
using Backend.Data;
using Backend.Middleware;
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
        private static readonly string[] StoreAdminRoles = { "admin", "manager" };

        private readonly MyDbContext _db;

        public StoresController(MyDbContext db)
        {
            _db = db;
        }

        private string? CurrentRole => User.FindFirst(ClaimTypes.Role)?.Value;

        private bool IsSuperAdmin() =>
            string.Equals(CurrentRole, TenantMiddleware.SuperAdminRole, StringComparison.OrdinalIgnoreCase);

        private bool IsStoreAdmin() =>
            CurrentRole != null && StoreAdminRoles.Any(r =>
                string.Equals(r, CurrentRole, StringComparison.OrdinalIgnoreCase));

        private int? CurrentJwtStoreId()
        {
            var claim = User.FindFirst("store_id")?.Value;
            return int.TryParse(claim, out var id) && id > 0 ? id : null;
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
            var query = _db.Stores.Where(s => s.IsActive);

            if (!IsSuperAdmin())
            {
                // Per-store roles (admin/manager/employee): see only your store.
                var storeId = CurrentJwtStoreId();
                if (storeId == null) return Ok(Array.Empty<object>());
                query = query.Where(s => s.Id == storeId.Value);
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

            /// <summary>
            /// Optional bootstrap admin. When present on POST /stores, the
            /// store + user + employee are created in a single transaction so
            /// the new tenant has a usable login from day one. Ignored on PUT.
            /// </summary>
            public BootstrapAdminDTO? FirstAdmin { get; set; }
        }

        public class BootstrapAdminDTO
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
        }

        /// <summary>
        /// users.Id has no AUTO_INCREMENT in this schema, so the API has to
        /// supply one. Mirror the timestamp+random pattern used in the CMS
        /// signup page to stay collision-resistant.
        /// </summary>
        private static long GenerateUserId()
        {
            var timestamp = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
            var rand = Random.Shared.Next(0, 4096);
            var id = (timestamp << 12) | (long)rand;
            return id < 1_000_000 ? 1_000_000 + rand : id;
        }

        /// <summary>
        /// Create a new tenant. Super-admin only. If FirstAdmin is supplied,
        /// the new store, its first admin user, and the corresponding employees
        /// row are written in one transaction — so a newly created tenant is
        /// guaranteed to have at least one person who can log in.
        /// </summary>
        [HttpPost("stores")]
        public async Task<IActionResult> Create([FromBody] StoreUpsertDTO dto)
        {
            if (!IsSuperAdmin()) return Forbid();
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
            if (string.IsNullOrWhiteSpace(dto.Slug)) return BadRequest("Slug is required.");

            if (await _db.Stores.AnyAsync(s => s.Slug == dto.Slug))
                return Conflict($"Slug '{dto.Slug}' is already in use.");

            // A store without an admin is unreachable — no one can log in.
            // Require the FirstAdmin payload up front.
            if (dto.FirstAdmin == null)
                return BadRequest("FirstAdmin is required so the new store has at least one usable login.");
            if (string.IsNullOrWhiteSpace(dto.FirstAdmin.Email))
                return BadRequest("First admin email is required.");
            if (string.IsNullOrWhiteSpace(dto.FirstAdmin.Password) || dto.FirstAdmin.Password.Length < 8)
                return BadRequest("First admin password must be at least 8 characters.");
            {
                var email = dto.FirstAdmin.Email.Trim().ToLowerInvariant();
                if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.email == email))
                    return Conflict($"Email '{email}' is already in use.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
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

                var newUser = new UserModel
                {
                    Id = GenerateUserId(),
                    role = "admin",
                    email = dto.FirstAdmin.Email.Trim().ToLowerInvariant(),
                    password = BCrypt.Net.BCrypt.HashPassword(dto.FirstAdmin.Password),
                    first_name = dto.FirstAdmin.FirstName?.Trim(),
                    last_name = dto.FirstAdmin.LastName?.Trim(),
                    phone_number = dto.FirstAdmin.PhoneNumber?.Trim(),
                };
                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                _db.Employees.Add(new EmployeeModel
                {
                    StoreId = store.Id,
                    UserId = newUser.Id,
                    AccessControl = new List<string> { "admin" },
                });
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                return CreatedAtAction(nameof(GetAll), new { id = store.Id }, new
                {
                    id = store.Id,
                    name = store.Name,
                    slug = store.Slug,
                    firstAdminUserId = newUser.Id,
                });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Update a tenant. Super-admin can update any store; a store-level admin
        /// or manager can update only their own store.
        /// </summary>
        [HttpPut("stores/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StoreUpsertDTO dto)
        {
            if (!IsSuperAdmin())
            {
                if (!IsStoreAdmin()) return Forbid();
                if (CurrentJwtStoreId() != id) return Forbid();
            }

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
