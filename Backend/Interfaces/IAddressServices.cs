using Backend.Models;

namespace Backend.Interfaces
{
    public interface IAddressServices
    {
        Task<ResponseBase> AddAddressForUserByEmail(long customerId, NewAddressDTO newAddressDTO);

        Task SaveChangesAsync();

        Task<ResponseBase> DeleteAddress(long addressId);
        Task<List<NewAddressDTO>> GetAddressesByCustomerId(long customerId);
        Task<ResponseBase> UpdateAddressesByAddressId(long addressId, NewAddressDTO newAddressDTO);
        Task<ResponseBase> UpdateDefaultAddressById(long addressId, NewAddressDTO newAddressDTO);
    }
}
