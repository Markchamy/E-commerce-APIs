using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class MapServicesRepository : IMapServices
    {
        private readonly MyDbContext _context;

        public MapServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DistrictModel>> GetAllDistricts()
        {
            return await _context.district.ToListAsync();
        }
        public async Task<IEnumerable<CityModel>> GetAllCitiesRelated(int districtId)
        {
            return await _context.city
                                 .Where(city => city.district_id == districtId)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<DistrictModel>> GetDeliveryPricesAsync(int[] districtIds)
        {
            return await _context.district
                .Where(d => districtIds.Contains(d.id))
                .Select(d => new DistrictModel
                {
                    id = d.id,
                    districts = d.districts,
                    delivery_price = d.delivery_price
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DistrictModel>> GetAllCitiesWithDistricts()
        {
            return await _context.district
                .Include(d => d.city)
                .ToListAsync();
        }
    }
}
