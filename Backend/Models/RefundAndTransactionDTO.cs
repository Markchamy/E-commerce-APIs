namespace Backend.Models
{
    public class RefundAndTransactionDTO
{
        public RefundDTO Refund { get; set; }
        public TransactionsDTO Transactions { get; set; }
        public RefundLineItemsDTO RefundLines { get; set; }
        public LineItemsDTO? LineItem { get; set; }
    }
}
