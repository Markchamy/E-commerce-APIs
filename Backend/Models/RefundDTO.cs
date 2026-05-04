namespace Backend.Models
{
    public class RefundDTO
    {
        public long orderid { get; set; }
        public string note { get; set; }
        public long? user_id { get; set; }              // was long -> make nullable to match model
        public bool restock { get; set; }
        public string? refunds_return { get; set; }
    }

    public class TransactionsDTO
    {
        public long orderid { get; set; }
        public long? refund_id { get; set; }            // int -> long?
        public string kind { get; set; }
        public string gateway { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public bool test { get; set; }
        public long? user_id { get; set; }
        public long? parent_id { get; set; }            // int -> long?
        public string source_name { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string payment_id { get; set; }
        public bool manual_payment_gateway { get; set; }
    }

    public class RefundLineItemsDTO
    {
        public long orderid { get; set; }
        public long refund_id { get; set; }             // int -> long
        public long? location_id { get; set; }          // int -> long?
        public string restock_type { get; set; }
        public int quantity { get; set; }
        public long line_item_id { get; set; }          // int -> long (FK to LineItemsModel.lineItemId)
        public double subtotal { get; set; }
        public double total_tax { get; set; }

        // optional navigation payload; keep if you actually post nested line item details
        public LineItemsDTO? LineItem { get; set; }
    }
}
