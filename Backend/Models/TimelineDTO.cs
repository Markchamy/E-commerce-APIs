using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    /// <summary>
    /// DTO for creating a new timeline event
    /// </summary>
    public class CreateTimelineEventDTO
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; }

        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Additional metadata for the event (will be serialized to JSON)
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// DTO for timeline event response
    /// </summary>
    public class TimelineEventDTO
    {
        public int Id { get; set; }
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Response wrapper for a single timeline event
    /// </summary>
    public class TimelineEventResponse
    {
        public TimelineEventDTO Data { get; set; }
    }

    /// <summary>
    /// Response wrapper for multiple timeline events
    /// </summary>
    public class TimelineEventsResponse
    {
        public List<TimelineEventDTO> Events { get; set; }
        public int Total { get; set; }
    }

    /// <summary>
    /// Response for delete operations
    /// </summary>
    public class TimelineDeleteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Available timeline event types
    /// </summary>
    public static class TimelineEventTypes
    {
        public const string OrderCreated = "order_created";
        public const string OrderUpdated = "order_updated";
        public const string OrderCancelled = "order_cancelled";
        public const string OrderArchived = "order_archived";
        public const string FulfillmentUpdated = "fulfillment_updated";
        public const string ItemFulfilled = "item_fulfilled";
        public const string ReadyForPickup = "ready_for_pickup";
        public const string PickedUp = "picked_up";
        public const string PaymentPending = "payment_pending";
        public const string PaymentMarkedPaid = "payment_marked_paid";
        public const string PaymentRefunded = "payment_refunded";
        public const string ConfirmationEmailSent = "confirmation_email_sent";
        public const string TagsUpdated = "tags_updated";
        public const string NotesUpdated = "notes_updated";
        public const string CustomerUpdated = "customer_updated";
        public const string CreatedFromDraft = "created_from_draft";
    }
}
