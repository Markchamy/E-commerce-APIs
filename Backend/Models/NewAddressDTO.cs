using Backend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class NewAddressDTO
{
        public long Id { get; set; }

        [Column("customer_id")]
        public long CustomerId { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? Company {  get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public string? Zip {  get; set; }
        public string? Phone { get; set; }
        public string? name { get; set; }

        [Column("province_code")]
        public string? ProvinceCode { get; set; }

        [Column("country_code")]
        public string? CountryCode { get; set; }

        [Column("country_name")]
        public string? CountryName { get; set; }

        [Column("default_address")]
        public bool Default { get; set; }
}
}
