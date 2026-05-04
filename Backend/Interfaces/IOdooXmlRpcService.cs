using Backend.Models;

namespace Backend.Interfaces
{
    public interface IOdooXmlRpcService
    {
        /// <summary>
        /// Authenticate with Odoo and return the user ID
        /// </summary>
        Task<int?> AuthenticateAsync();

        /// <summary>
        /// Create a general POS order in Odoo (create_general_pos_order_api)
        /// Creates order, picking, makes picking done and marks order as paid
        /// </summary>
        Task<OdooCreateOrderResponse> CreateGeneralPosOrderAsync(OdooCreateOrderRequest request);

        /// <summary>
        /// Create or update a customer/contact in Odoo (user_creation_general)
        /// </summary>
        Task<OdooUserCreationResponse> CreateUserGeneralAsync(OdooUserCreationRequest request);

        /// <summary>
        /// Check if codentify (serial numbers) are valid in Odoo (check_valid_codentify_general)
        /// </summary>
        Task<OdooCodentifyValidationResponse> CheckValidCodentifyAsync(OdooCodentifyValidationRequest request);

        /// <summary>
        /// Check if a customer exists in Odoo by email (get_external_profile on res.partner)
        /// Returns the customer's mobile if found, or indicates they don't exist
        /// </summary>
        Task<OdooExternalProfileResponse> GetExternalProfileAsync(OdooExternalProfileRequest request);
    }
}
