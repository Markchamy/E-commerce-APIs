using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Backend.Models
{
    public class CustomerModel
    {
        public long Id { get; set; }
        public string? Company { get; set; }
        public string? Address { get; set; }
        public string? Apartment { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        [Column("email_sms_opt_in")]
        public bool? EmailSmsOptIn { get; set; }

        [Column("news_letter")]
        public bool? Newsletter { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("orders_count")]
        public int? OrdersCount { get; set; }
        public bool? state { get; set; }

        [Column("total_spent")]
        public double? TotalSpent { get; set; }

        [Column("last_order_id")]
        public long? LastOrderId { get; set; }
        public string? note { get; set; }
        public string? tags { get; set; }

        [Column("last_order_name")]
        public string? LastOrderName { get; set; }
        public string? Currency { get; set; }

        // Reference to the User
        public long? UserId { get; set; }
        public UserModel User { get; set; }

        public CustomerModel()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
        public List<AddressesModel> Addresses { get; set; }

    }

    public class AddressesModel
    {
        public long Id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? Company { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public string? Zip { get; set; }
        public string? Phone { get; set; }

        public string? Name { get; set; }

        [Column("province_code")]
        public string? ProvinceCode { get; set; }

        [Column("country_code")]
        public string? CountryCode { get; set; }

        [Column("country_name")]
        public string? CountryName { get; set; }

        [Column("default_address")]
        public bool? Default { get; set; }

        //Reference to the customer
        public virtual CustomerModel Customer { get; set; }

        [Column("customer_id")]
        public long CustomerId { get; internal set; }

    }

    public class CustomerSortByModel
    {
        public int Id { get; set; }
        public string name { get; set; }
    }

}
