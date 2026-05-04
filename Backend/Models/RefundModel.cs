using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class RefundModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }                 // int -> long

        public long orderid { get; set; }            // matches OrdersModel.orderid
        public DateTime created_at { get; set; }
        public string note { get; set; }
        public long? user_id { get; set; }           // Shopify can send null
        public DateTime processed_at { get; set; }
        public bool restock { get; set; }
        public string? refunds_return { get; set; }  // add setter

        // optional: navigation to Order
        // [ForeignKey(nameof(orderid))]
        // public OrdersModel Order { get; set; }

        public RefundModel()
        {
            created_at = DateTime.Now;
            processed_at = DateTime.Now;
        }

        public List<TransactionsModel> Transaction { get; set; }
        public List<RefundLineItemsModel> RefundLine { get; set; }
    }

    public class TransactionsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }                 // int -> long

        public long orderid { get; set; }

        public long? refund_id { get; set; }         // int -> long?
        public string kind { get; set; }
        public string gateway { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public DateTime created_at { get; set; }
        public bool test { get; set; }
        public long? user_id { get; set; }
        public long? parent_id { get; set; }         // int -> long?
        public DateTime processed_at { get; set; }
        public string source_name { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string payment_id { get; set; }
        public bool manual_payment_gateway { get; set; }

        // optional: navigation to Refund
        // [ForeignKey(nameof(refund_id))]
        // public RefundModel Refund { get; set; }

        public TransactionsModel()
        {
            created_at = DateTime.Now;
            processed_at = DateTime.Now;
        }
    }

    public class RefundLineItemsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }                 // int -> long

        public long orderid { get; set; }

        public long refund_id { get; set; }          // int -> long
        public long? location_id { get; set; }       // int -> long?
        public string restock_type { get; set; }
        public int quantity { get; set; }

        public long line_item_id { get; set; }       // int -> long (FK to LineItemsModel.lineItemId)

        public double subtotal { get; set; }
        public double total_tax { get; set; }

        [ForeignKey(nameof(refund_id))]
        public RefundModel Refund { get; set; }

        [ForeignKey(nameof(line_item_id))]
        public LineItemsModel LineItem { get; set; }
    }
}
