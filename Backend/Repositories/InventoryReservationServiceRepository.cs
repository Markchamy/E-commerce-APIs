using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class InventoryReservationServiceRepository : IInventoryReservationService
    {
        private readonly MyDbContext _context;

        public InventoryReservationServiceRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryReservationResult> ReserveInventoryForOrderAsync(long orderId)
        {
            var result = new InventoryReservationResult();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.LineItems)
                    .FirstOrDefaultAsync(o => o.orderid == orderId);

                if (order == null)
                {
                    result.Success = false;
                    result.Message = $"Order {orderId} not found";
                    return result;
                }

                foreach (var lineItem in order.LineItems)
                {
                    var variant = await _context.Variants
                        .FirstOrDefaultAsync(v => v.id == lineItem.variant_id);

                    if (variant == null)
                    {
                        result.Success = false;
                        result.Message = $"Variant {lineItem.variant_id} not found";
                        return result;
                    }

                    // Check available quantity
                    int currentReserved = variant.reserved_quantity ?? 0;
                    int availableQty = variant.inventory_quantity - currentReserved;

                    if (availableQty < lineItem.quantity)
                    {
                        result.Success = false;
                        result.Message = $"Insufficient inventory for SKU {variant.sku}. Available: {availableQty}, Requested: {lineItem.quantity}";
                        return result;
                    }

                    // Store values for transaction log
                    int oldReserved = currentReserved;
                    int newReserved = currentReserved + lineItem.quantity;

                    // Reserve the quantity
                    variant.reserved_quantity = newReserved;
                    variant.updated_at = DateTime.UtcNow;

                    // Log transaction
                    var transaction = new InventoryTransactionLog
                    {
                        variant_id = variant.id,
                        transaction_type = "ORDER_RESERVE",
                        quantity_change = -lineItem.quantity, // Negative because it reduces available
                        inventory_before = variant.inventory_quantity,
                        inventory_after = variant.inventory_quantity, // Unchanged
                        reserved_before = oldReserved,
                        reserved_after = newReserved,
                        reason = $"Order #{order.order_number} Created",
                        order_id = orderId,
                        line_item_id = lineItem.lineItemId,
                        performed_by = "OrderSystem",
                        created_at = DateTime.UtcNow
                    };
                    _context.InventoryTransactionLog.Add(transaction);

                    // Add to result
                    result.Items.Add(new InventoryReservationItem
                    {
                        VariantId = variant.id,
                        Sku = variant.sku ?? string.Empty,
                        Quantity = lineItem.quantity,
                        AvailableBefore = availableQty,
                        AvailableAfter = availableQty - lineItem.quantity
                    });

                    result.TotalReserved += lineItem.quantity;
                }

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Successfully reserved inventory for order {order.order_number}";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error reserving inventory: {ex.Message}";
                return result;
            }
        }

        public async Task<InventoryReservationResult> ReleaseReservedInventoryForOrderAsync(long orderId)
        {
            var result = new InventoryReservationResult();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.LineItems)
                    .FirstOrDefaultAsync(o => o.orderid == orderId);

                if (order == null)
                {
                    result.Success = false;
                    result.Message = $"Order {orderId} not found";
                    return result;
                }

                foreach (var lineItem in order.LineItems)
                {
                    var variant = await _context.Variants
                        .FirstOrDefaultAsync(v => v.id == lineItem.variant_id);

                    if (variant == null)
                        continue;

                    int oldReserved = variant.reserved_quantity ?? 0;
                    int newReserved = Math.Max(0, oldReserved - lineItem.quantity);
                    int availableBefore = variant.inventory_quantity - oldReserved;
                    int availableAfter = variant.inventory_quantity - newReserved;

                    // Release the reservation
                    variant.reserved_quantity = newReserved;
                    variant.updated_at = DateTime.UtcNow;

                    // Log transaction
                    var transaction = new InventoryTransactionLog
                    {
                        variant_id = variant.id,
                        transaction_type = "FULFILLMENT",
                        quantity_change = lineItem.quantity, // Positive because it frees up reserved
                        inventory_before = variant.inventory_quantity,
                        inventory_after = variant.inventory_quantity,
                        reserved_before = oldReserved,
                        reserved_after = newReserved,
                        reason = $"Order #{order.order_number} Fulfilled/Out for Delivery",
                        order_id = orderId,
                        line_item_id = lineItem.lineItemId,
                        performed_by = "OrderSystem",
                        created_at = DateTime.UtcNow
                    };
                    _context.InventoryTransactionLog.Add(transaction);

                    result.Items.Add(new InventoryReservationItem
                    {
                        VariantId = variant.id,
                        Sku = variant.sku ?? string.Empty,
                        Quantity = lineItem.quantity,
                        AvailableBefore = availableBefore,
                        AvailableAfter = availableAfter
                    });

                    result.TotalReleased += lineItem.quantity;
                }

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Successfully released inventory for order {order.order_number}";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error releasing inventory: {ex.Message}";
                return result;
            }
        }

        public async Task<InventoryReservationResult> CancelOrderInventoryReservationAsync(long orderId)
        {
            var result = new InventoryReservationResult();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.LineItems)
                    .FirstOrDefaultAsync(o => o.orderid == orderId);

                if (order == null)
                {
                    result.Success = false;
                    result.Message = $"Order {orderId} not found";
                    return result;
                }

                foreach (var lineItem in order.LineItems)
                {
                    var variant = await _context.Variants
                        .FirstOrDefaultAsync(v => v.id == lineItem.variant_id);

                    if (variant == null)
                        continue;

                    int oldReserved = variant.reserved_quantity ?? 0;
                    int newReserved = Math.Max(0, oldReserved - lineItem.quantity);
                    int availableBefore = variant.inventory_quantity - oldReserved;
                    int availableAfter = variant.inventory_quantity - newReserved;

                    // Release the reservation
                    variant.reserved_quantity = newReserved;
                    variant.updated_at = DateTime.UtcNow;

                    // Log transaction
                    var transaction = new InventoryTransactionLog
                    {
                        variant_id = variant.id,
                        transaction_type = "CANCELLATION",
                        quantity_change = lineItem.quantity, // Positive because it frees up reserved
                        inventory_before = variant.inventory_quantity,
                        inventory_after = variant.inventory_quantity,
                        reserved_before = oldReserved,
                        reserved_after = newReserved,
                        reason = $"Order #{order.order_number} Cancelled",
                        order_id = orderId,
                        line_item_id = lineItem.lineItemId,
                        performed_by = "OrderSystem",
                        created_at = DateTime.UtcNow
                    };
                    _context.InventoryTransactionLog.Add(transaction);

                    result.Items.Add(new InventoryReservationItem
                    {
                        VariantId = variant.id,
                        Sku = variant.sku ?? string.Empty,
                        Quantity = lineItem.quantity,
                        AvailableBefore = availableBefore,
                        AvailableAfter = availableAfter
                    });

                    result.TotalReleased += lineItem.quantity;
                }

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"Successfully cancelled reservation for order {order.order_number}";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error cancelling reservation: {ex.Message}";
                return result;
            }
        }

        public async Task<bool> CheckInventoryAvailabilityAsync(List<InventoryCheckItem> items)
        {
            foreach (var item in items)
            {
                var variant = await _context.Variants
                    .FirstOrDefaultAsync(v => v.id == item.VariantId);

                if (variant == null)
                    return false;

                int availableQty = variant.inventory_quantity - (variant.reserved_quantity ?? 0);

                if (availableQty < item.RequestedQuantity)
                    return false;
            }

            return true;
        }
    }
}
