using System.Security.Claims;
using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    /// <summary>
    /// Per-store employee management. All reads/writes are auto-scoped to the
    /// current tenant by the DbContext query filter. Mutations require the
    /// caller to hold admin/manager access on that store.
    /// </summary>
    [ApiController]
    [Route("admin/api/2024-01")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private static readonly string[] StoreAdminAccess = { "admin", "manager" };

        private readonly MyDbContext _db;
        private readonly ITenantContext _tenant;

        public EmployeesController(MyDbContext db, ITenantContext tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        public class EmployeeDTO
        {
            public int Id { get; set; }
            public long UserId { get; set; }
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Role { get; set; }
            public List<string> AccessControl { get; set; } = new();
            public int StoreId { get; set; }
        }

        public class EmployeeUpsertDTO
        {
            public string Email { get; set; } = string.Empty;
            public string? Password { get; set; }   // required on create, optional on update
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? PhoneNumber { get; set; }
            public string Role { get; set; } = "employee"; // top-level role: admin | manager | employee
            public List<string> AccessControl { get; set; } = new();
        }

        private bool CallerHasStoreAdminAccess()
        {
            // Per-store admin/manager keep their existing power. super_admin is
            // also allowed because they need to add admins to other stores when
            // editing a tenant from the platform-level Stores page (they send
            // X-Store-Id explicitly, which the middleware accepts only for them).
            var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();
            if (role == null) return false;
            return StoreAdminAccess.Contains(role) || role == "super_admin";
        }

        private static long GenerateUserId()
        {
            var ts = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
            var rand = Random.Shared.Next(0, 4096);
            var id = (ts << 12) | (long)rand;
            return id < 1_000_000 ? 1_000_000 + rand : id;
        }

        /// <summary>List employees of the current tenant.</summary>
        [HttpGet("employees")]
        public async Task<IActionResult> List()
        {
            if (!_tenant.IsResolved)
                return BadRequest("No active store. Pick a store before viewing employees.");

            var rows = await _db.Employees
                .Include(e => e.User)
                .Select(e => new EmployeeDTO
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    Email = e.User.email,
                    FirstName = e.User.first_name,
                    LastName = e.User.last_name,
                    PhoneNumber = e.User.phone_number,
                    Role = e.User.role,
                    AccessControl = e.AccessControl ?? new List<string>(),
                    StoreId = e.StoreId,
                })
                .ToListAsync();

            return Ok(rows);
        }

        /// <summary>Create a new employee (and the underlying user) in the current tenant.</summary>
        [HttpPost("employees")]
        public async Task<IActionResult> Create([FromBody] EmployeeUpsertDTO dto)
        {
            if (!_tenant.IsResolved) return BadRequest("No active store.");
            if (!CallerHasStoreAdminAccess()) return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required.");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
                return BadRequest("Password must be at least 8 characters.");

            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.email == email))
                return Conflict($"Email '{email}' is already in use.");

            var role = (dto.Role ?? "employee").Trim().ToLowerInvariant();
            if (role is not ("admin" or "manager" or "employee"))
                return BadRequest("Role must be admin, manager, or employee.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var user = new UserModel
                {
                    Id = GenerateUserId(),
                    role = role,
                    email = email,
                    password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    first_name = dto.FirstName?.Trim(),
                    last_name = dto.LastName?.Trim(),
                    phone_number = dto.PhoneNumber?.Trim(),
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                var employee = new EmployeeModel
                {
                    UserId = user.Id,
                    StoreId = _tenant.StoreId!.Value,
                    AccessControl = dto.AccessControl ?? new List<string> { role },
                };
                _db.Employees.Add(employee);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
                return CreatedAtAction(nameof(List), new { id = employee.Id }, new
                {
                    id = employee.Id,
                    userId = user.Id,
                });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>Update an employee's profile, role, and access control.</summary>
        [HttpPut("employees/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpsertDTO dto)
        {
            if (!_tenant.IsResolved) return BadRequest("No active store.");
            if (!CallerHasStoreAdminAccess()) return Forbid();

            var emp = await _db.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id);
            if (emp == null) return NotFound();

            // Only the user themselves can keep their email unchanged; collision check otherwise.
            var newEmail = dto.Email.Trim().ToLowerInvariant();
            if (!string.Equals(emp.User.email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var taken = await _db.Users.IgnoreQueryFilters()
                    .AnyAsync(u => u.email == newEmail && u.Id != emp.UserId);
                if (taken) return Conflict($"Email '{newEmail}' is already in use.");
                emp.User.email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 8) return BadRequest("Password must be at least 8 characters.");
                emp.User.password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (!string.IsNullOrWhiteSpace(dto.FirstName)) emp.User.first_name = dto.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(dto.LastName)) emp.User.last_name = dto.LastName.Trim();
            if (dto.PhoneNumber != null) emp.User.phone_number = dto.PhoneNumber.Trim();

            var role = (dto.Role ?? emp.User.role ?? "employee").Trim().ToLowerInvariant();
            if (role is not ("admin" or "manager" or "employee"))
                return BadRequest("Role must be admin, manager, or employee.");
            emp.User.role = role;

            emp.AccessControl = dto.AccessControl ?? new List<string> { role };

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Remove an employee from this store. The underlying user account is
        /// preserved (they may belong to another store later, or have customer
        /// activity); only the employees row is deleted.
        /// </summary>
        [HttpDelete("employees/{id:int}")]
        public async Task<IActionResult> Remove(int id)
        {
            if (!_tenant.IsResolved) return BadRequest("No active store.");
            if (!CallerHasStoreAdminAccess()) return Forbid();

            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (emp == null) return NotFound();

            _db.Employees.Remove(emp);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
