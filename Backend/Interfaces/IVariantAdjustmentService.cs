using Backend.Models;

namespace Backend.Interfaces
{
    public interface IVariantAdjustmentService
    {
        /// <summary>
        /// Gets the adjustment history for a specific variant with pagination and filtering
        /// </summary>
        Task<(List<VariantAdjustmentHistory> adjustments, int totalRecords)> GetAdjustmentHistoryAsync(
            long variantId,
            int page = 1,
            int perPage = 25,
            string? activityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Creates a new adjustment record
        /// </summary>
        Task<VariantAdjustmentHistory> CreateAdjustmentAsync(
            long variantId,
            string activityType,
            string createdBy,
            long? createdById,
            int unavailableChange,
            int committedChange,
            int availableChange,
            int onHandChange,
            int incomingChange,
            string? activityReference = null,
            string? activityDescription = null,
            string? notes = null);

        /// <summary>
        /// Gets the current inventory status for a variant from the variants table
        /// </summary>
        Task<(int unavailable, int committed, int available, int onHand, int incoming, DateTime? lastUpdated)> GetCurrentInventoryStatusAsync(long variantId);
    }
}
