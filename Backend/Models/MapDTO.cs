namespace Backend.Models
{

    public class DistrictDTO
    {
        public string district { get; set; }
        public string delivery_price { get; set; }

    }

    public class CityDTO
    {
        public int district_id { get; set; }
        public string cities { get; set; }
    }
}
