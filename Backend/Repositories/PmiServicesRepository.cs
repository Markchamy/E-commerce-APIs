using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Backend.Repositories
{
    public class PmiServicesRepository : IPmiServices
    {
        private readonly MyDbContext _context;

        public PmiServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        // Customer operations
        public async Task<PmiCustomer?> GetCustomerByIdAsync(long customerId)
        {
            return await _context.PmiCustomers
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<PmiCustomer?> GetCustomerByPhoneAsync(string phone)
        {
            return await _context.PmiCustomers
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<ResponseBase> AddCustomerAsync(PmiCustomer customer)
        {
            try
            {
                _context.PmiCustomers.Add(customer);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Customer added successfully", customer);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error adding customer: {ex.Message}");
            }
        }

        // Order operations
        public async Task<PmiOrderListResponse> GetOrdersAsync(PmiOrderFilterParams filters)
        {
            var query = _context.PmiOrders
                .Include(o => o.Customer)
                .Include(o => o.Error)
                .Include(o => o.OrderedProducts)
                .Include(o => o.OrderedMachines)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filters.OrderReference))
            {
                query = query.Where(o => o.OrderReference.Contains(filters.OrderReference));
            }

            if (!string.IsNullOrEmpty(filters.OrderNumber))
            {
                query = query.Where(o => o.OrderNumber != null && o.OrderNumber.Contains(filters.OrderNumber));
            }

            if (filters.HasError.HasValue)
            {
                if (filters.HasError.Value)
                {
                    query = query.Where(o => o.ErrorId != null);
                }
                else
                {
                    query = query.Where(o => o.ErrorId == null);
                }
            }

            if (filters.Anonymous.HasValue)
            {
                query = query.Where(o => o.Anonymous == filters.Anonymous.Value);
            }

            if (filters.DateFrom.HasValue)
            {
                query = query.Where(o => o.DateCreated >= filters.DateFrom.Value);
            }

            if (filters.DateTo.HasValue)
            {
                query = query.Where(o => o.DateCreated <= filters.DateTo.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting using explicit expressions (avoids issues with value conversions + dynamic LINQ)
            var isAsc = filters.SortDirection?.ToLower() == "asc";
            var sortBy = (filters.SortBy ?? "dateCreated").ToLower();

            query = sortBy switch
            {
                "datecreated" => isAsc ? query.OrderBy(o => o.DateCreated) : query.OrderByDescending(o => o.DateCreated),
                "datedelivered" => isAsc ? query.OrderBy(o => o.DateDelivered) : query.OrderByDescending(o => o.DateDelivered),
                "ordernumber" => isAsc ? query.OrderBy(o => o.OrderNumber) : query.OrderByDescending(o => o.OrderNumber),
                "orderreference" => isAsc ? query.OrderBy(o => o.OrderReference) : query.OrderByDescending(o => o.OrderReference),
                _ => isAsc ? query.OrderBy(o => o.DateCreated) : query.OrderByDescending(o => o.DateCreated),
            };

            // Apply pagination
            var orders = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            // Map to DTOs
            var orderDtos = orders.Select(o => new PmiOrderDto
            {
                OrderReference = o.OrderReference,
                OrderNumber = o.OrderNumber,
                DateDelivered = o.DateDelivered,
                DateCreated = o.DateCreated,
                Anonymous = o.Anonymous,
                CustomerId = o.CustomerId,
                ErrorId = o.ErrorId,
                Customer = o.Customer != null ? new PmiCustomerDto
                {
                    Id = o.Customer.Id,
                    Name = o.Customer.Name,
                    LastName = o.Customer.LastName,
                    Phone = o.Customer.Phone,
                    Email = o.Customer.Email,
                    Address = o.Customer.Address
                } : null,
                Error = o.Error != null ? new PmiErrorDto
                {
                    Id = o.Error.Id,
                    Error = o.Error.Error
                } : null,
                Products = o.OrderedProducts?.Select(p => new PmiOrderedProductResponseDto
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    Price = p.Price
                }).ToList(),
                SerialNumbers = o.OrderedMachines?.Select(m => m.SerialNum ?? string.Empty).ToList()
            }).ToList();

            return new PmiOrderListResponse
            {
                Orders = orderDtos,
                TotalCount = totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize
            };
        }

        public async Task<PmiOrder?> GetOrderByReferenceAsync(string orderReference)
        {
            return await _context.PmiOrders
                .Include(o => o.Customer)
                .Include(o => o.Error)
                .Include(o => o.OrderedProducts)
                .Include(o => o.OrderedMachines)
                .FirstOrDefaultAsync(o => o.OrderReference == orderReference);
        }

        public async Task<List<PmiOrderedProductResponseDto>> GetOrderProductsAsync(string orderReference)
        {
            var products = await _context.PmiOrderedProducts
                .Where(p => p.OrderId == orderReference)
                .Select(p => new PmiOrderedProductResponseDto
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    Price = p.Price
                })
                .ToListAsync();

            return products;
        }

        public async Task<ResponseBase> AddOrderAsync(PmiOrder order)
        {
            try
            {
                order.DateCreated = DateTime.UtcNow;
                _context.PmiOrders.Add(order);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Order added successfully", order);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error adding order: {ex.Message}");
            }
        }

        public async Task<ResponseBase> AddOrderProductsAsync(string orderReference, List<PmiOrderedProduct> products, List<PmiOrderedMachine>? machines)
        {
            try
            {
                var order = await _context.PmiOrders.FindAsync(orderReference);
                if (order == null)
                {
                    return ResponseBase.Failure("Order not found");
                }

                foreach (var product in products)
                {
                    product.OrderId = orderReference;
                    _context.PmiOrderedProducts.Add(product);
                }

                if (machines != null)
                {
                    foreach (var machine in machines)
                    {
                        machine.OrderId = orderReference;
                        _context.PmiOrderedMachines.Add(machine);
                    }
                }

                await _context.SaveChangesAsync();
                return ResponseBase.Success("Products added to order successfully");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error adding products to order: {ex.Message}");
            }
        }

        public async Task<ResponseBase> UpdateOrderAsync(string orderReference, PmiOrderUpdateRequest request)
        {
            try
            {
                var order = await _context.PmiOrders.FindAsync(orderReference);
                if (order == null)
                {
                    return ResponseBase.Failure("Order not found");
                }

                if (request.OrderNumber != null)
                {
                    order.OrderNumber = request.OrderNumber;
                }

                if (request.DateDelivered.HasValue)
                {
                    order.DateDelivered = request.DateDelivered.Value;
                }

                if (request.Anonymous.HasValue)
                {
                    order.Anonymous = request.Anonymous.Value;
                }

                await _context.SaveChangesAsync();
                return ResponseBase.Success("Order updated successfully", order);
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error updating order: {ex.Message}");
            }
        }

        public async Task<ResponseBase> DeleteOrderAsync(string orderReference)
        {
            try
            {
                var order = await _context.PmiOrders
                    .Include(o => o.OrderedProducts)
                    .Include(o => o.OrderedMachines)
                    .FirstOrDefaultAsync(o => o.OrderReference == orderReference);

                if (order == null)
                {
                    return ResponseBase.Failure("Order not found");
                }

                // Delete related products and machines (cascading should handle this, but being explicit)
                if (order.OrderedProducts != null)
                {
                    _context.PmiOrderedProducts.RemoveRange(order.OrderedProducts);
                }

                if (order.OrderedMachines != null)
                {
                    _context.PmiOrderedMachines.RemoveRange(order.OrderedMachines);
                }

                _context.PmiOrders.Remove(order);
                await _context.SaveChangesAsync();
                return ResponseBase.Success("Order deleted successfully");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error deleting order: {ex.Message}");
            }
        }

        public async Task<ResponseBase> UpdateOrderErrorAsync(string orderReference, string? error)
        {
            try
            {
                var order = await _context.PmiOrders.FindAsync(orderReference);
                if (order == null)
                {
                    return ResponseBase.Failure("Order not found");
                }

                if (string.IsNullOrEmpty(error))
                {
                    order.ErrorId = null;
                }
                else
                {
                    var pmiError = await CreateErrorAsync(error);
                    order.ErrorId = pmiError.Id;
                }

                await _context.SaveChangesAsync();
                return ResponseBase.Success("Order error updated successfully");
            }
            catch (Exception ex)
            {
                return ResponseBase.Failure($"Error updating order error: {ex.Message}");
            }
        }

        public async Task<ResponseBase> ClearOrderErrorAsync(string orderReference)
        {
            return await UpdateOrderErrorAsync(orderReference, null);
        }

        // Product operations
        public async Task<PmiProduct?> GetProductByNameAsync(string productName)
        {
            return await _context.PmiProducts
                .FirstOrDefaultAsync(p => p.Name != null && p.Name.ToLower() == productName.ToLower());
        }

        public async Task<decimal?> GetProductPriceAsync(string productName)
        {
            var product = await GetProductByNameAsync(productName);
            return product?.Price;
        }

        // Machine/Serial operations
        public async Task<List<string>> GetOrderSerialNumbersAsync(string orderReference)
        {
            var serialNumbers = await _context.PmiOrderedMachines
                .Where(m => m.OrderId == orderReference && m.SerialNum != null)
                .Select(m => m.SerialNum!)
                .ToListAsync();

            return serialNumbers;
        }

        // Error operations
        public async Task<PmiError> CreateErrorAsync(string error)
        {
            var pmiError = new PmiError { Error = error };
            _context.PmiErrors.Add(pmiError);
            await _context.SaveChangesAsync();
            return pmiError;
        }

        public async Task<PmiErrorListResponse> GetErrorsAsync(PmiErrorFilterParams filters)
        {
            var query = _context.PmiErrors.AsQueryable();

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            var sortDirection = filters.SortDirection?.ToLower() == "asc" ? "ascending" : "descending";
            var sortBy = filters.SortBy ?? "Id";
            query = query.OrderBy($"{sortBy} {sortDirection}");

            // Apply pagination
            var errors = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            // Map to DTOs
            var errorDtos = errors.Select(e => new PmiErrorDto
            {
                Id = e.Id,
                Error = e.Error
            }).ToList();

            return new PmiErrorListResponse
            {
                Errors = errorDtos,
                TotalCount = totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize
            };
        }
    }
}
