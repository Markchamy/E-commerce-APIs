namespace Backend.Models
{
    /// <summary>
    /// Application-wide settings configured in appsettings.json
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Base URL for the application (e.g., http://52.73.44.227)
        /// Used for generating image URLs and other public-facing resources
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
    }
}
