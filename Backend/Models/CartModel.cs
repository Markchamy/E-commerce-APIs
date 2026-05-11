using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    public class CartItem : IStoreScoped
    {
        public long Id { get; set; } // Primary Key

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        [Column("product_id")]
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        [Column("user_id")]
        public long UserId { get; set; } // To associate the cart with a user
    }

}
