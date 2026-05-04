using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Backend.Repositories
{
    public class AddressServicesRepository : IAddressServices
    {
        private readonly MyDbContext _context;

        public AddressServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseBase> AddAddressForUserByEmail(long customerId, NewAddressDTO addressDTO)
        {
            // Check if the customer exists
            var customer = await _context.Users.FirstOrDefaultAsync(user => user.Id == customerId);
            if (customer == null)
            {
                return ResponseBase.Failure("Customer not found.");
            }

            // Create a new address for the existing customer
            var newAddress = new AddressesModel
            {
                CustomerId = customerId,
                first_name = addressDTO.first_name,
                last_name = addressDTO.last_name,
                Company = addressDTO.Company,
                Address1 = addressDTO.Address1,
                Address2 = addressDTO.Address2,
                City = addressDTO.City,
                Province = addressDTO.Province,
                Country = addressDTO.Country,
                Zip = addressDTO.Zip,
                Phone = addressDTO.Phone,
                Name = addressDTO.name,
                ProvinceCode = addressDTO.ProvinceCode,
                CountryCode = addressDTO.CountryCode,
                CountryName = addressDTO.CountryName,
                Default = addressDTO.Default,
            };

            // Add the new address to the database
            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return ResponseBase.Success("New address added successfully for the customer.", newAddress);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<ResponseBase> DeleteAddress(long addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                return ResponseBase.Failure("Address not found.");
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return ResponseBase.Success("Address deleted successfully.");
        }

        public async Task<List<NewAddressDTO>> GetAddressesByCustomerId(long customerId)
        {
            return await _context.Addresses
                .Where(address => address.CustomerId == customerId)
                .Select(address => new NewAddressDTO
                {
                    Id = address.Id,
                    CustomerId = address.CustomerId,
                    first_name = address.first_name,
                    last_name = address.last_name,
                    Company = address.Company,
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    City = address.City,
                    Province = address.Province,
                    Country = address.Country,
                    Zip = address.Zip,
                    Phone = address.Phone,
                    name = address.Name,
                    ProvinceCode = address.ProvinceCode,
                    CountryCode = address.CountryCode,
                    CountryName = address.CountryName,
                    Default = address.Default ?? false
                })
                .ToListAsync();
        }


        public async Task<ResponseBase> UpdateAddressesByAddressId(long addressId, NewAddressDTO addressDto)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                return ResponseBase.Failure("Address not found.");
            }

            // Update the address with the new data from addressDto
            address.first_name = addressDto.first_name;
            address.last_name = addressDto.last_name;
            address.Company = addressDto.Company;
            address.Address1 = addressDto.Address1;
            address.Address2 = addressDto.Address2;
            address.City = addressDto.City;
            address.Province = addressDto.Province;
            address.Country = addressDto.Country;
            address.Zip = addressDto.Zip;
            address.Phone = addressDto.Phone;
            address.Name = addressDto.name;
            address.ProvinceCode = addressDto.ProvinceCode;
            address.CountryCode = addressDto.CountryCode;
            address.CountryName = addressDto.CountryName;
            address.Default = addressDto.Default;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return ResponseBase.Success("Address updated successfully.", address);
        }

        public async Task<ResponseBase> UpdateDefaultAddressById(long addressId, NewAddressDTO addressDto)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                return ResponseBase.Failure("Address not found.");
            }

            // If the address is being set as the default
            if (addressDto.Default)
            {
                // Set all other addresses for the same customer to not be the default
                var customerAddresses = _context.Addresses.Where(address => address.CustomerId == address.CustomerId && address.Id != addressId);
                foreach (var otherAddress in customerAddresses)
                {
                    otherAddress.Default = false;
                }
            }

            // Update the target address
            address.Default = addressDto.Default;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return ResponseBase.Success("Default Address State updated successfully.", address);
        }

    }
}
