using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class CartItem
    {
        public long Id { get; set; } // Primary Key
        [Column("product_id")]
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        [Column("user_id")]
        public long UserId { get; set; } // To associate the cart with a user
    }

}
