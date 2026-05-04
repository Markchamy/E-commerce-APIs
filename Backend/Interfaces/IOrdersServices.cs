using Backend.Models;

namespace Backend.Interfaces
{
    public interface IOrdersServices
    {
        Task<bool> OrderExist(long id);
        Task<ResponseBase> CreateOrder(OrdersModel orders);
        Task<OrdersModel> GetOrderById(long orderid);
        Task<List<OrdersModel>> GetAllOrders(
            int page,
            int pageSize,
            string sortBy = "date",
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        );
        Task<List<OrdersModel>> GetPMIOrders(
            int page,
            int pageSize,
            string sortBy = "date",
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        );
        Task<OrdersModel> GetLatestOrderAsync();
        Task<List<OrdersModel>> ExportOrders();
        Task<List<OrdersModel>> GetAllCancelledOrders();
        Task<int> GetOrdersCount();
        Task <OrdersModel> GetOrderByIdWithDetails(long orderid);
        Task UpdateOrder(OrdersModel order);
        Task DeleteOrder(OrdersModel order);
        Task SaveChangesAsync();
        Task<List<OrdersModel>> GetOrdersByCustomer(long userId, int limit);
        Task<long> GetNextOrderIdAsync();
        Task<string> GetNextOrderNameAsync();
    }
}
