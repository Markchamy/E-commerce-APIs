namespace Backend.Models
{
    public class JobApplicationModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string TransportationMode {  get; set; }
        public string HomeAddress { get; set; }
        public string? CoverLetter { get; set; }
        public IFormFile? Resume { get; set; }
    }
}
