using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class VariantAdjustmentServiceRepository : IVariantAdjustmentService
    {
        private readonly MyDbContext _context;

        public VariantAdjustmentServiceRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<(List<VariantAdjustmentHistory> adjustments, int totalRecords)> GetAdjustmentHistoryAsync(
            long variantId,
            int page = 1,
            int perPage = 25,
            string? activityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.VariantAdjustmentHistory
                .Where(a => a.VariantId == variantId);

            // Apply filters
            if (!string.IsNullOrEmpty(activityType))
            {
                query = query.Where(a => a.ActivityType == activityType);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= endDate.Value);
            }

            // Get total count for pagination
            var totalRecords = await query.CountAsync();

            // Apply pagination and ordering
            var adjustments = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            return (adjustments, totalRecords);
        }

        public async Task<VariantAdjustmentHistory> CreateAdjustmentAsync(
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
            string? notes = null)
        {
            // Get the variant to find its product_id and current inventory
            var variant = await _context.Variants.FindAsync(variantId);
            if (variant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");
            }

            // Get the most recent adjustment to calculate new snapshot values
            var lastAdjustment = await _context.VariantAdjustmentHistory
                .Where(a => a.VariantId == variantId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            // Calculate new snapshot values (after changes)
            int newUnavailable, newCommitted, newAvailable, newOnHand, newIncoming;

            if (lastAdjustment != null)
            {
                // Use last adjustment's snapshot values as base
                newUnavailable = lastAdjustment.Unavailable + unavailableChange;
                newCommitted = lastAdjustment.Committed + committedChange;
                newAvailable = lastAdjustment.Available + availableChange;
                newOnHand = lastAdjustment.OnHand + onHandChange;
                newIncoming = lastAdjustment.Incoming + incomingChange;
            }
            else
            {
                // First adjustment - use variant's current inventory_quantity as starting point
                // Assume all current inventory is available, then apply the changes
                int currentInventory = variant.inventory_quantity;

                newUnavailable = Math.Max(0, unavailableChange);
                newCommitted = Math.Max(0, committedChange);
                newAvailable = currentInventory + availableChange; // Start with current inventory, apply change
                newOnHand = currentInventory + onHandChange;
                newIncoming = Math.Max(0, incomingChange);
            }

            // Validate that quantities don't go negative
            if (newUnavailable < 0 || newCommitted < 0 || newAvailable < 0 || newOnHand < 0 || newIncoming < 0)
            {
                throw new InvalidOperationException("Inventory adjustment would result in negative quantities");
            }

            var adjustment = new VariantAdjustmentHistory
            {
                VariantId = variantId,
                ProductId = variant.product_id,
                ActivityType = activityType,
                ActivityReference = activityReference,
                ActivityDescription = activityDescription,
                CreatedBy = createdBy,
                CreatedById = createdById,
                Unavailable = newUnavailable,
                Committed = newCommitted,
                Available = newAvailable,
                OnHand = newOnHand,
                Incoming = newIncoming,
                UnavailableChange = unavailableChange,
                CommittedChange = committedChange,
                AvailableChange = availableChange,
                OnHandChange = onHandChange,
                IncomingChange = incomingChange,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.VariantAdjustmentHistory.Add(adjustment);
            await _context.SaveChangesAsync();

            return adjustment;
        }

        public async Task<(int unavailable, int committed, int available, int onHand, int incoming, DateTime? lastUpdated)> GetCurrentInventoryStatusAsync(long variantId)
        {
            // Get the most recent adjustment record for this variant
            var lastAdjustment = await _context.VariantAdjustmentHistory
                .Where(a => a.VariantId == variantId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastAdjustment != null)
            {
                return (
                    lastAdjustment.Unavailable,
                    lastAdjustment.Committed,
                    lastAdjustment.Available,
                    lastAdjustment.OnHand,
                    lastAdjustment.Incoming,
                    lastAdjustment.CreatedAt
                );
            }

            // If no adjustment history exists, return zeros
            return (0, 0, 0, 0, 0, null);
        }
    }
}
