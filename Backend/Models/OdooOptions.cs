namespace Backend.Models
{
    public class OdooOptions
    {
        /// <summary>
        /// Odoo server URL (e.g., https://iqos-stage-backup-5748791.dev.odoo.com)
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Odoo database name
        /// </summary>
        public string Database { get; set; } = string.Empty;

        /// <summary>
        /// Odoo username for authentication
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Odoo password for authentication
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Source channel ID for user creation
        /// </summary>
        public int SourceChannel { get; set; }

        /// <summary>
        /// Location ID in Odoo for order creation
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// API authentication code required by Odoo custom APIs
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Retailer ID for user creation
        /// </summary>
        public int? RetailerId { get; set; }

        /// <summary>
        /// Default phone number for anonymous customers
        /// </summary>
        public string AnonymousPhone { get; set; } = string.Empty;
    }
}
