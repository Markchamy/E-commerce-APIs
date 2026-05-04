using Backend.Models;

namespace Backend.Interfaces
{
    public interface IInventoryReservationService
    {
        /// <summary>
        /// Reserves inventory for a newly created order
        /// </summary>
        Task<InventoryReservationResult> ReserveInventoryForOrderAsync(long orderId);

        /// <summary>
        /// Releases reserved inventory when order is fulfilled or out for delivery
        /// </summary>
        Task<InventoryReservationResult> ReleaseReservedInventoryForOrderAsync(long orderId);

        /// <summary>
        /// Releases reserved inventory when order is cancelled
        /// </summary>
        Task<InventoryReservationResult> CancelOrderInventoryReservationAsync(long orderId);

        /// <summary>
        /// Checks if sufficient inventory is available for the requested quantities
        /// </summary>
        Task<bool> CheckInventoryAvailabilityAsync(List<InventoryCheckItem> items);
    }

    public class InventoryReservationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<InventoryReservationItem> Items { get; set; } = new();
        public int TotalReserved { get; set; }
        public int TotalReleased { get; set; }
    }

    public class InventoryReservationItem
    {
        public long VariantId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int AvailableBefore { get; set; }
        public int AvailableAfter { get; set; }
    }

    public class InventoryCheckItem
    {
        public long VariantId { get; set; }
        public int RequestedQuantity { get; set; }
    }
}
