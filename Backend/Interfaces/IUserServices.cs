using Backend.Models;

namespace Backend.Interfaces
{
    public interface IUserServices
    {
        Task<bool> UserExists(string email);
        Task<ResponseBase> AddUser(UserModel user);
        Task<ResponseBase> AddCustomer(CustomerModel customer);
        Task<ResponseBase> AddAddresses(AddressesModel address);
        Task<ResponseBase> AddEmployee(EmployeeModel employee);
        Task SaveChangesAsync();
        Task<long> GetNextUserIdAsync();
        Task<long> GetNextCustomerIdAsync();
        Task<long> GetNextAddressIdAsync();
        Task<UserModel?> GetUserByIdAsync(long userId);
        Task<CustomerModel?> GetCustomerByIdIncludingRelatedEntities(long customerId);
        Task<UserModel?> GetUserByEmailIncludingRelatedEntities(string email);
        Task<UserModel?> GetUserByPhoneNumberIncludingRelatedEntities(string phoneNumber);

        Task<ResponseBase> DeleteUser(UserModel user);
        Task DeleteCustomer(CustomerModel customer);
        Task DeleteAddresses(AddressesModel address);
        Task DeleteEmployee(EmployeeModel employee);
        Task<IEnumerable<UserModel>> GetAllUsersIncludingRelatedEntitie();
        Task<ResponseBase> GetUserByEmail(string email);
        Task<IEnumerable<UserModel>> GetAllUsersIncludingRelatedEntities(
            int page,
            int pageSize,
            string sortBy,
            string sortDirection,
            string search
        );


        Task<ResponseBase> GetUsersByIdIncludingRelatedEntities(string email);
        Task<int> GetCustomerCount();
        Task<IEnumerable<UserModel>> GetAllCustomer();
        Task<ResponseBase> UpdateUser(UserModel user);
        Task<ResponseBase> GetUserByResetToken(string token);
        Task SendPasswordResetEmail(string email, string resetLink);
        Task SendWelcomeEmail(string email, string firstName);

        Task<IEnumerable<CustomerSortByModel>> GetAllSortingAsync();
        Task<List<EmployeeModel>> GetAllEmployees();
        Task<EmployeeModel?> GetEmployeeByUserId(long userId);
        Task<(bool IsSuccess, string Message)> SaveChangeAsync();
    }
}
