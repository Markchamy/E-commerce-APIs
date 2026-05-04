namespace Backend.Models
{
    public class PurchaseOrderDTO
    {
    }

    public class SupplierDTO
    {
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
}
