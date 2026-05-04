using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PermissionServicesRepository : IPermissionServices
    {
        private readonly MyDbContext _context;

        public PermissionServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PermissionsModel>> GetAllPermissionsAsync()
        {
            return await _context.permissions.ToListAsync();
        }
    }
}
