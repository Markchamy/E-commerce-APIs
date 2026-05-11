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
        private readonly MyDbContext _db;

        public StoresController(MyDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// List all active stores. Not tenant-scoped (StoreModel is global).
        /// Used by the CMS store-switcher.
        /// </summary>
        [HttpGet("stores")]
        public async Task<IActionResult> GetAll()
        {
            var stores = await _db.Stores
                .Where(s => s.IsActive)
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
