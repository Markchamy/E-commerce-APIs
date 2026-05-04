using Backend.Models;

namespace Backend.Interfaces
{
    public interface IMapServices
{

        Task<IEnumerable<DistrictModel>> GetAllDistricts();
        Task<IEnumerable<CityModel>> GetAllCitiesRelated(int districtId);
        Task<IEnumerable<DistrictModel>> GetDeliveryPricesAsync(int[] districtIds);
        Task<IEnumerable<DistrictModel>> GetAllCitiesWithDistricts();
    }
}
