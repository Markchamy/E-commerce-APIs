using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Backend.Repositories
{
    public class UserServicesRepository : IUserServices
    {
        private readonly MyDbContext _context;
        public UserServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UserExists(string email)
        {
            return await _context.Users.AnyAsync(user => user.email == email);
        }
        public async Task<ResponseBase> AddUser(UserModel user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("User Successfuly registered");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error adding user: {ex.Message}");
            }
        }

public async Task<UserModel?> GetUserByIdAsync(long userId)
    {
        return await _context.Users
            .Include(u => u.Customer)           // reference nav
            .ThenInclude(c => c.Addresses)      // collection nav
            // AsNoTracking removed to allow tracking for updates
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<ResponseBase> AddCustomer(CustomerModel customer)
        {
            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Customer Successfuly registered");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error adding customer: {ex.Message}");
            }
        }
        public async Task<ResponseBase> AddAddresses(AddressesModel address)
        {
            try
            {
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Address added Successfuly");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error adding address: {ex.Message}");
            }
        }
        public async Task<ResponseBase> AddEmployee(EmployeeModel employee)
        {
            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Employee Successfuly registered.");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error adding employee: {ex.Message}");
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<CustomerModel?> GetCustomerByIdIncludingRelatedEntities(long customerId)
        {
            // Example using EF Core
            return await _context.Customers
                .Include(c => c.User)          // If you need user info
                .Include(c => c.Addresses)     // If you need addresses
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }


        public async Task<UserModel?> GetUserByEmailIncludingRelatedEntities(string email)
        {
            return await _context.Users
                                 .Include(user => user.Customer)
                                    .ThenInclude(customer => customer.Addresses)
                                 .Include(user => user.Employee)
                                 .FirstOrDefaultAsync(user => user.email == email);
        }
        public async Task<UserModel?> GetUserByPhoneNumberIncludingRelatedEntities(string phoneNumber)
        {
            return await _context.Users
                                 .Include(user => user.Customer)
                                    .ThenInclude(customer => customer.Addresses)
                                 .Include(user => user.Employee)
                                 .FirstOrDefaultAsync(user => user.phone_number == phoneNumber);
        }


        public async Task<ResponseBase> DeleteUser(UserModel user)
        {
            try
            {
                if (user == null)
                {
                    return ResponseBase.Failure("user not found");
                }
                if (user.Customer != null)
                {
                    var customerAddresses = await _context.Addresses
                        .Where(address => address.CustomerId == user.Customer.Id)
                        .ToListAsync();

                    _context.Addresses.RemoveRange(customerAddresses);
                    _context.Customers.Remove(user.Customer);
                }

                if (user.Employee != null)
                {
                    _context.Employees.Remove(user.Employee);
                }

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                return ResponseBase.Success($"User with email {user.email} has been deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return ResponseBase.Failure("This user has already been deleted or does not exist");
            }
            catch (DbUpdateException)
            {
                return ResponseBase.Failure("A database problem occured while deleting the user.");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error occurred while deleting user: {ex.Message}");
            }
        }

        public async Task DeleteCustomer(CustomerModel customer)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAddresses(AddressesModel addresses)
        {
            _context.Addresses.Remove(addresses);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteEmployee(EmployeeModel employee)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<ResponseBase> GetUserByEmail(string email)
        {
            var user = await _context.Users
                .Include(u => u.Employee) // Ensure Employee data is loaded
                .FirstOrDefaultAsync(u => u.email == email);

            if (user == null)
            {
                return ResponseBase.Failure("User not found.", null); // ✅ Now includes null data explicitly
            }

            return ResponseBase.Success("User found.", user); // ✅ Corrected to pass user
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersIncludingRelatedEntities(
      int page,
      int pageSize,
      string sortBy,
      string sortDirection,
      string search
  )
        {
            try
            {
                var offset = (page - 1) * pageSize;

                // ✅ If search is used — run LIKE-based search query
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string likeSearch = $"%{search.Trim()}%";

                    var users = await _context.Users
                        .Where(u => u.role == "customer" &&
                            (
                                EF.Functions.Like(u.first_name, likeSearch) ||
                                EF.Functions.Like(u.last_name, likeSearch) ||
                                EF.Functions.Like(u.email, likeSearch) ||
                                EF.Functions.Like(u.phone_number, likeSearch)
                            ))
                        .Include(u => u.Customer)
                            .ThenInclude(c => c.Addresses)
                        .AsSplitQuery()
                        .AsNoTracking()
                        .OrderByDescending(u => u.Id)
                        .Skip(offset)
                        .Take(pageSize)
                        .ToListAsync();

                    return users;
                }

                // ✅ No search: build EF query with sorting
                IQueryable<UserModel> query = _context.Users
                    .Where(u => u.role == "customer")
                    .Include(u => u.Customer)
                        .ThenInclude(c => c.Addresses)
                    .AsSplitQuery()
                    .AsNoTracking();

                switch (sortBy?.ToLower())
                {
                    case "last update":
                        query = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(user => user.Customer.UpdatedAt)
                            : query.OrderBy(user => user.Customer.UpdatedAt);
                        break;
                    case "amount spent":
                        query = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(user => user.Customer.TotalSpent)
                            : query.OrderBy(user => user.Customer.TotalSpent);
                        break;
                    case "total orders":
                        query = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(user => user.Customer.OrdersCount)
                            : query.OrderBy(user => user.Customer.OrdersCount);
                        break;
                    case "last order date":
                        query = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(user => user.Customer.LastOrderName)
                            : query.OrderBy(user => user.Customer.LastOrderName);
                        break;
                    case "first order date":
                        query = sortDirection.ToLower() == "asc"
                            ? query.OrderByDescending(user => user.Customer.LastOrderName)
                            : query.OrderBy(user => user.Customer.LastOrderName);
                        break;
                    case "date added as customer":
                        query = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(user => user.Customer.CreatedAt)
                            : query.OrderBy(user => user.Customer.CreatedAt);
                        break;
                    default:
                        query = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(user => user.Id)
                            : query.OrderBy(user => user.Id);
                        break;
                }

                var result = await query
                    .Skip(offset)
                    .Take(pageSize)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 ERROR in GetAllUsersIncludingRelatedEntities:");
                Console.WriteLine(ex.ToString());
                return new List<UserModel>();
            }
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersIncludingRelatedEntitie()
        {
            return await _context.Users
                                   .Where(user => user.role == "customer")
                                 .Include(user => user.Customer)
                                    .ThenInclude(customer => customer.Addresses)
                                 .ToListAsync();
        }

        public async Task<ResponseBase> GetUsersByIdIncludingRelatedEntities(string email)
        {
            try
            {
                var user = await _context.Users
                    .Include(user => user.Customer)
                        .ThenInclude(customer => customer.Addresses)
                    .FirstOrDefaultAsync(user => user.email == email);

                if (user == null)
                {
                    return ResponseBase.Failure("User not found.");
                }

                return ResponseBase.Success("User retrieved successfully.", user);
            }
            catch (Exception ex)
            {
                // Log the exception here if necessary
                return ResponseBase.Failure($"An error occurred while retrieving the user: {ex.Message}");
            }
        }

        public async Task<int> GetCustomerCount()
        {
            return await _context.Users.CountAsync(user => user.role == "customer");
        }

        public async Task<long> GetNextUserIdAsync()
        {
            var latestUser = await _context.Users
                .OrderByDescending(u => u.Id)
                .FirstOrDefaultAsync();

            if (latestUser == null)
            {
                return 1; // Starting ID if no users exist
            }

            return latestUser.Id + 1;
        }

        public async Task<long> GetNextCustomerIdAsync()
        {
            var latestCustomer = await _context.Customers
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (latestCustomer == null)
            {
                return 1; // Starting ID if no customers exist
            }

            return latestCustomer.Id + 1;
        }

        public async Task<long> GetNextAddressIdAsync()
        {
            var latestAddress = await _context.Addresses
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();

            if (latestAddress == null)
            {
                return 1; // Starting ID if no addresses exist
            }

            return latestAddress.Id + 1;
        }

        public async Task<IEnumerable<UserModel>> GetAllCustomer()
        {
            return await _context.Users
                                 .Include(user => user.Customer)
                                 .ToListAsync();
        }

        public async Task<ResponseBase> UpdateUser(UserModel user)
        {
            try
            {
                // Code to update user in the database
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("User updated successfully.");
            }
            catch (Exception)
            {
                // Handle exception and return failure response
                return ResponseBase.Failure("Failed to update the user.");
            }
        }
        public async Task<ResponseBase> GetUserByResetToken(string token)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(user => user.PasswordResetToken == token);
                if (user == null)
                {
                    return ResponseBase.Failure("User not found or invalid token.");
                }

                return ResponseBase.Success("User found.", user);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"An error occurred while retrieving the user: {ex.Message}");
            }
        }

        public async Task SendPasswordResetEmail(string email, string resetLink)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("qa.akikis.cigars@gmail.com", "Password Reset"); // Specify a valid 'From' email
                message.To.Add(new MailAddress(email));
                message.Subject = "Password Reset Request";
                message.Body = $"Click the link to reset your password: {resetLink}";
                message.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587)) // Replace with actual SMTP server and port
                {
                    smtpClient.Credentials = new NetworkCredential("qa.akikis.cigars@gmail.com", "zbxm trgz ndxw cwxz"); // Replace with actual credentials
                    smtpClient.EnableSsl = true;

                    await smtpClient.SendMailAsync(message);
                }

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                // Log detailed exception information
                Console.WriteLine($"Failed to send email: {ex.Message}, Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task SendWelcomeEmail(string email, string firstName)
        {
            try
            {
                const string shopName = "Akikis Cigars";
                const string shopUrl = "https://akikiscigars.com";
                const string shopEmail = "qa.akikis.cigars@gmail.com";
                const string accentColor = "#1a1a1a";

                var emailTitle = $"Welcome to {shopName}!";
                var emailBody = "You've activated your customer account. Next time you shop with us, log in for faster checkout.";

                var htmlBody = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <title>{emailTitle}</title>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
  <meta name=""viewport"" content=""width=device-width"">
  <style>
    body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
    .body {{ width: 100%; background-color: #f4f4f4; }}
    .header {{ background-color: #1a1a1a; padding: 20px 0; text-align: center; }}
    .shop-name__text a {{ color: #ffffff; text-decoration: none; font-size: 24px; font-weight: bold; }}
    .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
    .content__cell {{ padding: 40px 30px; }}
    h2 {{ color: #1a1a1a; }}
    p {{ color: #555555; line-height: 1.6; }}
    .button__cell {{ background: {accentColor}; border-radius: 4px; text-align: center; padding: 12px 24px; display: inline-block; }}
    .button__text {{ color: #ffffff; text-decoration: none; font-weight: bold; font-size: 15px; }}
    .footer__cell {{ padding: 20px 30px; border-top: 1px solid #eeeeee; text-align: center; }}
    .disclaimer__subtext {{ color: #aaaaaa; font-size: 13px; }}
    .disclaimer__subtext a {{ color: {accentColor}; }}
  </style>
</head>
<body>
  <table class=""body"" width=""100%"">
    <tr>
      <td>
        <table class=""header"" width=""100%"">
          <tr>
            <td style=""text-align:center; padding: 20px;"">
              <h1 class=""shop-name__text""><a href=""{shopUrl}"">{shopName}</a></h1>
            </td>
          </tr>
        </table>

        <table width=""100%"">
          <tr>
            <td style=""text-align:center;"">
              <table class=""container"" width=""600"" align=""center"">
                <tr>
                  <td class=""content__cell"">
                    <h2>{emailTitle}</h2>
                    <p>Hi {firstName},</p>
                    <p>{emailBody}</p>
                    <br/>
                    <table>
                      <tr>
                        <td class=""button__cell"">
                          <a href=""{shopUrl}"" class=""button__text"">Visit our store</a>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
                <tr>
                  <td class=""footer__cell"">
                    <p class=""disclaimer__subtext"">If you have any questions, reply to this email or contact us at <a href=""mailto:{shopEmail}"">{shopEmail}</a></p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

                var message = new MailMessage();
                message.From = new MailAddress("qa.akikis.cigars@gmail.com", shopName);
                message.To.Add(new MailAddress(email));
                message.Subject = emailTitle;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("qa.akikis.cigars@gmail.com", "zbxm trgz ndxw cwxz");
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(message);
                }

                Console.WriteLine("Welcome email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send welcome email: {ex.Message}, Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<CustomerSortByModel>> GetAllSortingAsync()
        {
            return await _context.customer_sort_by.ToListAsync();
        }
        public async Task<List<EmployeeModel>> GetAllEmployees()
        {
            // Fetch employees along with their associated User data
            return await _context.Employees
                .Include(e => e.User) // Include User data
                .ToListAsync();
        }

        public async Task<EmployeeModel?> GetEmployeeByUserId(long userId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // General SaveChanges
        public async Task<(bool IsSuccess, string Message)> SaveChangeAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return (true, "Success");
            }
            catch (Exception ex)
            {
                // Log error
                return (false, ex.Message);
            }
        }

        // Builds:  p != null && p.ToLower().Contains(@0)
        private string BuildGlobalStringPredicate(Type modelType)
        {
            var stringProperties = modelType.GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => $"{p.Name} != null && {p.Name}.ToLower().Contains(@0)");

            return string.Join(" || ", stringProperties);
        }
        private string BuildPrefixedPredicate(Type modelType, string prefix)
        {
            var stringProps = modelType.GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => $"{prefix}{p.Name} != null && {prefix}{p.Name}.ToLower().Contains(@0)");

            return string.Join(" || ", stringProps);
        }


    }
}
