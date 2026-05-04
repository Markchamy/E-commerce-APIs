namespace Backend.Models
{
    #region Odoo API Request/Response DTOs

    /// <summary>
    /// Request for create_general_pos_order_api
    /// </summary>
    public class OdooCreateOrderRequest
    {
        /// <summary>
        /// Invoice/Order reference (required)
        /// </summary>
        public string InvoiceRef { get; set; } = string.Empty;

        /// <summary>
        /// Return flag: '0' for normal order, '1' for return
        /// </summary>
        public string Return { get; set; } = "0";

        /// <summary>
        /// Customer mobile number
        /// </summary>
        public string? Mobile { get; set; }

        /// <summary>
        /// Voucher/discount code (optional)
        /// </summary>
        public string? VoucherCode { get; set; }

        /// <summary>
        /// List of products with their details
        /// </summary>
        public List<OdooOrderProduct> Products { get; set; } = new();

        /// <summary>
        /// Authentication/API code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Order date in format: dd-MM-yyyy HH:mm:ss
        /// </summary>
        public string OrderDate { get; set; } = string.Empty;

        /// <summary>
        /// Location ID in Odoo
        /// </summary>
        public int LocationId { get; set; }
    }

    /// <summary>
    /// Product item for create_general_pos_order_api
    /// </summary>
    public class OdooOrderProduct
    {
        /// <summary>
        /// Product reference/SKU in Odoo
        /// </summary>
        public string ProductRef { get; set; } = string.Empty;

        /// <summary>
        /// Product price
        /// </summary>
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Lot/Serial number (codentify) - optional
        /// </summary>
        public string? Lot { get; set; }
    }

    /// <summary>
    /// Response from create_general_pos_order_api
    /// </summary>
    public class OdooCreateOrderResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Request for user_creation_general
    /// </summary>
    public class OdooUserCreationRequest
    {
        /// <summary>
        /// Mobile number (required)
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Birthdate in format: yyyy-MM-dd
        /// </summary>
        public string? Birthdate { get; set; }

        /// <summary>
        /// Authentication/API code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Terms and conditions acceptance (1 = accepted)
        /// </summary>
        public int TermsConditions { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string Firstname { get; set; } = string.Empty;

        /// <summary>
        /// Last name
        /// </summary>
        public string? Lastname { get; set; }

        /// <summary>
        /// Source channel ID
        /// </summary>
        public int? SourceChannel { get; set; }

        /// <summary>
        /// Retailer ID
        /// </summary>
        public int? RetailerId { get; set; }

        /// <summary>
        /// Whether email is verified
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Gender (e.g., "male", "female")
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Opt-in for marketing (1 = opted in)
        /// </summary>
        public int OptIn { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// City
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Is duty free customer
        /// </summary>
        public bool IsDutyFree { get; set; }
    }

    /// <summary>
    /// Response from user_creation_general
    /// </summary>
    public class OdooUserCreationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }

        /// <summary>
        /// Starter kit ID
        /// </summary>
        public int? StarterKit { get; set; }

        /// <summary>
        /// List of eligible products
        /// </summary>
        public List<OdooEligibilityItem>? EligibilityList { get; set; }

        /// <summary>
        /// Consumer/Partner ID in Odoo
        /// </summary>
        public int? Consumer { get; set; }

        /// <summary>
        /// Customer stage (e.g., "New")
        /// </summary>
        public string? Stage { get; set; }
    }

    /// <summary>
    /// Eligibility item in user_creation_general response
    /// </summary>
    public class OdooEligibilityItem
    {
        public string Name { get; set; } = string.Empty;
        public string InternalRef { get; set; } = string.Empty;
        public bool Eligibility { get; set; }
    }

    /// <summary>
    /// Request for check_valid_codentify_general
    /// </summary>
    public class OdooCodentifyValidationRequest
    {
        /// <summary>
        /// List of products with codentify to validate
        /// </summary>
        public List<OdooCodentifyItem> Products { get; set; } = new();
    }

    /// <summary>
    /// Codentify item for validation
    /// </summary>
    public class OdooCodentifyItem
    {
        /// <summary>
        /// Codentify/Serial number to validate
        /// </summary>
        public string Codentify { get; set; } = string.Empty;

        /// <summary>
        /// Product reference/SKU
        /// </summary>
        public string ProductRef { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from check_valid_codentify_general
    /// </summary>
    public class OdooCodentifyValidationResponse
    {
        public bool Success { get; set; }
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Request for get_external_profile on res.partner
    /// Checks if a customer already exists in Odoo by email
    /// </summary>
    public class OdooExternalProfileRequest
    {
        /// <summary>
        /// Customer email to look up in Odoo
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from get_external_profile on res.partner
    /// </summary>
    public class OdooExternalProfileResponse
    {
        public bool Success { get; set; }

        /// <summary>
        /// "error" if user not found, "success" or other value if found
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Whether the customer exists in Odoo
        /// </summary>
        public bool CustomerExists { get; set; }

        /// <summary>
        /// Customer mobile number from Odoo (when customer exists)
        /// </summary>
        public string? Mobile { get; set; }

        public string? Error { get; set; }
    }

    /// <summary>
    /// PMI controller request for checking external profile
    /// </summary>
    public class PmiExternalProfileRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// PMI controller response for external profile check
    /// </summary>
    public class PmiExternalProfileResponse
    {
        public bool CustomerExists { get; set; }
        public string? Mobile { get; set; }
    }

    #endregion

    #region PMI Controller Request DTOs

    public class PmiCustomerCheckRequest
    {
        public string Phone { get; set; } = string.Empty;
    }

    public class PmiCustomerCreateRequest
    {
        public string Firstname { get; set; } = string.Empty;
        public string? Lastname { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Birthdate { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public int TermsConditions { get; set; } = 1;
        public int OptIn { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsDutyFree { get; set; }
    }

    public class PmiOrderSubmitRequest
    {
        public string InvoiceRef { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public bool IsReturn { get; set; }
        public string? VoucherCode { get; set; }
        public List<PmiOrderProductItem> Products { get; set; } = new();
        public DateTime? OrderDate { get; set; }
    }

    public class PmiOrderProductItem
    {
        /// <summary>
        /// Product reference/SKU in Odoo
        /// </summary>
        public string ProductRef { get; set; } = string.Empty;

        /// <summary>
        /// Product price
        /// </summary>
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Lot/Serial number (codentify) - optional
        /// </summary>
        public string? Lot { get; set; }
    }

    public class PmiCodentifyValidationRequest
    {
        public List<PmiCodentifyItem> Products { get; set; } = new();
    }

    public class PmiCodentifyItem
    {
        public string Codentify { get; set; } = string.Empty;
        public string ProductRef { get; set; } = string.Empty;
    }

    public class PmiErrorCreateRequest
    {
        public string Error { get; set; } = string.Empty;
    }

    public class PmiCustomerAddRequest
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
    }

    public class PmiOrderAddRequest
    {
        public string OrderReference { get; set; } = string.Empty;
        public string? OrderNumber { get; set; }
        public DateTime? DateDelivered { get; set; }
        public long? CustomerId { get; set; }
        public DateTime? DateCreated { get; set; }
        public long? ErrorId { get; set; }
        public bool? Anonymous { get; set; }
    }

    public class PmiOrderUpdateRequest
    {
        public string? OrderNumber { get; set; }
        public DateTime? DateDelivered { get; set; }
        public bool? Anonymous { get; set; }
    }

    public class PmiOrderProductsAddRequest
    {
        public List<PmiOrderedProductDto> Products { get; set; } = new();
        public List<string>? SerialNumbers { get; set; }
    }

    public class PmiOrderedProductDto
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    #endregion

    #region PMI Controller Response DTOs

    public class PmiOrderListResponse
    {
        public List<PmiOrderDto> Orders { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class PmiErrorListResponse
    {
        public List<PmiErrorDto> Errors { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class PmiOrderDto
    {
        public string OrderReference { get; set; } = string.Empty;
        public string? OrderNumber { get; set; }
        public DateTime? DateDelivered { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool? Anonymous { get; set; }
        public long? CustomerId { get; set; }
        public long? ErrorId { get; set; }
        public PmiCustomerDto? Customer { get; set; }
        public PmiErrorDto? Error { get; set; }
        public List<PmiOrderedProductResponseDto>? Products { get; set; }
        public List<string>? SerialNumbers { get; set; }
    }

    public class PmiCustomerDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
    }

    public class PmiErrorDto
    {
        public long Id { get; set; }
        public string? Error { get; set; }
    }

    public class PmiOrderedProductResponseDto
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
    }

    public class PmiCustomerCreateResponse
    {
        public bool Success { get; set; }
        public int? OdooConsumerId { get; set; }
        public int? StarterKit { get; set; }
        public string? Stage { get; set; }
        public List<OdooEligibilityItem>? EligibilityList { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    public class PmiOrderSubmitResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    public class PmiCodentifyValidationResponse
    {
        public bool Success { get; set; }
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    public class PmiProductPriceResponse
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public bool Found { get; set; }
    }

    public class PmiSerialNumberResponse
    {
        public string OrderReference { get; set; } = string.Empty;
        public List<string> SerialNumbers { get; set; } = new();
    }

    /// <summary>
    /// Response for customer metafields endpoint
    /// Like Java project: checks "birthday" and "your_consent" metafields
    /// </summary>
    public class PmiCustomerMetafieldsResponse
    {
        // Use lowercase to match frontend expectations
        public List<PmiMetafieldItem> metafields { get; set; } = new();
    }

    /// <summary>
    /// Single metafield item matching Shopify metafield structure
    /// </summary>
    public class PmiMetafieldItem
    {
        // Use lowercase to match frontend expectations
        public string key { get; set; } = string.Empty;
        public string? value { get; set; }
        public string? @namespace { get; set; }
    }

    #endregion

    #region Filter Parameters

    public class PmiOrderFilterParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? OrderReference { get; set; }
        public string? OrderNumber { get; set; }
        public bool? HasError { get; set; }
        public bool? Anonymous { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortBy { get; set; } = "DateCreated";
        public string? SortDirection { get; set; } = "desc";
    }

    public class PmiErrorFilterParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "Id";
        public string? SortDirection { get; set; } = "desc";
    }

    #endregion
}
