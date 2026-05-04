using Backend.Models;

namespace Backend.Interfaces
{
    public interface IRefundServices
    {
        Task<OrdersModel> GetOrderById(long orderid);
        Task CreateRefund(RefundModel refund);
        Task CreateRefundLine(RefundLineItemsModel refundLine);
        Task CreateTransaction(TransactionsModel transaction);
        Task CreateLineItem(LineItemsModel lineItem);
        Task<RefundModel> GetRefundById(int id);
        Task<List<RefundModel>> GetAllRefunds();
        Task SaveChangesAsync();
    }
}
