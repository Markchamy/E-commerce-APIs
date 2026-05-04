using Backend.Models;

namespace Backend.Interfaces
{
    public interface IPmiServices
    {
        // Customer operations
        Task<PmiCustomer?> GetCustomerByIdAsync(long customerId);
        Task<PmiCustomer?> GetCustomerByPhoneAsync(string phone);
        Task<ResponseBase> AddCustomerAsync(PmiCustomer customer);

        // Order operations
        Task<PmiOrderListResponse> GetOrdersAsync(PmiOrderFilterParams filters);
        Task<PmiOrder?> GetOrderByReferenceAsync(string orderReference);
        Task<List<PmiOrderedProductResponseDto>> GetOrderProductsAsync(string orderReference);
        Task<ResponseBase> AddOrderAsync(PmiOrder order);
        Task<ResponseBase> AddOrderProductsAsync(string orderReference, List<PmiOrderedProduct> products, List<PmiOrderedMachine>? machines);
        Task<ResponseBase> UpdateOrderAsync(string orderReference, PmiOrderUpdateRequest request);
        Task<ResponseBase> DeleteOrderAsync(string orderReference);
        Task<ResponseBase> UpdateOrderErrorAsync(string orderReference, string? error);
        Task<ResponseBase> ClearOrderErrorAsync(string orderReference);

        // Product operations
        Task<PmiProduct?> GetProductByNameAsync(string productName);
        Task<decimal?> GetProductPriceAsync(string productName);

        // Machine/Serial operations
        Task<List<string>> GetOrderSerialNumbersAsync(string orderReference);

        // Error operations
        Task<PmiError> CreateErrorAsync(string error);
        Task<PmiErrorListResponse> GetErrorsAsync(PmiErrorFilterParams filters);
    }
}
