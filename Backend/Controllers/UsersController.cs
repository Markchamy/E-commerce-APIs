using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;
using Backend.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Net;
using System.Numerics;
using System.Xml.Linq;
using System.Diagnostics.Metrics;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;


namespace Backend.Controllers
{
    [ApiController]
    [Route("admin/api/2024-01")]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userRepository;
        private readonly IConfiguration _configuration;

        public UsersController(IUserServices userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        // API to register user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO userDto)
        {
            // Validate the incoming data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userDtoUser = userDto.User;

            // Check if the user already exists
            var userExists = await _userRepository.UserExists(userDtoUser.Email);
            if (userExists)
            {
                return BadRequest("Email already in use.");
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDtoUser.Password);

            // Map DTO to UserModel
            var user = new UserModel
            {
                role = userDtoUser.Role,
                first_name = userDtoUser.FirstName,
                last_name = userDtoUser.LastName,
                email = userDtoUser.Email,
                password = hashedPassword,
                phone_number = userDtoUser.PhoneNumber,
                Birthday = userDtoUser.Birthday
            };

            // Add user to database
            var addUserResponse = await _userRepository.AddUser(user);

            await _userRepository.SaveChangesAsync();

            if (user.role == "customer" && userDtoUser.Customer != null)
            {
                var customer = new CustomerModel
                {
                    // Map customer-specific fields from nested DTO
                    Company = userDtoUser.Customer.Company,
                    Address = userDtoUser.Customer.Address,
                    Apartment = userDtoUser.Customer.Apartment,
                    City = userDtoUser.Customer.City,
                    Country = userDtoUser.Customer.Country,
                    EmailSmsOptIn = userDtoUser.Customer.EmailSmsOptIn,
                    Newsletter = userDtoUser.Customer.Newsletter,
                    UserId = user.Id // Link to the newly created user
                };

                var addCustomerResponse = await _userRepository.AddCustomer(customer);
                if (!addCustomerResponse.IsSuccess)
                {
                    return BadRequest(addCustomerResponse.Message);
                } 
                await _userRepository.SaveChangesAsync();

                // Handling addresses if any
                foreach (var addressDto in userDtoUser.Customer.Addresses)
                {
                    var address = new AddressesModel
                    {
                        Address2 = addressDto.Address2,
                        Province = addressDto.Province,
                        Zip = addressDto.Zip,
                        ProvinceCode = addressDto.ProvinceCode,
                        CountryCode = addressDto.CountryCode,
                        CountryName = addressDto.CountryName,
                        CustomerId = customer.Id, // Link to the newly created customer
                        first_name = userDtoUser.FirstName,
                        last_name = userDtoUser.LastName,
                        Company = customer.Company,
                        Address1 = customer.Address,
                        City = customer.City,
                        Country = customer.Country,
                        Phone = userDtoUser.PhoneNumber,
                        Name = $"{userDtoUser.FirstName} {userDtoUser.LastName}"
                    };

                     var addAddressResponse = await _userRepository.AddAddresses(address);
                    if (!addAddressResponse.IsSuccess)
                    {
                        return BadRequest(addAddressResponse.Message);
                    }
                }
                await _userRepository.SaveChangesAsync();
            }

            if (user.role == "employee" && userDtoUser.Employee != null)
            {
                var employee = new EmployeeModel
                {
                    // Map employee-specific fields from nested DTO
                    AccessControl = user.Employee.AccessControl,
                    UserId = user.Id // Link to the newly created user
                };

                var addEmployeeResponse = await _userRepository.AddEmployee(employee);
                if (!addEmployeeResponse.IsSuccess)
                {
                    return BadRequest(addEmployeeResponse.Message);
                } 
                await _userRepository.SaveChangesAsync();
            }

            await _userRepository.SaveChangesAsync();

            // Send welcome email
            await _userRepository.SendWelcomeEmail(user.email, user.first_name);

            // Return response
            return Ok(new { message = "Registration Successful" , userDto});
        }

        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeeDTO employeeDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var u = employeeDto.User;

            // 1) unique email
            if (await _userRepository.UserExists(u.Email))
                return BadRequest("Email already in use.");

            // 2) create user (you’re providing the Id manually – that’s fine)
            var user = new UserModel
            {
                role = "employee",
                Id = u.Id,
                first_name = u.FirstName,
                last_name = u.LastName,
                email = u.Email,
                password = BCrypt.Net.BCrypt.HashPassword(u.Password),
                phone_number = u.PhoneNumber,
            };

            try
            {
                await _userRepository.AddUser(user);
                await _userRepository.SaveChangesAsync();   // ensure the user row exists
            }
            catch (DbUpdateException ex)
            {
                var root = ex.GetBaseException()?.Message ?? ex.Message;
                // _logger?.LogError(ex, "Error adding user: {Error}", root);
                return StatusCode(500, $"Error adding user: {root}");
            }

            // 3) create employee
            var employee = new EmployeeModel
            {
                // IMPORTANT: use the actual saved user.Id
                UserId = user.Id,
                // If the DTO might omit Employee/AccessControl, guard it:
                AccessControl = u.Employee?.AccessControl ?? new List<string>()
                // If your EmployeeModel.AccessControl is a *string* column instead of List<string>,
                // use one of these instead:
                // AccessControl = string.Join(",", u.Employee?.AccessControl ?? new List<string>()),
                // or serialize to JSON and store in a TEXT/JSON column:
                // AccessControl = JsonSerializer.Serialize(u.Employee?.AccessControl ?? new List<string>())
            };

            try
            {
                var addEmployeeResponse = await _userRepository.AddEmployee(employee);
                if (!addEmployeeResponse.IsSuccess)
                    return BadRequest(addEmployeeResponse.Message);

                await _userRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var root = ex.GetBaseException()?.Message ?? ex.Message;

                // Optional: classify common MySQL/MariaDB errors for clearer messages
                if (root.Contains("Duplicate entry") || root.Contains("1062"))
                    return Conflict($"Duplicate key when saving employee: {root}");

                if (root.Contains("foreign key constraint fails") || root.Contains("1452"))
                    return BadRequest($"Foreign key failed (employees.user_id -> users.id). Check types and that user.Id exists. Details: {root}");

                // _logger?.LogError(ex, "Error adding employee: {Error}", root);
                return StatusCode(500, $"Error adding employee: {root}");
            }

            return Ok(new { message = "Employee registration successful" });
        }
        [HttpGet("get-all-employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _userRepository.GetAllEmployees();

                if (employees == null || !employees.Any())
                {
                    return NotFound("No employees found.");
                }

                var employeeDtos = employees.Select(e => new UserDTO
                {
                    Id = e.User?.Id ?? 0,
                    Role = e.User?.role ?? string.Empty,
                    FirstName = e.User?.first_name ?? string.Empty,
                    LastName = e.User?.last_name ?? string.Empty,
                    Email = e.User?.email ?? string.Empty,
                    PhoneNumber = e.User?.phone_number ?? string.Empty,
                    Employee = e.AccessControl != null
                        ? new EmployeeDTO { AccessControl = e.AccessControl }
                        : null
                }).ToList();

                return Ok(employeeDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message} - StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDTO userDto)
        {
            try
            {
                // ✅ Step 1: Validate input
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("[ERROR] Invalid model state");
                    return BadRequest(ModelState);
                }

                var userDtoUser = userDto.UserCustomer;
                Console.WriteLine($"[DEBUG] Incoming registration for: {userDtoUser.Email}");

                // ✅ Step 2: Check if email already exists
                var userExists = await _userRepository.UserExists(userDtoUser.Email);
                if (userExists)
                {
                    Console.WriteLine("[ERROR] Email already in use");
                    return BadRequest("Email already in use.");
                }

                // ✅ Step 3: Hash password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDtoUser.Password);

                // ✅ Step 3.5: Get next User ID
                var nextUserId = await _userRepository.GetNextUserIdAsync();
                Console.WriteLine($"[DEBUG] Next User ID: {nextUserId}");

                // ✅ Step 4: Create User
                var user = new UserModel
                {
                    Id = nextUserId,
                    role = "customer",
                    first_name = userDtoUser.FirstName,
                    last_name = userDtoUser.LastName,
                    Birthday = userDtoUser.Birthday,
                    phone_number = userDtoUser.PhoneNumber,
                    email = userDtoUser.Email,
                    password = hashedPassword
                };

                Console.WriteLine($"[DEBUG] Creating User: {user.email}");

                var addUserResponse = await _userRepository.AddUser(user);
                if (!addUserResponse.IsSuccess)
                {
                    Console.WriteLine($"[ERROR] AddUser failed: {addUserResponse.Message}");
                    return BadRequest(addUserResponse.Message);
                }

                await _userRepository.SaveChangesAsync(); // ✅ ensures User.Id is generated

                // ✅ Step 4.5: Get next Customer ID
                var nextCustomerId = await _userRepository.GetNextCustomerIdAsync();
                Console.WriteLine($"[DEBUG] Next Customer ID: {nextCustomerId}");

                // ✅ Step 5: Create Customer
                var customer = new CustomerModel
                {
                    Id = nextCustomerId,
                    Company = userDtoUser.Customer?.Company ?? "",
                    Address = userDtoUser.Customer?.Address ?? "",
                    Apartment = userDtoUser.Customer?.Apartment ?? "",
                    City = userDtoUser.Customer?.City ?? "",
                    Country = userDtoUser.Customer?.Country ?? "",
                    EmailSmsOptIn = userDtoUser.Customer?.EmailSmsOptIn ?? false,
                    Newsletter = userDtoUser.Customer?.Newsletter ?? false,
                    tags = userDtoUser.Customer?.tags ?? "",
                    OrdersCount = userDtoUser.Customer?.OrdersCount ?? 0,
                    state = userDtoUser.Customer?.state ?? false,
                    TotalSpent = userDtoUser.Customer?.TotalSpent ?? 0.00,
                    LastOrderId = userDtoUser.Customer?.LastOrderId ?? 0,
                    note = userDtoUser.Customer?.note ?? "",
                    LastOrderName = userDtoUser.Customer?.LastOrderName ?? "",
                    Currency = userDtoUser.Customer?.Currency ?? "",
                    UserId = user.Id
                };

                Console.WriteLine($"[DEBUG] Creating Customer for UserId {user.Id}");

                var addCustomerResponse = await _userRepository.AddCustomer(customer);
                if (!addCustomerResponse.IsSuccess)
                {
                    Console.WriteLine($"[ERROR] AddCustomer failed: {addCustomerResponse.Message}");
                    return BadRequest(addCustomerResponse.Message);
                }

                await _userRepository.SaveChangesAsync(); // ✅ ensures Customer.Id is generated
                Console.WriteLine($"[SUCCESS] Customer created with Id: {customer.Id}");

                // ✅ Step 6: Handle Addresses
                if (userDtoUser.Customer?.Addresses != null && userDtoUser.Customer.Addresses.Any())
                {
                    Console.WriteLine($"[DEBUG] Adding {userDtoUser.Customer.Addresses.Count} provided addresses");

                    foreach (var addr in userDtoUser.Customer.Addresses)
                    {
                        var nextAddressId = await _userRepository.GetNextAddressIdAsync();
                        Console.WriteLine($"[DEBUG] Next Address ID: {nextAddressId}");

                        var address = new AddressesModel
                        {
                            Id = nextAddressId,
                            CustomerId = customer.Id,
                            first_name = userDtoUser.FirstName,
                            last_name = userDtoUser.LastName,
                            Company = customer.Company,
                            Address1 = addr.Address1 ?? "",
                            Address2 = addr.Address2 ?? "",
                            City = addr.City ?? "",
                            Country = addr.CountryName ?? customer.Country,
                            Phone = addr.Phone ?? userDtoUser.PhoneNumber,
                            Name = addr.Name ?? $"{userDtoUser.FirstName} {userDtoUser.LastName}"
                        };

                        var addAddrResponse = await _userRepository.AddAddresses(address);
                        if (!addAddrResponse.IsSuccess)
                        {
                            Console.WriteLine($"[ERROR] AddAddresses failed: {addAddrResponse.Message}");
                            return BadRequest(addAddrResponse.Message);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[DEBUG] No addresses provided → creating default address");

                    var nextAddressId = await _userRepository.GetNextAddressIdAsync();
                    Console.WriteLine($"[DEBUG] Next Address ID (default): {nextAddressId}");

                    var defaultAddress = new AddressesModel
                    {
                        Id = nextAddressId,
                        CustomerId = customer.Id,
                        first_name = userDtoUser.FirstName,
                        last_name = userDtoUser.LastName,
                        Company = customer.Company,
                        Address1 = customer.Address ?? "",
                        Address2 = customer.Apartment ?? "",
                        City = customer.City ?? "",
                        Country = customer.Country ?? "",
                        Phone = userDtoUser.PhoneNumber,
                        Name = $"{userDtoUser.FirstName} {userDtoUser.LastName}"
                    };

                    var addAddressResponse = await _userRepository.AddAddresses(defaultAddress);
                    if (!addAddressResponse.IsSuccess)
                    {
                        Console.WriteLine($"[ERROR] Default AddAddresses failed: {addAddressResponse.Message}");
                        return BadRequest(addAddressResponse.Message);
                    }
                }

                await _userRepository.SaveChangesAsync();
                Console.WriteLine($"[SUCCESS] User {user.email} registered successfully with CustomerId {customer.Id}");

                return Ok(new { message = "Registration Successful", user = user.email, customerId = customer.Id });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                Console.WriteLine($"[EXCEPTION] {ex.Message} | Inner: {inner} | StackTrace: {ex.StackTrace}");

                // Log entity validation errors (for EF Core)
                if (ex is DbUpdateException dbEx && dbEx.Entries.Any())
                {
                    foreach (var entry in dbEx.Entries)
                    {
                        Console.WriteLine($"Entity of type {entry.Entity.GetType().Name} in state {entry.State} caused the error.");
                    }
                }

                return StatusCode(500, $"Internal Server Error: {ex.Message} | Inner: {inner}");
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _userRepository.GetUserByEmail(loginUserDto.Email);
                if (!response.IsSuccess)
                {
                    return Unauthorized(response.Message);
                }

                var user = (UserModel)response.Data;

                // Verify the password
                bool verified = BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.password);
                if (!verified)
                {
                    return Unauthorized("Invalid Password.");
                }

                // Create response object
                var userResponseDto = new LoginResponseDTO
                {
                    Id = user.Id,
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    Role = user.role
                };

                // If user is an employee, fetch access control
                if (user.role == "employee" && user.Employee != null)
                {
                    userResponseDto.EmployeeDetails = new EmployeeDetailsDTO
                    {
                        AccessControl = user.Employee.AccessControl // Get from the JSON-mapped field
                    };
                }

                return Ok(new ResponseBase(true, "Login successful.", userResponseDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An internal server error occurred."));
            }
        }


        [HttpGet("export/customers")]
        public async Task<IActionResult> ExportCustomers()
        {
            try
            {
                var user = await _userRepository.GetAllUsersIncludingRelatedEntitie();

                var csvBuilder = new StringBuilder();
                var headers = "Customer Id, First Name, Last Name, Email, Accepts Email Marketing, Default Company, Default Address1," +
                    "Default Address2, Default City, Default Province Code, Default Country Code, Default Zip, Default Phone, Phone," +
                    "Total Spent, Total Orders, Note, Tags, Birthday";

                csvBuilder.AppendLine(headers);

                    foreach (var users in user)
                    {
                    if (users.role == "customer")
                    {
                        foreach (var address in users.Customer.Addresses)
                        {
                            var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                            users.Id,
                            users.first_name,
                            users.last_name,
                            users.email,
                            users.Customer.EmailSmsOptIn,
                            users.Customer.Company,
                            address.Address1,
                            address.Address2,
                            address.City,
                            address.ProvinceCode,
                            address.CountryCode,
                            address.Zip,
                            address.Phone,
                            users.phone_number,
                            users.Customer.TotalSpent,
                            users.Customer.OrdersCount,
                            users.Customer.note,
                            users.Customer.tags,
                            users.Birthday
                            );
                            csvBuilder.AppendLine(newLine);
                        }
                      }
                    }
                

                var byteArray = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var stream = new MemoryStream(byteArray);

                return File(stream, "text/csv", "customer_export.csv");
            } 
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("Failed to export the customers: " + ex.Message));
            }
        }

        [HttpGet("all-customers")]
        public async Task<IActionResult> GetAllUsers(
    int page = 1,                  // Current page for pagination
    int pageSize = 50,             // Number of users per page
    string sortBy = "Last Update", // Default sort column
    string sortDirection = "asc",  // Default sort direction
    string search = ""             // Search term
)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest("Page and pageSize must be greater than 0.");
                }

                // Fetch users from the repository
                var users = await _userRepository.GetAllUsersIncludingRelatedEntities(
                    page, pageSize, sortBy, sortDirection, search);

                if (users == null || !users.Any())
                {
                    return Ok(new List<object>());
                }

                // Transform users to DTOs
                var usersDto = users.Select(user =>
                {
                    // Safely access the customer's addresses - handle null customer
                    List<object> defaultAddressList;
                    List<object> addressesList;

                    if (user.Customer?.Addresses != null)
                    {
                        defaultAddressList = user.Customer.Addresses
                            .Where(a => a.Default == true)
                            .Select(defaultAddress => new
                            {
                                defaultAddress.Id,
                                customerId = defaultAddress.Customer?.Id ?? 0,
                                defaultAddress.first_name,
                                defaultAddress.last_name,
                                defaultAddress.Company,
                                address1 = defaultAddress.Address1,
                                address2 = defaultAddress.Address2,
                                defaultAddress.Country,
                                defaultAddress.City,
                                defaultAddress.Province,
                                defaultAddress.Zip,
                                defaultAddress.Phone,
                                defaultAddress.Name,
                                defaultAddress.ProvinceCode,
                                defaultAddress.CountryCode,
                                defaultAddress.CountryName,
                                defaultAddress.Default
                            })
                            .Cast<object>()
                            .ToList();

                        addressesList = user.Customer.Addresses
                            .Select(address => new
                            {
                                address.Id,
                                customerId = address.Customer?.Id ?? 0,
                                address.first_name,
                                address.last_name,
                                address.Company,
                                address1 = address.Address1,
                                address2 = address.Address2,
                                address.Country,
                                address.City,
                                address.Province,
                                address.Zip,
                                address.Phone,
                                address.Name,
                                address.ProvinceCode,
                                address.CountryCode,
                                address.CountryName,
                                address.Default
                            })
                            .Cast<object>()
                            .ToList();
                    }
                    else
                    {
                        defaultAddressList = new List<object>();
                        addressesList = new List<object>();
                    }

                    // Always return customer data, using User data when Customer record doesn't exist
                    var customerDto = new
                    {
                        id = user.Customer?.Id ?? user.Id,
                        email = user.email ?? "",
                        createdAt = user.Customer?.CreatedAt ?? DateTime.Now,
                        updatedAt = user.Customer?.UpdatedAt ?? DateTime.Now,
                        first_name = user.first_name ?? "",
                        last_name = user.last_name ?? "",
                        orders_count = user.Customer?.OrdersCount ?? 0,
                        state = user.Customer?.state ?? false,
                        total_spent = user.Customer?.TotalSpent ?? 0.0,
                        last_order_id = user.Customer?.LastOrderId ?? 0,
                        note = user.Customer?.note ?? "",
                        tags = user.Customer?.tags ?? "",
                        last_order_name = user.Customer?.LastOrderName ?? "",
                        currency = user.Customer?.Currency ?? "",
                        phone = user.phone_number ?? "",
                        Addresses = addressesList,
                        email_marketing_consent = user.Customer?.EmailSmsOptIn ?? false,
                        sms_marketing_consent = user.Customer?.EmailSmsOptIn ?? false,
                        Default_address = defaultAddressList
                    };

                    return new
                    {
                        Id = user.Id,
                        Role = user.role,
                        Customer = customerDto
                    };
                }).ToList();

                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                // For development, consider returning the full exception message:
                // return StatusCode(500, ex.ToString());

                // For production, you might log it:
                // _logger.LogError(ex, "Error retrieving customers.");

                return StatusCode(500, ex.Message);
            }
        }


        //[HttpGet("customer/{customerId}/addresses")]
        //public async Task<IActionResult> GetAddressesByCustomerId(int customerId)
        //{
        //    try
        //    {
        //        var addresses = await _userRepository.GetAddressesByCustomerIdAsync(customerId);

        //        if (addresses == null || !addresses.Any())
        //        {
        //            return NotFound("No addresses found for the given customer.");
        //        }

        //        return Ok(addresses);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception details (use a logger to capture the error)
        //        return StatusCode(500, "An error occurred while retrieving the addresses.");
        //    }
        //}

        [HttpGet("customer/by-id/{customerId}")]
        public async Task<IActionResult> GetCustomerByCustomerId(long customerId)
        {
            try
            {
                // Fetch the customer by ID (NOT the user ID)
                var customer = await _userRepository.GetCustomerByIdIncludingRelatedEntities(customerId);
                if (customer == null)
                {
                    return NotFound("Customer not found.");
                }

                // Build the address lists
                List<object> defaultAddressList = new List<object>();
                List<object> addressesList = new List<object>();

                if (customer.Addresses != null)
                {
                    foreach (var address in customer.Addresses)
                    {
                        // If it's the default address, add it to the defaultAddressList
                        if (address.Default == true)
                        {
                            defaultAddressList.Add(new
                            {
                                Id = address.Id,
                                customerId = address.Customer.Id,
                                first_name = address.first_name,
                                last_name = address.last_name,
                                Company = address.Company,
                                address1 = address.Address1,
                                address2 = address.Address2,
                                country = address.Country,
                                city = address.City,
                                province = address.Province,
                                phone = address.Phone,
                                name = address.Name,
                                provinceCode = address.ProvinceCode,
                                countryCode = address.CountryCode,
                                countryName = address.CountryName,
                                Default = address.Default,
                            });
                        }

                        // Add all addresses to addressesList
                        addressesList.Add(new
                        {
                            Id = address.Id,
                            customerId = address.Customer.Id,
                            first_name = address.first_name,
                            last_name = address.last_name,
                            Company = address.Company,
                            address1 = address.Address1,
                            address2 = address.Address2,
                            country = address.Country,
                            city = address.City,
                            province = address.Province,
                            phone = address.Phone,
                            name = address.Name,
                            provinceCode = address.ProvinceCode,
                            countryCode = address.CountryCode,
                            countryName = address.CountryName,
                            Default = address.Default,
                        });
                    }
                }

                // Build the final DTO for the customer
                // Note: We can also include customer.User if you want user info in the response
                var customerDto = new
                {
                    id = customer.Id,
                    // If you want to include user data (like email), make sure you Include(c => c.User) in the repository
                    email = customer.User?.email,
                    createdAt = customer.CreatedAt,
                    updatedAt = customer.UpdatedAt,
                    first_name = customer.User?.first_name,
                    last_name = customer.User?.last_name,
                    orders_count = customer.OrdersCount,
                    state = customer.state,
                    total_spent = customer.TotalSpent,
                    last_order_id = customer.LastOrderId,
                    note = customer.note,
                    tags = customer.tags,
                    last_order_name = customer.LastOrderName,
                    currency = customer.Currency,
                    phone = customer.User?.phone_number,
                    Addresses = addressesList,
                    email_marketing_consent = customer.EmailSmsOptIn,
                    sms_marketing_consent = customer.EmailSmsOptIn,
                    Default_address = defaultAddressList,
                };

                return Ok(customerDto);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("user/by-id/{userId}")]
        public async Task<IActionResult> GetUserById(long userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null) return NotFound("User not found.");

                // ⚠️ Use your *entity* property names here:
                var email = user.email;        // not user.email
                var firstName = user.first_name;
                var lastName = user.last_name;
                var phone = user.phone_number;

                var cust = user.Customer; // may be null
                var primaryAddress = cust?.Addresses?
                    .OrderByDescending(a => a.Default)
                    .ThenBy(a => a.Id)
                    .FirstOrDefault();

                // OPTION A: use stored count if your *entity* has it
                int? storedOrdersCount = cust?.OrdersCount;

                // OPTION B: compute if not stored (uncomment if needed)
                // int computedOrdersCount = 0;
                // if (cust != null)
                //     computedOrdersCount = await _db.Orders.CountAsync(o => o.CustomerId == cust.Id);

                var userDto = new
                {
                    id = user.Id,
                    email = email,
                    first_name = firstName,
                    last_name = lastName,
                    phone = phone,

                    total_orders = storedOrdersCount ?? 0, // or computedOrdersCount,

                    customer = cust == null ? null : new
                    {
                        company = cust.Company,
                        address = cust.Address,
                        apartment = cust.Apartment,
                        city = cust.City,
                        country = cust.Country,
                        email_sms_opt_in = cust.EmailSmsOptIn,
                        news_letter = cust.Newsletter,
                        orders_count = storedOrdersCount ?? 0, // or computedOrdersCount,
                        total_spent = cust.TotalSpent,
                        last_order_id = cust.LastOrderId,
                        last_order_name = cust.LastOrderName,
                        currency = cust.Currency,
                        tags = cust.tags,
                        note = cust.note,

                        primary_address = primaryAddress == null ? null : new
                        {
                            id = primaryAddress.Id,
                            address1 = primaryAddress.Address1,
                            address2 = primaryAddress.Address2,
                            province = primaryAddress.Province,
                            city = primaryAddress.City,
                            phone = primaryAddress.Phone,
                            name = primaryAddress.Name,
                            zip = primaryAddress.Zip,
                            province_code = primaryAddress.ProvinceCode,
                            country_code = primaryAddress.CountryCode,
                            country_name = primaryAddress.CountryName,
                            _default = primaryAddress.Default
                        },

                        addresses = cust.Addresses?.Select(a => new
                        {
                            id = a.Id,
                            address1 = a.Address1,
                            address2 = a.Address2,
                            province = a.Province,
                            city = a.City,
                            phone = a.Phone,
                            name = a.Name,
                            zip = a.Zip,
                            province_code = a.ProvinceCode,
                            country_code = a.CountryCode,
                            country_name = a.CountryName,
                            _default = a.Default
                        }).ToList()
                    }
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                // TEMP: surface the real error while you test
                return Problem(detail: ex.ToString(), statusCode: 500);
            }
        }



        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomer(string? email = null, string? phoneNumber = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber))
                {
                    return BadRequest("Either email or phone number is required.");
                }

                UserModel user = null;

                if (!string.IsNullOrEmpty(email))
                {
                    user = await _userRepository.GetUserByEmailIncludingRelatedEntities(email);
                }
                else if (!string.IsNullOrEmpty(phoneNumber))
                {
                    user = await _userRepository.GetUserByPhoneNumberIncludingRelatedEntities(phoneNumber);
                }

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                List<object> defaultAddressList = new List<object>();
                if (user.Customer != null && user.Customer.Addresses != null)
                {
                    foreach (var defaultAddress in user.Customer.Addresses)
                    {
                        if (defaultAddress.Default == true)
                        {
                            defaultAddressList.Add(new
                            {
                                Id = defaultAddress.Id,
                                customerId = defaultAddress.Customer.Id,
                                first_name = defaultAddress.first_name,
                                last_name = defaultAddress.last_name,
                                Company = defaultAddress.Company,
                                address1 = defaultAddress.Address1,
                                address2 = defaultAddress.Address2,
                                country = defaultAddress.Country,
                                city = defaultAddress.City,
                                province = defaultAddress.Province,
                                zip = defaultAddress.Zip,
                                phone = defaultAddress.Phone,
                                name = defaultAddress.Name,
                                provinceCode = defaultAddress.ProvinceCode,
                                countryCode = defaultAddress.CountryCode,
                                countryName = defaultAddress.CountryName,
                                Default = defaultAddress.Default,
                            });
                        }
                    }
                }

                List<object> addressesList = new List<object>();
                if (user.Customer != null && user.Customer.Addresses != null)
                {
                    foreach (var address in user.Customer.Addresses)
                    {
                        addressesList.Add(new
                        {
                            Id = address.Id,
                            customerId = address.Customer.Id,
                            first_name = address.first_name,
                            last_name = address.last_name,
                            Company = address.Company,
                            address1 = address.Address1,
                            address2 = address.Address2,
                            country = address.Country,
                            city = address.City,
                            province = address.Province,
                            zip = address.Zip,
                            phone = address.Phone,
                            name = address.Name,
                            provinceCode = address.ProvinceCode,
                            countryCode = address.CountryCode,
                            countryName = address.CountryName,
                            Default = address.Default,
                        });
                    }
                }

                var customerDto = user.Customer != null ? new
                {
                    id = user.Customer.Id,
                    email = user.email,
                    createdAt = user.Customer.CreatedAt,
                    updatedAt = user.Customer.UpdatedAt,
                    first_name = user.first_name,
                    last_name = user.last_name,
                    orders_count = user.Customer.OrdersCount,
                    state = user.Customer.state,
                    total_spent = user.Customer.TotalSpent,
                    last_order_id = user.Customer.LastOrderId,
                    note = user.Customer.note,
                    tags = user.Customer.tags,
                    last_order_name = user.Customer.LastOrderName,
                    currency = user.Customer.Currency,
                    phone = user.phone_number,
                    Addresses = addressesList,
                    email_marketing_consent = user.Customer.EmailSmsOptIn,
                    sms_marketing_consent = user.Customer.EmailSmsOptIn,
                    Default_address = defaultAddressList,
                } : null;

                var userDto = new
                {
                    Id = user.Id,
                    Role = user.role,
                    Customer = customerDto,
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpGet("customer/count")]
        public async Task<IActionResult> GetCustomerCount()
        {
            try
            {
                var count = await _userRepository.GetCustomerCount();
                return Ok(new { customerCount = count });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while retrieving the customer count.");
            }
        }

        //[HttpPut("update")]
        //public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateUserDto)
        //{
        //    try 
        //    { 
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var response = await _userRepository.GetUsersByIdIncludingRelatedEntities(updateUserDto.Email);
        //    if (!response.IsSuccess)
        //    {
        //        return response.Message == "User not found." ? NotFound(response) : StatusCode(500, response);
        //    }

        //    var user = (UserModel)response.Data;

        //    // Update user fields
        //    user.first_name = updateUserDto.FirstName;
        //    user.last_name = updateUserDto.LastName;
        //    user.email = updateUserDto.Email; // Consider checking if the email is already in use by another user
        //    user.phone_number = updateUserDto.PhoneNumber;
        //    user.Birthday = updateUserDto.Birthday;

        //    // Update password if provided and not empty
        //    //if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
        //    //{
        //    //    user.password = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
        //    //}

        //    // Update customer fields and addresses
        //    var customer = user.Customer;
        //    if (customer != null && updateUserDto.Customer != null)
        //    {
        //        customer.Company = updateUserDto.Customer.Company;
        //        customer.Address = updateUserDto.Customer.Address;
        //        customer.Apartment = updateUserDto.Customer.Apartment;
        //        customer.City = updateUserDto.Customer.City;
        //        customer.Country = updateUserDto.Customer.Country;
        //        customer.EmailSmsOptIn = updateUserDto.Customer.EmailSmsOptIn;
        //        customer.Newsletter = updateUserDto.Customer.Newsletter;
        //        customer.OrdersCount = (int)updateUserDto.Customer.OrdersCount;
        //        customer.state = (bool)updateUserDto.Customer.state;
        //        customer.TotalSpent = (double)updateUserDto.Customer.TotalSpent;
        //        customer.LastOrderId = (int)updateUserDto.Customer.LastOrderId;
        //        customer.note = updateUserDto.Customer.note;
        //        customer.tags = updateUserDto.Customer.tags;
        //        customer.LastOrderName = updateUserDto.Customer.LastOrderName;
        //        customer.Currency = updateUserDto.Customer.Currency;

        //        // Update the UpdatedAt field for the customer
        //        customer.UpdatedAt = DateTime.Now;

        //        // Update addresses based on the existing relationship
        //        foreach (var addressDto in updateUserDto.Customer.Addresses)
        //        {
        //            var existingAddress = customer.Addresses.FirstOrDefault(address => updateUserDto.Email == updateUserDto.Email);
        //            if (existingAddress != null)
        //            {
        //                existingAddress.Address1 = addressDto.Address1;
        //                existingAddress.Address2 = addressDto.Address2;
        //                existingAddress.Province = addressDto.Province;
        //                existingAddress.City = addressDto.City;
        //                existingAddress.Phone = addressDto.Phone;
        //                existingAddress.Name = $"{user.first_name} {user.last_name}";
        //                existingAddress.Zip = addressDto.Zip;
        //                existingAddress.ProvinceCode = addressDto.ProvinceCode;
        //                existingAddress.CountryCode = addressDto.CountryCode;
        //                existingAddress.CountryName = addressDto.CountryName;
        //            }
        //            else
        //            {
        //                var newAddress = new AddressesModel
        //                {
        //                    CustomerId = customer.Id,
        //                    Address1 = addressDto.Address1,
        //                    Address2 = addressDto.Address2,
        //                    Province = addressDto.Province,
        //                    City = addressDto.City,
        //                    Phone = addressDto.Phone,
        //                    Name = $"{user.first_name} {user.last_name}",
        //                    Zip = addressDto.Zip,
        //                    ProvinceCode = addressDto.ProvinceCode,
        //                    CountryCode = addressDto.CountryCode
        //                };
        //                customer.Addresses.Add(newAddress);
        //            }
        //        }
        //    }

        //    // Save changes to the database
        //    await _userRepository.SaveChangesAsync();

        //    return Ok(ResponseBase.Success("User updated successfully.", updateUserDto));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception here
        //        return StatusCode(500, "An error occurred while retrieving the customer count.");
        //    }
        //}

        [HttpPut("update/customer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateUserDTO updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _userRepository.GetUsersByIdIncludingRelatedEntities(updateUserDto.Email);
                if (!response.IsSuccess)
                {
                    return response.Message == "User not found." ? NotFound(response) : StatusCode(500, response);
                }
                var user = (UserModel)response.Data;

                // Update user fields
                user.first_name = updateUserDto.FirstName;
                user.last_name = updateUserDto.LastName;
                user.email = updateUserDto.Email; // Consider checking if the email is already in use by another user
                user.phone_number = updateUserDto.PhoneNumber;
                user.Birthday = updateUserDto.Birthday;

                // Update customer fields and addresses
                var customer = user.Customer;
                if (customer != null && updateUserDto.Customer != null)
                {
                    customer.Company = updateUserDto.Customer.Company;
                    customer.Address = updateUserDto.Customer.Address;
                    customer.Apartment = updateUserDto.Customer.Apartment;
                    customer.City = updateUserDto.Customer.City;
                    customer.Country = updateUserDto.Customer.Country;
                    customer.Newsletter = updateUserDto.Customer.Newsletter;

                    // Update the UpdatedAt field for the customer
                    customer.UpdatedAt = DateTime.Now;

                    // Update addresses based on the existing relationship
                    foreach (var addressDto in updateUserDto.Customer.Addresses)
                    {
                        var existingAddress = customer.Addresses.FirstOrDefault(address => updateUserDto.Email == updateUserDto.Email);
                        if (existingAddress != null)
                        {
                            existingAddress.Address1 = addressDto.Address1;
                            existingAddress.City = addressDto.City;
                        }
                        else
                        {
                            var newAddress = new AddressesModel
                            {
                                CustomerId = customer.Id,
                                Address1 = addressDto.Address1,
                                City = addressDto.City,
                            };
                            customer.Addresses.Add(newAddress);
                        }
                    }
                }

                // Save changes to the database
                await _userRepository.SaveChangesAsync();

                return Ok(ResponseBase.Success("User updated successfully.", updateUserDto));
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while retrieving the customer count.");
            }
        }

        [HttpDelete("delete/user")]

        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserDTO deleteUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userRepository.GetUserByEmailIncludingRelatedEntities(deleteUserDto.Email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var response = await _userRepository.DeleteUser(user);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                if (response.Message.Equals("User not found.", StringComparison.Ordinal))
                {
                    return NotFound(response);
                }
                else
                {
                    return StatusCode(500, response);
                }
            }
        }

        //[HttpPost("request-password-reset")]
        //public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDTO request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        // Fetch the user by email
        //        var response = await _userRepository.GetUserByEmail(request.Email);
        //        if (!response.IsSuccess)
        //        {
        //            return NotFound("User not found.");
        //        }

        //        var user = (UserModel)response.Data;

        //        // Generate a password reset token (could be JWT or a GUID)
        //        string resetToken = Guid.NewGuid().ToString();  // Or use JWT

        //        // Store the token and its expiration in the database (or cache)
        //        user.PasswordResetToken = resetToken;
        //        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
        //        await _userRepository.UpdateUser(user);

        //        // Send an email to the user with the reset token link
        //        string resetLink = Url.Action("ResetPassword", "Auth", new { token = resetToken }, Request.Scheme);  // Generates a link like https://localhost:7042/Auth/ResetPassword?token=resetToken
        //        await _userRepository.SendPasswordResetEmail(user.email, resetLink);

        //        return Ok("Password reset link has been sent to your email.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception details to get more information
        //        Console.WriteLine($"Error: {ex.Message} - StackTrace: {ex.StackTrace}");

        //        return StatusCode(500, ResponseBase.Failure("An internal server error occurred."));
        //    }
        //}

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Fetch the user by email
                var response = await _userRepository.GetUserByEmail(request.Email);
                if (!response.IsSuccess)
                {
                    return NotFound("User not found.");
                }

                var user = (UserModel)response.Data;

                // Generate a password reset token (could be JWT or a GUID)
                string resetToken = Guid.NewGuid().ToString();  // Or use JWT

                // Store the token and its expiration in the database (or cache)
                user.PasswordResetToken = resetToken;
                user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
                await _userRepository.UpdateUser(user);

                // Send the reset token directly via email for testing purposes
                string emailBody = $"Your password reset token is: {resetToken}\n\n" +
                                   "Use this token to reset your password.";

                await _userRepository.SendPasswordResetEmail(user.email, emailBody);

                return Ok("Password reset token has been sent to your email for testing purposes.");
            }
            catch (Exception ex)
            {
                // Log the exception details to get more information
                Console.WriteLine($"Error: {ex.Message} - StackTrace: {ex.StackTrace}");

                return StatusCode(500, ResponseBase.Failure("An internal server error occurred."));
            }
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDTO resetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Fetch the user by the reset token
                var response = await _userRepository.GetUserByResetToken(resetDto.Token);
                if (!response.IsSuccess)
                {
                    return BadRequest("Invalid or expired password reset token.");
                }

                var user = (UserModel)response.Data;

                // Check if the token is expired
                if (user.ResetTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest("Password reset token has expired.");
                }

                // Hash the new password
                string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);

                // Update the password and remove the reset token
                user.password = newHashedPassword;
                user.PasswordResetToken = null;
                user.ResetTokenExpiry = null;

                await _userRepository.UpdateUser(user);

                return Ok("Password has been successfully reset.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBase.Failure("An internal server error occurred."));
            }
        }

        [HttpGet("customer/sortby")]
        public async Task<IActionResult> GetAllSortBy()
        {
            try
            {
                var sortby = await _userRepository.GetAllSortingAsync();
                return Ok(sortby);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "an internal error occured while fetching the sortby: " + ex.Message);
            }
        }

        [HttpPut("update-employee-access-control")]
        public async Task<IActionResult> UpdateEmployeeAccessControl([FromBody] UpdateEmployeeAccessControlDTO updateDto)
        {
            // Validate the incoming data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Retrieve the Employee from the database based on UserId
            var employee = await _userRepository.GetEmployeeByUserId(updateDto.UserId);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            // 2. Update the AccessControl property
            employee.AccessControl = updateDto.AccessControl;

            // 3. Save changes
            var updateResponse = await _userRepository.SaveChangeAsync();
            if (!updateResponse.IsSuccess)
            {
                return BadRequest("Failed to update employee access control.");
            }

            // 4. Return success response
            return Ok(new
            {
                message = "Access control updated successfully.",
                updatedAccessControl = employee.AccessControl
            });
        }

        [HttpPost("force-change-password")]
        public async Task<IActionResult> ForceChangePassword([FromBody] ForceChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool databaseUpdated = false;
            bool cognitoUpdated = false;
            string errorMessage = "";

            try
            {
                // Step 1: Update password in database
                var response = await _userRepository.GetUserByEmail(dto.Email);
                if (!response.IsSuccess)
                {
                    return NotFound("User not found in database.");
                }

                var user = (UserModel)response.Data;

                // Hash and update the new password in database
                user.password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _userRepository.UpdateUser(user);
                databaseUpdated = true;

                Console.WriteLine($"[SUCCESS] Database password updated for: {dto.Email}");

                // Step 2: Update password in Cognito
                try
                {
                    var region = Amazon.RegionEndpoint.GetBySystemName(_configuration["AWS:Region"]);

                    // Create Cognito client with explicit credentials from appsettings
                    var accessKey = _configuration["AWS:AccessKeyId"];
                    var secretKey = _configuration["AWS:SecretAccessKey"];

                    var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
                    var cognitoClient = new AmazonCognitoIdentityProviderClient(credentials, region);

                    var userPoolId = _configuration["AWS:Cognito:UserPoolId"];

                    // Set permanent password in Cognito (admin operation)
                    var setPasswordRequest = new AdminSetUserPasswordRequest
                    {
                        UserPoolId = userPoolId,
                        Username = dto.Email,
                        Password = dto.NewPassword,
                        Permanent = true // Make it permanent, not temporary
                    };

                    await cognitoClient.AdminSetUserPasswordAsync(setPasswordRequest);
                    cognitoUpdated = true;

                    Console.WriteLine($"[SUCCESS] Cognito password updated for: {dto.Email}");
                }
                catch (UserNotFoundException)
                {
                    errorMessage = "User not found in Cognito. Database password updated successfully.";
                    Console.WriteLine($"[WARNING] User not found in Cognito: {dto.Email}");
                }
                catch (Exception cognitoEx)
                {
                    errorMessage = $"Database updated but Cognito update failed: {cognitoEx.Message}";
                    Console.WriteLine($"[ERROR] Cognito update failed: {cognitoEx.Message}");
                }

                // Return response
                return Ok(new ForceChangePasswordResponseDTO
                {
                    IsSuccess = true,
                    Message = string.IsNullOrEmpty(errorMessage)
                        ? "Password changed successfully in both database and Cognito."
                        : errorMessage,
                    DatabaseUpdated = databaseUpdated,
                    CognitoUpdated = cognitoUpdated
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Force change password failed: {ex.Message}");
                return StatusCode(500, new ForceChangePasswordResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}",
                    DatabaseUpdated = databaseUpdated,
                    CognitoUpdated = cognitoUpdated
                });
            }
        }

    }
}

