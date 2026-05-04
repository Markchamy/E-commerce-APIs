using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressServices _addressServices;

        public AddressesController(IAddressServices addressServices)
        {
            _addressServices = addressServices;
        }

        //API to create Addresses
        [HttpPost("add-addresses")]
        public async Task<IActionResult> AddAddress([FromBody] NewAddressDTO addressDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _addressServices.AddAddressForUserByEmail(addressDTO.CustomerId, addressDTO);

                if (!response.IsSuccess)
                {
                    return BadRequest(response.Message);
                }

                return Ok(ResponseBase.Success("address has been added successfully." ,response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while adding the new address."));
            }
        }

        [HttpDelete("delete-addresses")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            try
            {
                var response = await _addressServices.DeleteAddress(addressId);

                if (!response.IsSuccess)
                {
                    return BadRequest(response.Message);
                }

                return Ok(ResponseBase.Success($"address with id {addressId} has been deleted.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while deleting the address."));
            }
        }

        [HttpGet("get-all-addresses")]
        public async Task<IActionResult> GetAddresses(int customerId)
        {
            // Retrieve addresses from the repository
            var addresses = await _addressServices.GetAddressesByCustomerId(customerId);

            // If no addresses are found, return a NotFound response
            if (addresses == null)
            {
                return NotFound(new { message = "No addresses found for the given customer ID." });
            }

            // Return the list of addresses in JSON format
            return Ok(addresses);
        }

        [HttpPut("update-addresses")]
        public async Task<IActionResult> UpdateAddresses(int addressId, [FromBody] NewAddressDTO addressDto)
        {
            try
            {
                var response = await _addressServices.UpdateAddressesByAddressId(addressId, addressDto);

                if (!response.IsSuccess)
                {
                    return BadRequest(response.Message);
                }
                return Ok(response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while updating the new address."));
            }
        }

        [HttpPut("update-default-address")]
        public async Task<IActionResult> UpdateDefaultAddress(int addressId, [FromBody] NewAddressDTO addressDTO)
        {
            try
            {
                var response = await _addressServices.UpdateDefaultAddressById(addressId, addressDTO);

                if (!response.IsSuccess)
                {
                    return BadRequest(response.Message);
                }
                return Ok(response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An error occured while setting the new default address."));
            }
        }

    }
}
