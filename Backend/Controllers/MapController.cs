using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class MapController : ControllerBase
    {
        private readonly IMapServices _maprepository;
        public MapController(IMapServices maprepository)
        {
            _maprepository = maprepository;
        }

        [HttpGet("district")]
        public async Task<IActionResult> GetAllDistricts()
        {
            try
            {
                var districts = await _maprepository.GetAllDistricts();
                if (districts == null || !districts.Any())
                {
                    return NotFound("No districts found");
                }
                return Ok(new ResponseBase(true, "District retrieved successfully.", districts));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner Message";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while retrieving the districts: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("district/{district_id}/cities")]
        public async Task<IActionResult> GetAllCitiesRelated(int district_id)
        {
            try
            {
                var cities = await _maprepository.GetAllCitiesRelated(district_id);
                if (cities == null)
                {
                    return NotFound("No Cities related to the district has been found.");
                }
                return Ok(new ResponseBase(true, "Cities related to the district retrieved successfully.", cities));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occured while retrieving the cities: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("district/pricing")]
        public async Task<IActionResult> GetDeliveryPrices([FromQuery] int[] districtIds)
        {
            try
            {
                if (districtIds == null || !districtIds.Any())
                {
                    return BadRequest("District IDs cannot be null or empty.");
                }

                var pricing = await _maprepository.GetDeliveryPricesAsync(districtIds);

                if (pricing == null || !pricing.Any())
                {
                    return NotFound("No pricing information found for the specified districts.");
                }

                return Ok(new ResponseBase(true, "Pricing information retrieved successfully.", pricing));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving the pricing: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }

        [HttpGet("cities-with-districts")]
        public async Task<IActionResult> GetAllCitiesWithDistricts()
        {
            try
            {
                var districtsWithCities = await _maprepository.GetAllCitiesWithDistricts();

                if (districtsWithCities == null || !districtsWithCities.Any())
                {
                    return NotFound("No cities or districts found.");
                }

                return Ok(new ResponseBase(true, "Cities with districts retrieved successfully.", districtsWithCities));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, ResponseBase.Failure($"An internal error occurred while retrieving cities with districts: {ex.Message}; Inner exception: {innerMessage}"));
            }
        }


    }
}
