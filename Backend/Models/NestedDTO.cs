using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Backend.Models
{
    public class CustomerDTO
    {
        public long? Id {get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string Apartment { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        [Column("email_sms_opt_in")]
        public bool EmailSmsOptIn { get; set; }

        [Column("news_letter")]
        public bool Newsletter { get; set; }

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
        public List<AddressesDTO> Addresses { get; set; }

    }

    public class AddressesDTO
    {
        public long Id { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Province { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string? Zip { get; set; }

        [Column("province_code")]
        public string? ProvinceCode { get; set; }

        [Column("country_code")]
        public string? CountryCode { get; set; }

        [Column("country_name")]
        public string? CountryName { get; set; }

        [Column("default_address")]
        public bool Default { get; set; }

    }

    public class EmployeeDTO
    {
        public List<string> AccessControl { get; set; } // Change from string to List<string>
    }

    public class UpdateEmployeeAccessControlDTO
    {
        public long UserId { get; set; }
        public List<string> AccessControl { get; set; }
    }


}
