using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Backend.Repositories
{
    public class OrdersServicesRepository : IOrdersServices
    {
        private readonly MyDbContext _context;
        public OrdersServicesRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<bool> OrderExist(long orderid)
        {
            return await _context.Orders.AnyAsync(orders => orders.orderid == orderid);
        }
        public async Task<ResponseBase> CreateOrder(OrdersModel orders)
        {
            try
            {
                _context.Orders.Add(orders);
                await _context.SaveChangesAsync();

                return ResponseBase.Success("Orders have been created successfully.", orders);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure("Error creating the order: " + ex.Message);
            }
        }
        public async Task<int> GetOrdersCount()
        {
            return await _context.Orders.CountAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrder(OrdersModel order)
        {
            _context.Set<OrdersModel>().Remove(order);
        }

        public async Task<OrdersModel> GetLatestOrderAsync()
        {
            return await _context.Orders
                .OrderByDescending(o => o.created_at) // Assuming CreatedAt is the timestamp
                .FirstOrDefaultAsync();
        }

        public async Task<List<OrdersModel>> GetAllCancelledOrders()
        {
            return await _context.Set<OrdersModel>()
                                 .Where(order => order.order_status == "Cancelled")
                                 .Include(orders => orders.client_details)
                                 .Include(orders => orders.subtotal_price_set)
                                 .Include(orders => orders.TotalDiscount)
                                 .Include(orders => orders.CurrentTotalPrice)
                                 .Include(orders => orders.TotalTax)
                                 .Include(orders => orders.discount_code)
                                 .Include(orders => orders.note_attributes)
                                 .Include(orders => orders.priceSet)
                                 .Include(orders => orders.taxLines)
                                 .Include(orders => orders.LineModels)
                                 .Include(orders => orders.totalShipping)
                                 .Include(orders => orders.billing_address)
                                 .Include(orders => orders.discount_applications)
                                 .Include(orders => orders.fulfillment)
                                 .Include(orders => orders.LineItems)
                                 .Include(orders => orders.ShippingAddress)
                                 .Include(orders => orders.ShippingLines)
                                 .ToListAsync();
        }

        public async Task<List<OrdersModel>> GetAllOrders(
            int page = 1,
            int pageSize = 50,
            string sortBy = "date",
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        )
        {
            // Validate pagination parameters
            if (page < 1 || pageSize < 1)
            {
                throw new ArgumentException("Page and pageSize must be greater than 0");
            }

            // Only include what the list view actually displays — omitting the 14 relations
            // that are only needed on the order detail page cuts query time dramatically.
            IQueryable<OrdersModel> query = _context.Set<OrdersModel>()
                .Include(o => o.billing_address)
                .Include(o => o.fulfillment)
                .Include(o => o.LineItems);

            // 1. Filter logic
            if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
            {
                switch (filter.ToLower())
                {
                    case "unfulfilled":
                        query = query.Where(o => o.fulfillment_status == "unfulfilled");
                        break;
                    case "unpaid":
                        query = query.Where(o => o.financial_status == "pending");
                        break;
                    case "open":
                        query = query.Where(o => o.cancelled_at == null && o.closed_at == null);
                        break;
                    case "archived":
                        query = query.Where(o => o.order_status == "archived");
                        break;
                    case "fleetrunnr":
                        query = query.Where(o => o.tags == "FleetRunner" || o.tags == "FleetRunnr");
                        break;
                    case "aramex":
                        query = query.Where(o => o.tags == "Aramex");
                        break;
                    case "fr dropped-off":
                        query = query.Where(o => o.tags == "FR Dropped-Off");
                        break;
                    case "pickup":
                        query = query.Where(o => o.tags == "Pickup");
                        break;
                    default:
                        query = query.Where(_ => false);
                        break;
                }
            }

            // 2. Search logic
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                // Parse once so EF can use a direct integer equality check (index-friendly)
                // instead of ToString().Contains() which forces a full table scan.
                long? searchId = long.TryParse(searchTerm, out var parsed) ? parsed : null;
                query = query.Where(o =>
                    (searchId.HasValue && o.orderid == searchId.Value) ||
                    (o.name != null && o.name.Contains(searchTerm)) ||
                    (o.email != null && o.email.Contains(searchTerm)) ||
                    (o.contact_email != null && o.contact_email.Contains(searchTerm)) ||
                    (o.phone != null && o.phone.Contains(searchTerm)) ||
                    (o.tags != null && o.tags.Contains(searchTerm)) ||
                    o.LineItems.Any(li =>
                        (li.name != null && li.name.Contains(searchTerm)) ||
                        (li.sku != null && li.sku.Contains(searchTerm)) ||
                        (li.title != null && li.title.Contains(searchTerm))
                    )
                );
            }


            // 3. Sorting logic
            switch (sortBy?.ToLower())
            {
                case "order number":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.order_number)
                        : query.OrderBy(o => o.order_number);
                    break;
                case "date":
                default:
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.created_at ?? DateTime.MinValue)
                        : query.OrderBy(o => o.created_at ?? DateTime.MinValue);
                    break;
                case "items":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.LineItems.Count())
                        : query.OrderBy(o => o.LineItems.Count());
                    break;
                case "destination":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.ShippingAddress.FirstOrDefault().address1)
                        : query.OrderBy(o => o.ShippingAddress.FirstOrDefault().address1);
                    break;
                case "customer name":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.Customer.first_name)
                        : query.OrderBy(o => o.Customer.first_name);
                    break;
                case "payment status":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.financial_status)
                        : query.OrderBy(o => o.financial_status);
                    break;
                case "total":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.total_price)
                        : query.OrderBy(o => o.total_price);
                    break;
            }

            // 4. Pagination logic
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            // 5. Execute query
            return await query.ToListAsync();
        }

        public async Task<List<OrdersModel>> GetPMIOrders(
            int page = 1,
            int pageSize = 50,
            string sortBy = "date",
            string sortDirection = "desc",
            string filter = "All",
            string search = ""
        )
        {
            // Validate pagination parameters
            if (page < 1 || pageSize < 1)
            {
                throw new ArgumentException("Page and pageSize must be greater than 0");
            }

            // Start with base query including all necessary navigation properties
            IQueryable<OrdersModel> query = _context.Set<OrdersModel>()
                .Include(o => o.client_details)
                .Include(o => o.subtotal_price_set)
                .Include(o => o.TotalDiscount)
                .Include(o => o.CurrentTotalPrice)
                .Include(o => o.TotalTax)
                .Include(o => o.discount_code)
                .Include(o => o.note_attributes)
                .Include(o => o.priceSet)
                .Include(o => o.taxLines)
                .Include(o => o.LineModels)
                .Include(o => o.totalShipping)
                .Include(o => o.billing_address)
                .Include(o => o.discount_applications)
                .Include(o => o.fulfillment)
                .Include(o => o.LineItems)
                .Include(o => o.ShippingAddress)
                .Include(o => o.ShippingLines);

            // PMI SPECIFIC FILTER 1: Order must be fulfilled OR have tag "do"
            query = query.Where(o =>
                o.fulfillment_status.ToLower() == "fulfilled" ||
                (o.tags != null && o.tags.ToLower().Contains("do"))
            );

            // PMI SPECIFIC FILTER 2: Order must contain PMI products
            // Products with vendor or title containing: heets, fiit, iqos, terea, blends, delia, lil
            query = query.Where(o =>
                o.LineItems.Any(li =>
                    (li.vendor != null && (
                        li.vendor.ToLower().Contains("heets") ||
                        li.vendor.ToLower().Contains("fiit") ||
                        li.vendor.ToLower().Contains("iqos") ||
                        li.vendor.ToLower().Contains("terea") ||
                        li.vendor.ToLower().Contains("blends") ||
                        li.vendor.ToLower().Contains("delia") ||
                        li.vendor.ToLower().Contains("lil")
                    )) ||
                    (li.title != null && (
                        li.title.ToLower().Contains("heets") ||
                        li.title.ToLower().Contains("fiit") ||
                        li.title.ToLower().Contains("iqos") ||
                        li.title.ToLower().Contains("terea") ||
                        li.title.ToLower().Contains("blends") ||
                        li.title.ToLower().Contains("delia") ||
                        li.title.ToLower().Contains("lil")
                    )) ||
                    (li.name != null && (
                        li.name.ToLower().Contains("heets") ||
                        li.name.ToLower().Contains("fiit") ||
                        li.name.ToLower().Contains("iqos") ||
                        li.name.ToLower().Contains("terea") ||
                        li.name.ToLower().Contains("blends") ||
                        li.name.ToLower().Contains("delia") ||
                        li.name.ToLower().Contains("lil")
                    ))
                )
            );

            // 1. Additional filter logic (same as GetAllOrders)
            if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
            {
                switch (filter.ToLower())
                {
                    case "unfulfilled":
                        query = query.Where(o => o.fulfillment_status == "unfulfilled");
                        break;
                    case "unpaid":
                        query = query.Where(o => o.financial_status == "pending");
                        break;
                    case "fulfilled":
                        query = query.Where(o => o.fulfillment_status == "fulfilled");
                        break;
                    case "paid":
                        query = query.Where(o => o.financial_status == "paid");
                        break;
                    default:
                        break;
                }
            }

            // 2. Search logic
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                query = query.Where(o =>
                    o.orderid.ToString().Contains(searchTerm) ||
                    (o.name != null && o.name.Contains(searchTerm)) ||
                    (o.email != null && o.email.Contains(searchTerm)) ||
                    (o.order_status != null && o.order_status.Contains(searchTerm)) ||
                    (o.financial_status != null && o.financial_status.Contains(searchTerm)) ||
                    (o.fulfillment_status != null && o.fulfillment_status.Contains(searchTerm)) ||
                    (o.phone != null && o.phone.Contains(searchTerm)) ||
                    o.LineItems.Any(li =>
                        (li.name != null && li.name.Contains(searchTerm)) ||
                        (li.sku != null && li.sku.Contains(searchTerm)) ||
                        (li.title != null && li.title.Contains(searchTerm))
                    )
                );
            }

            // 3. Sorting logic
            switch (sortBy?.ToLower())
            {
                case "order number":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.order_number)
                        : query.OrderBy(o => o.order_number);
                    break;
                case "date":
                default:
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.created_at ?? DateTime.MinValue)
                        : query.OrderBy(o => o.created_at ?? DateTime.MinValue);
                    break;
                case "customer name":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.Customer.first_name)
                        : query.OrderBy(o => o.Customer.first_name);
                    break;
                case "total":
                    query = sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(o => o.total_price)
                        : query.OrderBy(o => o.total_price);
                    break;
            }

            // 4. Pagination logic
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            // 5. Execute query
            return await query.ToListAsync();
        }

        public async Task<List<OrdersModel>> ExportOrders()
        {
            return await _context.Set<OrdersModel>()
                                 .Include(orders => orders.client_details)
                                 .Include(orders => orders.subtotal_price_set)
                                 .Include(orders => orders.TotalDiscount)
                                 .Include(orders => orders.CurrentTotalPrice)
                                 .Include(orders => orders.TotalTax)
                                 .Include(orders => orders.discount_code)
                                 .Include(orders => orders.note_attributes)
                                 .Include(orders => orders.priceSet)
                                 .Include(orders => orders.taxLines)
                                 .Include(orders => orders.LineModels)
                                 .Include(orders => orders.totalShipping)
                                 .Include(orders => orders.billing_address)
                                 .Include(orders => orders.discount_applications)
                                 .Include(orders => orders.fulfillment)
                                 .Include(orders => orders.LineItems)
                                 .Include(orders => orders.ShippingAddress)
                                 .ToListAsync();
        }

        public async Task<OrdersModel> GetOrderByIdWithDetails(long orderid)
        {
            return await _context.Orders
                                 .Where(order => order.orderid == orderid)
                                 .Include(orders => orders.client_details)
                                 .Include(orders => orders.subtotal_price_set)
                                 .Include(orders => orders.TotalDiscount)
                                 .Include(orders => orders.CurrentTotalPrice)
                                 .Include(orders => orders.TotalTax)
                                 .Include(orders => orders.discount_code)
                                 .Include(orders => orders.note_attributes)
                                 .Include(orders => orders.priceSet)
                                 .Include(orders => orders.taxLines)
                                 .Include(orders => orders.LineModels)
                                 .Include(orders => orders.totalShipping)
                                 .Include(orders => orders.billing_address)
                                 .Include(orders => orders.discount_applications)
                                 .Include(orders => orders.fulfillment)
                                 .Include(orders => orders.LineItems)
                                 .Include(orders => orders.ShippingAddress)
                                 .Include(orders => orders.ShippingLines)
                                 .FirstOrDefaultAsync();
        }



        public async Task<OrdersModel> GetOrderById(long orderid)
        {
            // Make sure to use `await` with `FirstOrDefaultAsync`
            return await _context.Orders.FirstOrDefaultAsync(order => order.orderid == orderid);
        }

        public async Task UpdateOrder(OrdersModel order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrdersModel>> GetOrdersByCustomer(long userId, int limit)
        {
            return await _context.Orders
                .Where(order => order.user_id == userId)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<long> GetNextOrderIdAsync()
        {
            var latestOrder = await _context.Orders
                .OrderByDescending(o => o.orderid)
                .FirstOrDefaultAsync();

            if (latestOrder == null)
            {
                return 5916711321844;
            }

            return latestOrder.orderid + 1;
        }

        public async Task<string> GetNextOrderNameAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.name != null && o.name.StartsWith("#"))
                .Select(o => o.name)
                .ToListAsync();

            int maxNumber = 0;

            foreach (var name in orders)
            {
                var numberPart = name.Substring(1).Trim();
                if (int.TryParse(numberPart, out int number))
                {
                    if (number > maxNumber)
                    {
                        maxNumber = number;
                    }
                }
            }

            return $"#{maxNumber + 1}";
        }

        // 🔧 Builds:  "name.Contains(@0) OR tags.Contains(@0) OR ... "
        private static string BuildGlobalStringPredicate(Type modelType)
        {
            var stringProps = modelType
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => $"{p.Name} != null && {p.Name}.Contains(@0)");

            // Fall back to "true" so EF doesn't complain if the class has no string properties
            return stringProps.Any() ? string.Join(" OR ", stringProps) : "true";
        }


    }
}
