using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class UpdateUserDTO
{

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        //public string Password { get; set; }
        public string PhoneNumber { get; set; }

        public DateOnly Birthday { get; set; }
        public CustomerDTO Customer { get; set; }
    }

    public class CustomersDTO
    {
        public string Company { get; set; }
        public string Address { get; set; }
        public string Apartment { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        [Column("news_letter")]
        public bool Newsletter { get; set; }
        public List<AddressDTO> Addresses { get; set; }
        public List<AddressDTO> DefaultAddresses { get; set; }

    }

    public class AddressDTO
    {
        public string Address1 { get; set; }
        public string City { get; set; }
    }

}
