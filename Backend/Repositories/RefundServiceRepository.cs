using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class RefundServiceRepository : IRefundServices
    {
        private readonly MyDbContext _context;
        public RefundServiceRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<OrdersModel> GetOrderById(long orderid)
        {
            return await _context.Orders.FirstOrDefaultAsync(order => order.orderid == orderid);
        }
        public async Task CreateRefund(RefundModel refund)
        {
            _context.Refund.Add(refund);
        }

        public async Task CreateRefundLine(RefundLineItemsModel refundLine)
        {
            _context.refund_line_items.Add(refundLine);
        }

        public async Task CreateTransaction(TransactionsModel transaction)
        {
            _context.transactions.Add(transaction);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task CreateLineItem(LineItemsModel lineItem)
        {
            _context.LineItems.Add(lineItem);
        }
        public async Task<List<RefundModel>> GetAllRefunds()
        {
            return await _context.Refund
                .Include(refund => refund.Transaction)
                .Include(refund => refund.RefundLine)
                .ThenInclude(refundLine => refundLine.LineItem)
                .ToListAsync();
        }
        public async Task<RefundModel> GetRefundById(int id)
        {
            return await _context.Refund
                .Include(refund => refund.Transaction)
                .Include(refund => refund.RefundLine)
                .ThenInclude(refundLine => refundLine.LineItem)
                .FirstOrDefaultAsync(refund => refund.id == id);
        }



    }
}
