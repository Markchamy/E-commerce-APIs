using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Interfaces;

namespace Backend.Models
{
    public class SupplierModel : IStoreScoped
    {
        [Key]
        public int id { get; set; }

        [Required]
        [Column("store_id")]
        public int StoreId { get; set; } = 1;

        public string company { get; set; }
        public string country { get; set; }
        public string address { get; set; }
        public string apartment { get; set; }
        public string city { get; set; }
        public string postal_code { get; set; }
        public string contact_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class PurchaseOrderModel
    {
        [Key]
        public int id { get; set; }
        public string supplier { get; set; }
        public int supplier_id { get; set; }
        public string destination { get; set; }
        public string payment_terms { get; set; }
        public string supplier_currency { get; set; }
        public int product_id { get; set; }
        public string product_name { get; set; }
    }

}
