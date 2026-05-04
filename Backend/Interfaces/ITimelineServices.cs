using Backend.Models;

namespace Backend.Interfaces
{
    public interface ITimelineServices
    {
        /// <summary>
        /// Get all timeline events for a specific order
        /// </summary>
        Task<(List<TimelineEventDTO> events, int total)> GetTimelineEventsByOrderId(long orderId);

        /// <summary>
        /// Get a single timeline event by ID
        /// </summary>
        Task<TimelineEventDTO?> GetTimelineEventById(int eventId);

        /// <summary>
        /// Create a new timeline event
        /// </summary>
        Task<TimelineEventDTO> CreateTimelineEvent(CreateTimelineEventDTO request);

        /// <summary>
        /// Delete a timeline event
        /// </summary>
        Task<bool> DeleteTimelineEvent(int eventId);

        /// <summary>
        /// Helper method to automatically create timeline event from context
        /// </summary>
        Task<TimelineEventDTO> CreateTimelineEventAuto(long orderId, long userId, string eventType, string description, Dictionary<string, object>? metadata = null);
    }
}
