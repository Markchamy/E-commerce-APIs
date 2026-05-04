using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
        public class DistrictModel
        {
            public int id { get; set; }
            public string districts { get; set; }
            public string delivery_price { get; set; }
            public List<CityModel> city { get; set; }
        }

    [Table("cities")]
    public class CityModel
    {
            public int id { get; set; }
            public int district_id { get; set; }
            public string cities { get; set; }
    }

}

