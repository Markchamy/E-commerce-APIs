namespace Backend.Models;

    public class Carrier
{
    public string uuid { get; set; }
    public string name { get; set; }
    public string avatar { get; set; }
    public List<ShippingMethod> ShippingMethods { get; set; }
}

public class ShippingMethod
{
    public string uuid { get; set; }
    public string carrier_uuid { get; set; }
    public string name { get; set; }
    public List<BaseFee> BaseFee { get; set; }
    public string currency { get; set; }
    public decimal weight_threshold { get; set; }
    public decimal weight_surcharge { get; set; }
    public bool weight_surcharge_flat { get; set; }
    public string weight_unit { get; set; }
    public decimal distance_threshold { get; set; }
    public decimal distance_surcharge { get; set; }
    public bool distance_surcharge_flat { get; set; }
    public string distance_unit { get; set; }
    public decimal volume_threshold { get; set; }
    public decimal volume_surcharge { get; set; }
    public bool volume_surcharge_flat { get; set; }
    public int volume_divisor { get; set; }
    public string volume_unit { get; set; }
}

public class BaseFee
{
    public string shipping_method_uuid { get; set; }
    public decimal fee_from { get; set; }
    public decimal? fee_to { get; set; }
    public decimal amount { get; set; }
    public string amount_formatted { get; set; }
}
public class CarrierApiResponse
{
    public string Message { get; set; }
    public CarrierData Data { get; set; }
}

public class CarrierData
{
    public List<Carrier> Carriers { get; set; }
}

public class FleetCustomer
{
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public int type { get; set; }
    public string? external_id { get; set; }
    public string? note { get; set; }
    public List<FleetMetafields>? metafields { get; set; }

}
public class FleetMetafields
{
    public string? key { get; set; }
    public string? value { get; set; }
}

public class FleetLocation
{
    public string line_1 { get; set; }
    public string country { get; set; }
    public List<LocationCoordinates> coordinates { get; set; }
    public string name { get; set; }
    public string external_id { get; set; }
    public string line_2 { get; set; }
    public string city { get; set; }
    public string region { get; set; }
    public string directions { get; set; }
    public string zip_code { get; set; }
    public int abilities { get; set; }
    public int allowed_vehicle_types { get; set; }
}

public class LocationCoordinates
{
    public decimal lat { get; set; }
    public decimal lng { get; set; }
}

public class CreateOrderRequest
{
    public string? external_id { get; set; }
    public Customer customer { get; set; }
    public SourceLocation source_location { get; set; }
    public Contact source_contact { get; set; }
    public Location destination_location { get; set; }
    public Contact destination_contact { get; set; }
    public List<Collection>? collections { get; set; }
    public List<Package>? packages { get; set; }
    public List<string>? requirements { get; set; }
    public ProofOfDelivery? proof_of_delivery { get; set; }
    public Schedule? pickup_schedule { get; set; }
    public Schedule? delivery_schedule { get; set; }
    public bool is_return { get; set; }
    public string? notes { get; set; }
    public List<string>? tags { get; set; }
}

public class FleetOrderUpdate
{
    public string external_id { get; set; }
    public Contact source_contact { get; set; }
    public Location destination_location { get; set; }
    public List<Collection> collections { get; set; }
    public ProofOfDelivery proof_of_delivery { get; set; }
    public Schedule pickup_schedule { get; set; }
    public Schedule delivery_schedule { get; set; }
    public List<string> tags { get; set; }
    public string notes { get; set; }
}
public class Customer
{
    public string? external_id { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string? notes { get; set; }
}

public class SourceLocation
{
    public string uuid { get; set; }
}

public class Contact
{
    public string name { get; set; }
    public string phone { get; set; }
    public string email { get; set; }
}

public class Location
{
    public string name { get; set; }
    public string? external_id { get; set; }
    public string line_1 { get; set; }
    public string? line_2 { get; set; }
    public string city { get; set; }
    public string? region { get; set; }
    public string country { get; set; }
    public string? zip_code { get; set; }
    public string? directions { get; set; }
    public Coordinates? coordinates { get; set; }
}

public class Coordinates
{
    public double lat { get; set; }
    public double lng { get; set; }
}

public class Collection
{
    public int amount { get; set; }
    public string currency { get; set; }
    public string type { get; set; }
}

public class Package
{
    public string? external_id { get; set; }
    public string? barcode { get; set; }
    public string? type { get; set; }
    public double length { get; set; }
    public double width { get; set; }
    public double height { get; set; }
    public double weight { get; set; }
    public int quantity { get; set; }
}

public class ProofOfDelivery
{
    public bool signature { get; set; }
    public bool image { get; set; }
}

public class Schedule
{
    public string from { get; set; }
    public string to { get; set; }
}

public class AssignCarrierRequest
{
    public string CarrierUuid { get; set; }
    public string ShippingMethodUuid { get; set; }
}

public class GenerateLabelsRequest
{
    public List<Guid> Orders { get; set; }
    public string Size { get; set; }
}

public class WebhookSubscribe
{
    public string topic { get; set; }
    public string integration { get; set; }
    public string url { get; set; }
}

public class WebhookUnsubscribe
{
    public string topic { get; set; }
    public string integration { get; set; }
}

public class FleetRunnrWebhookPayload
{
    public string topic { get; set; }
    public string external_id { get; set; }
    public string? driver_name { get; set; }
    public string? failure_reason { get; set; }
    public string? fulfillment_status { get; set; }
    public decimal? amount { get; set; }
    public string? currency { get; set; }
}