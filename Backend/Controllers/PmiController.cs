using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{
    [ApiController]
    [Route("pmi")]
    public class PmiController : ControllerBase
    {
        private readonly IPmiServices _pmiServices;
        private readonly IOdooXmlRpcService _odooService;
        private readonly IUserServices _userServices;
        private readonly OdooOptions _odooOptions;
        private readonly ILogger<PmiController> _logger;

        public PmiController(
            IPmiServices pmiServices,
            IOdooXmlRpcService odooService,
            IUserServices userServices,
            IOptions<OdooOptions> odooOptions,
            ILogger<PmiController> logger)
        {
            _pmiServices = pmiServices;
            _odooService = odooService;
            _userServices = userServices;
            _odooOptions = odooOptions.Value;
            _logger = logger;
        }

        #region Odoo Integration APIs

        /// <summary>
        /// 1. Create a customer/contact in Odoo (user_creation_general)
        /// </summary>
        /// <remarks>
        /// Creates or updates a contact in Odoo with full customer details.
        /// Returns starter kit eligibility and consumer ID.
        ///
        /// Sample request:
        ///     POST /pmi/customer/create
        ///     {
        ///         "firstname": "John",
        ///         "lastname": "Doe",
        ///         "mobile": "+9611234567",
        ///         "email": "john@example.com",
        ///         "birthdate": "1990-01-15",
        ///         "gender": "male",
        ///         "termsConditions": 1
        ///     }
        /// </remarks>
        [HttpPost("customer/create")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCustomer([FromBody] PmiCustomerCreateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Mobile) || string.IsNullOrEmpty(request.Firstname))
                {
                    return BadRequest(ResponseBase.Failure("Mobile and firstname are required"));
                }

                var odooRequest = new OdooUserCreationRequest
                {
                    Mobile = request.Mobile,
                    Firstname = request.Firstname,
                    Lastname = request.Lastname,
                    Email = request.Email,
                    Birthdate = request.Birthdate,
                    Gender = request.Gender,
                    Address = request.Address,
                    Country = request.Country,
                    City = request.City,
                    TermsConditions = request.TermsConditions,
                    OptIn = request.OptIn,
                    EmailVerified = request.EmailVerified,
                    IsDutyFree = request.IsDutyFree,
                    Code = _odooOptions.Code,
                    SourceChannel = _odooOptions.SourceChannel,
                    RetailerId = _odooOptions.RetailerId
                };

                var result = await _odooService.CreateUserGeneralAsync(odooRequest);

                if (result.Success)
                {
                    var response = new PmiCustomerCreateResponse
                    {
                        Success = true,
                        OdooConsumerId = result.Consumer,
                        StarterKit = result.StarterKit,
                        Stage = result.Stage,
                        EligibilityList = result.EligibilityList,
                        Message = "Customer created successfully in Odoo"
                    };
                    return Ok(ResponseBase.Success("Customer created successfully", response));
                }

                return BadRequest(ResponseBase.Failure(result.Error ?? "Failed to create customer in Odoo"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer in Odoo");
                return StatusCode(500, ResponseBase.Failure($"Error creating customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// 2. Create a POS order in Odoo (create_general_pos_order_api)
        /// </summary>
        /// <remarks>
        /// Creates order, picking, makes picking done and marks order as paid.
        /// Codentify/serial numbers are passed as the 'lot' field in each product.
        ///
        /// Sample request:
        ///     POST /pmi/order/create
        ///     {
        ///         "invoiceRef": "INV-2024-001",
        ///         "mobile": "+9611234567",
        ///         "isReturn": false,
        ///         "products": [
        ///             {
        ///                 "productRef": "373336",
        ///                 "productPrice": 10000,
        ///                 "quantity": 1,
        ///                 "lot": "CODENTIFY123"
        ///             }
        ///         ]
        ///     }
        /// </remarks>
        [HttpPost("order/create")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] PmiOrderSubmitRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.InvoiceRef))
                {
                    return BadRequest(ResponseBase.Failure("Invoice reference is required"));
                }

                if (request.Products == null || !request.Products.Any())
                {
                    return BadRequest(ResponseBase.Failure("At least one product is required"));
                }

                // Format order date
                var orderDate = (request.OrderDate ?? DateTime.Now).ToString("dd-MM-yyyy HH:mm:ss");

                var odooRequest = new OdooCreateOrderRequest
                {
                    InvoiceRef = request.InvoiceRef,
                    Return = request.IsReturn ? "1" : "0",
                    Mobile = request.Mobile ?? string.Empty,
                    VoucherCode = request.VoucherCode,
                    Code = _odooOptions.Code,
                    OrderDate = orderDate,
                    LocationId = _odooOptions.LocationId,
                    Products = request.Products.Select(p => new OdooOrderProduct
                    {
                        ProductRef = p.ProductRef,
                        ProductPrice = p.ProductPrice,
                        Quantity = p.Quantity,
                        Lot = p.Lot // Codentify/serial number
                    }).ToList()
                };

                var result = await _odooService.CreateGeneralPosOrderAsync(odooRequest);

                if (result.Success)
                {
                    // Clear any previous error for this order in local DB
                    await _pmiServices.ClearOrderErrorAsync(request.InvoiceRef);

                    var response = new PmiOrderSubmitResponse
                    {
                        Success = true,
                        Message = "Order created successfully in Odoo"
                    };
                    return Ok(ResponseBase.Success("Order created successfully", response));
                }

                // Store error in local database for retry
                await _pmiServices.UpdateOrderErrorAsync(request.InvoiceRef, result.Error);

                return BadRequest(ResponseBase.Failure(result.Error ?? "Failed to create order in Odoo"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order in Odoo");
                await _pmiServices.UpdateOrderErrorAsync(request.InvoiceRef, ex.Message);
                return StatusCode(500, ResponseBase.Failure($"Error creating order: {ex.Message}"));
            }
        }

        /// <summary>
        /// 3. Validate codentify/serial numbers in Odoo (check_valid_codentify_general)
        /// </summary>
        /// <remarks>
        /// Checks if the codentify/serial numbers exist and are valid in Odoo inventory.
        /// Use this before creating an order to validate serial numbers.
        ///
        /// Sample request:
        ///     POST /pmi/codentify/validate
        ///     {
        ///         "products": [
        ///             {
        ///                 "codentify": "6F49YL9BEZ88",
        ///                 "productRef": "373336"
        ///             }
        ///         ]
        ///     }
        /// </remarks>
        [HttpPost("codentify/validate")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateCodentify([FromBody] PmiCodentifyValidationRequest request)
        {
            try
            {
                if (request.Products == null || !request.Products.Any())
                {
                    return BadRequest(ResponseBase.Failure("At least one product with codentify is required"));
                }

                var odooRequest = new OdooCodentifyValidationRequest
                {
                    Products = request.Products.Select(p => new OdooCodentifyItem
                    {
                        Codentify = p.Codentify,
                        ProductRef = p.ProductRef
                    }).ToList()
                };

                var result = await _odooService.CheckValidCodentifyAsync(odooRequest);

                var response = new PmiCodentifyValidationResponse
                {
                    Success = result.Success,
                    IsValid = result.IsValid,
                    Message = result.Message,
                    Error = result.Error
                };

                if (result.Success)
                {
                    return Ok(ResponseBase.Success(
                        result.IsValid ? "Codentify is valid" : "Codentify is not valid",
                        response));
                }

                return BadRequest(ResponseBase.Failure(result.Error ?? "Validation failed", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating codentify");
                return StatusCode(500, ResponseBase.Failure($"Error validating codentify: {ex.Message}"));
            }
        }

        /// <summary>
        /// 4. Check if a customer exists in Odoo by email (get_external_profile)
        /// </summary>
        /// <remarks>
        /// Checks if a customer already exists in Odoo via res.partner get_external_profile.
        /// Returns whether the customer exists and their mobile number if found.
        ///
        /// Sample request:
        ///     POST /pmi/customer/external-profile
        ///     {
        ///         "email": "john@example.com"
        ///     }
        /// </remarks>
        [HttpPost("customer/external-profile")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckExternalProfile([FromBody] PmiExternalProfileRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(ResponseBase.Failure("Email is required"));
                }

                var odooRequest = new OdooExternalProfileRequest
                {
                    Email = request.Email
                };

                var result = await _odooService.GetExternalProfileAsync(odooRequest);

                if (result.Success)
                {
                    var response = new PmiExternalProfileResponse
                    {
                        CustomerExists = result.CustomerExists,
                        Mobile = result.Mobile
                    };
                    return Ok(ResponseBase.Success(
                        result.CustomerExists ? "Customer found in Odoo" : "Customer not found in Odoo",
                        response));
                }

                return BadRequest(ResponseBase.Failure(result.Error ?? "Failed to check external profile"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking external profile for email {Email}", request.Email);
                return StatusCode(500, ResponseBase.Failure($"Error checking external profile: {ex.Message}"));
            }
        }

        /// <summary>
        /// 5. Get customer metafields from local database
        /// </summary>
        /// <remarks>
        /// Fetches customer metafields (birthday, consent) from local database.
        /// Like Java project: checks "birthday" and "your_consent" metafields to determine if anonymous.
        ///
        /// Sample response:
        ///     GET /pmi/customer/12345/metafields
        ///     {
        ///         "isSuccess": true,
        ///         "data": {
        ///             "metafields": [
        ///                 { "key": "birthday", "value": "1990-01-15" },
        ///                 { "key": "your_consent", "value": "true" }
        ///             ]
        ///         }
        ///     }
        /// </remarks>
        [HttpGet("customer/{customerId}/metafields")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerMetafields(long customerId)
        {
            try
            {
                // Fetch user by ID from local database
                // The customerId here maps to the User ID (users table)
                var user = await _userServices.GetUserByIdAsync(customerId);

                if (user == null)
                {
                    // Return empty metafields if user not found (will be treated as anonymous)
                    var emptyResponse = new PmiCustomerMetafieldsResponse
                    {
                        metafields = new List<PmiMetafieldItem>()
                    };
                    return Ok(ResponseBase.Success("Customer not found, returning empty metafields", emptyResponse));
                }

                // Build metafields list matching Java project's expected fields
                var metafieldsList = new List<PmiMetafieldItem>();

                // Add birthday metafield if available
                if (user.Birthday.HasValue)
                {
                    metafieldsList.Add(new PmiMetafieldItem
                    {
                        key = "birthday",
                        value = user.Birthday.Value.ToString("yyyy-MM-dd"),
                        @namespace = "customer"
                    });
                }

                // Add consent metafield from customer's EmailSmsOptIn field
                if (user.Customer != null && user.Customer.EmailSmsOptIn.HasValue)
                {
                    metafieldsList.Add(new PmiMetafieldItem
                    {
                        key = "your_consent",
                        value = user.Customer.EmailSmsOptIn.Value ? "true" : "false",
                        @namespace = "customer"
                    });
                }

                var response = new PmiCustomerMetafieldsResponse
                {
                    metafields = metafieldsList
                };

                return Ok(ResponseBase.Success("Customer metafields retrieved", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer metafields for customer {CustomerId}", customerId);
                return StatusCode(500, ResponseBase.Failure($"Error getting customer metafields: {ex.Message}"));
            }
        }

        #endregion

        #region Local Database APIs (for tracking/management)

        /// <summary>
        /// Get a customer from the local database by phone number
        /// </summary>
        [HttpGet("customer/by-phone/{phone}")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerByPhone(string phone)
        {
            try
            {
                var customer = await _pmiServices.GetCustomerByPhoneAsync(phone);
                if (customer == null)
                {
                    return NotFound(ResponseBase.Failure("Customer not found"));
                }

                // Map to DTO to avoid exposing the full model if needed, but for now returning the model
                var customerDto = new PmiCustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    LastName = customer.LastName,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Address = customer.Address
                };

                return Ok(ResponseBase.Success("Customer found", customerDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by phone {Phone}", phone);
                return StatusCode(500, ResponseBase.Failure($"Error getting customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// 4. Get all PMI orders from local database with pagination and filters
        /// </summary>
        [HttpGet("orders")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrders([FromQuery] PmiOrderFilterParams filters)
        {
            try
            {
                var result = await _pmiServices.GetOrdersAsync(filters);
                return Ok(ResponseBase.Success("Orders retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PMI orders");
                return StatusCode(500, ResponseBase.Failure($"Error retrieving orders: {ex.Message}"));
            }
        }

        /// <summary>
        /// 5. Get products for a specific order from local database
        /// </summary>
        [HttpGet("orders/{orderReference}/products")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderProducts(string orderReference)
        {
            try
            {
                var order = await _pmiServices.GetOrderByReferenceAsync(orderReference);
                if (order == null)
                {
                    return NotFound(ResponseBase.Failure("Order not found"));
                }

                var products = await _pmiServices.GetOrderProductsAsync(orderReference);
                return Ok(ResponseBase.Success("Products retrieved successfully", products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order products for {OrderReference}", orderReference);
                return StatusCode(500, ResponseBase.Failure($"Error retrieving products: {ex.Message}"));
            }
        }

        /// <summary>
        /// 6. Get serial numbers (codentify) for an order from local database
        /// </summary>
        [HttpGet("order/{orderReference}/serial")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderSerialNumbers(string orderReference)
        {
            try
            {
                var order = await _pmiServices.GetOrderByReferenceAsync(orderReference);
                if (order == null)
                {
                    return NotFound(ResponseBase.Failure("Order not found"));
                }

                var serialNumbers = await _pmiServices.GetOrderSerialNumbersAsync(orderReference);
                var response = new PmiSerialNumberResponse
                {
                    OrderReference = orderReference,
                    SerialNumbers = serialNumbers
                };

                return Ok(ResponseBase.Success("Serial numbers retrieved", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serial numbers for order {OrderReference}", orderReference);
                return StatusCode(500, ResponseBase.Failure($"Error getting serial numbers: {ex.Message}"));
            }
        }

        /// <summary>
        /// 7. Retry failed order submission to Odoo
        /// </summary>
        [HttpPost("order/{orderReference}/retry")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RetryOrder(string orderReference)
        {
            try
            {
                var order = await _pmiServices.GetOrderByReferenceAsync(orderReference);
                if (order == null)
                {
                    return NotFound(ResponseBase.Failure("Order not found"));
                }

                if (order.ErrorId == null)
                {
                    return BadRequest(ResponseBase.Failure("Order does not have an error to retry"));
                }

                // Get products and serial numbers for the order
                var products = await _pmiServices.GetOrderProductsAsync(orderReference);
                var serialNumbers = await _pmiServices.GetOrderSerialNumbersAsync(orderReference);

                // Format order date
                var orderDate = (order.DateDelivered ?? DateTime.Now).ToString("dd-MM-yyyy HH:mm:ss");

                // Build products with codentify
                var odooProducts = new List<OdooOrderProduct>();
                var serialIndex = 0;
                foreach (var product in products)
                {
                    var odooProduct = new OdooOrderProduct
                    {
                        ProductRef = product.ProductId.ToString(), // Assuming ProductId maps to product_ref
                        ProductPrice = product.Price ?? 0,
                        Quantity = product.Quantity
                    };

                    // Assign serial numbers to products if available
                    if (serialIndex < serialNumbers.Count)
                    {
                        odooProduct.Lot = serialNumbers[serialIndex];
                        serialIndex++;
                    }

                    odooProducts.Add(odooProduct);
                }

                var odooRequest = new OdooCreateOrderRequest
                {
                    InvoiceRef = orderReference,
                    Return = "0",
                    Mobile = order.Customer?.Phone ?? string.Empty,
                    Code = _odooOptions.Code,
                    OrderDate = orderDate,
                    LocationId = _odooOptions.LocationId,
                    Products = odooProducts
                };

                var result = await _odooService.CreateGeneralPosOrderAsync(odooRequest);

                if (result.Success)
                {
                    await _pmiServices.ClearOrderErrorAsync(orderReference);
                    return Ok(ResponseBase.Success("Order retry successful", result));
                }

                await _pmiServices.UpdateOrderErrorAsync(orderReference, result.Error);
                return BadRequest(ResponseBase.Failure(result.Error ?? "Retry failed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying order {OrderReference}", orderReference);
                await _pmiServices.UpdateOrderErrorAsync(orderReference, ex.Message);
                return StatusCode(500, ResponseBase.Failure($"Error retrying order: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an order in the local database
        /// </summary>
        [HttpPut("order/{orderReference}")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrder(string orderReference, [FromBody] PmiOrderUpdateRequest request)
        {
            try
            {
                var result = await _pmiServices.UpdateOrderAsync(orderReference, request);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderReference}", orderReference);
                return StatusCode(500, ResponseBase.Failure($"Error updating order: {ex.Message}"));
            }
        }

        /// <summary>
        /// 8. Delete PMI order from local database
        /// </summary>
        [HttpDelete("order/{orderReference}")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrder(string orderReference)
        {
            try
            {
                var result = await _pmiServices.DeleteOrderAsync(orderReference);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderReference}", orderReference);
                return StatusCode(500, ResponseBase.Failure($"Error deleting order: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all errors from the local database with pagination and filters
        /// </summary>
        [HttpGet("errors")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetErrors([FromQuery] PmiErrorFilterParams filters)
        {
            try
            {
                var result = await _pmiServices.GetErrorsAsync(filters);
                return Ok(ResponseBase.Success("Errors retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PMI errors");
                return StatusCode(500, ResponseBase.Failure($"Error retrieving errors: {ex.Message}"));
            }
        }

        /// <summary>
        /// 9. Add a generic error to the local database
        /// </summary>
        [HttpPost("error")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateError([FromBody] PmiErrorCreateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Error))
                {
                    return BadRequest(ResponseBase.Failure("Error message cannot be empty"));
                }

                var error = await _pmiServices.CreateErrorAsync(request.Error);

                return Ok(ResponseBase.Success("Error created successfully", error));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a new error entry");
                return StatusCode(500, ResponseBase.Failure($"Error creating error: {ex.Message}"));
            }
        }

        /// <summary>
        /// 9. Add customer to local database (for tracking)
        /// </summary>
        [HttpPost("customer")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCustomer([FromBody] PmiCustomerAddRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Phone))
                {
                    return BadRequest(ResponseBase.Failure("Name and phone are required"));
                }

                var customer = new PmiCustomer
                {
                    Id = request.Id,
                    Name = request.Name,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    Email = request.Email,
                    Address = request.Address
                };

                var result = await _pmiServices.AddCustomerAsync(customer);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer");
                return StatusCode(500, ResponseBase.Failure($"Error adding customer: {ex.Message}"));
            }
        }

        /// <summary>
        /// 10. Add order to local database (for tracking)
        /// </summary>
        [HttpPost("order")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddOrder([FromBody] PmiOrderAddRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OrderReference))
                {
                    return BadRequest(ResponseBase.Failure("Order reference is required"));
                }

                var order = new PmiOrder
                {
                    OrderReference = request.OrderReference,
                    OrderNumber = request.OrderNumber,
                    DateDelivered = request.DateDelivered,
                    CustomerId = request.CustomerId,
                    DateCreated = request.DateCreated ?? DateTime.UtcNow,
                    ErrorId = request.ErrorId,
                    Anonymous = request.Anonymous
                };

                var result = await _pmiServices.AddOrderAsync(order);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order");
                return StatusCode(500, ResponseBase.Failure($"Error adding order: {ex.Message}"));
            }
        }

        /// <summary>
        /// 11. Add products to an order in local database
        /// </summary>
        [HttpPost("order/{orderReference}/products")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddOrderProducts(string orderReference, [FromBody] PmiOrderProductsAddRequest request)
        {
            try
            {
                if (request.Products == null || !request.Products.Any())
                {
                    return BadRequest(ResponseBase.Failure("At least one product is required"));
                }

                var products = request.Products.Select(p => new PmiOrderedProduct
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    Price = p.Price
                }).ToList();

                var machines = request.SerialNumbers?.Select(s => new PmiOrderedMachine
                {
                    SerialNum = s
                }).ToList();

                var result = await _pmiServices.AddOrderProductsAsync(orderReference, products, machines);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding products to order {OrderReference}", orderReference);
                return StatusCode(500, ResponseBase.Failure($"Error adding products: {ex.Message}"));
            }
        }

        /// <summary>
        /// 12. Get product price by name from local database
        /// </summary>
        [HttpGet("product/{productName}/price")]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductPrice(string productName)
        {
            try
            {
                var product = await _pmiServices.GetProductByNameAsync(productName);
                var response = new PmiProductPriceResponse
                {
                    ProductName = productName,
                    Price = product?.Price,
                    Found = product != null
                };

                if (product != null)
                {
                    return Ok(ResponseBase.Success("Product found", response));
                }
                return NotFound(ResponseBase.Failure("Product not found", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product price for {ProductName}", productName);
                return StatusCode(500, ResponseBase.Failure($"Error getting product price: {ex.Message}"));
            }
        }

        #endregion
    }
}
