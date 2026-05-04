using Backend.Data;
using Backend.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Repositories
{
    public class TimelineServicesRepository : ITimelineServices
    {
        private readonly MyDbContext _context;

        public TimelineServicesRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<(List<TimelineEventDTO> events, int total)> GetTimelineEventsByOrderId(long orderId)
        {
            var timelineEvents = await _context.TimelineEvents
                .Include(t => t.User)
                .Where(t => t.OrderId == orderId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var eventDTOs = timelineEvents.Select(MapToDTO).ToList();

            return (eventDTOs, eventDTOs.Count);
        }

        public async Task<TimelineEventDTO?> GetTimelineEventById(int eventId)
        {
            var timelineEvent = await _context.TimelineEvents
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == eventId);

            return timelineEvent != null ? MapToDTO(timelineEvent) : null;
        }

        public async Task<TimelineEventDTO> CreateTimelineEvent(CreateTimelineEventDTO request)
        {
            // Get user information
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {request.UserId} not found");
            }

            var userName = $"{user.first_name} {user.last_name}".Trim();
            if (string.IsNullOrEmpty(userName))
            {
                userName = user.email ?? "Unknown User";
            }

            var timelineEvent = new TimelineEventModel
            {
                OrderId = request.OrderId,
                UserId = request.UserId,
                UserName = userName,
                EventType = request.EventType,
                Description = request.Description,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimelineEvents.Add(timelineEvent);
            await _context.SaveChangesAsync();

            // Reload with user information
            var savedEvent = await _context.TimelineEvents
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == timelineEvent.Id);

            return savedEvent != null ? MapToDTO(savedEvent) : null!;
        }

        public async Task<bool> DeleteTimelineEvent(int eventId)
        {
            var timelineEvent = await _context.TimelineEvents.FindAsync(eventId);

            if (timelineEvent == null)
                return false;

            _context.TimelineEvents.Remove(timelineEvent);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TimelineEventDTO> CreateTimelineEventAuto(long orderId, long userId, string eventType, string description, Dictionary<string, object>? metadata = null)
        {
            var request = new CreateTimelineEventDTO
            {
                OrderId = orderId,
                UserId = userId,
                EventType = eventType,
                Description = description,
                Metadata = metadata
            };

            return await CreateTimelineEvent(request);
        }

        private TimelineEventDTO MapToDTO(TimelineEventModel model)
        {
            Dictionary<string, object>? metadata = null;

            if (!string.IsNullOrEmpty(model.Metadata))
            {
                try
                {
                    metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(model.Metadata);
                }
                catch
                {
                    // If deserialization fails, leave metadata as null
                    metadata = null;
                }
            }

            return new TimelineEventDTO
            {
                Id = model.Id,
                OrderId = model.OrderId,
                UserId = model.UserId,
                UserName = model.UserName,
                EventType = model.EventType,
                Description = model.Description,
                Metadata = metadata,
                CreatedAt = model.CreatedAt
            };
        }
    }
}
