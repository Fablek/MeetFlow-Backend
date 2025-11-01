using MeetFlow_Backend.DTOs.EventType;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IEventTypeService
{
    Task<EventTypeResponse> CreateEventTypeAsync(Guid userId, CreateEventTypeRequest request);
    Task<List<EventTypeResponse>> GetUserEventTypesAsync(Guid userId);
    Task<EventTypeResponse?> GetEventTypeByIdAsync(Guid eventTypeId, Guid userId);
    Task<EventTypeResponse?> UpdateEventTypeAsync(Guid eventTypeId, Guid userId, UpdateEventTypeRequest request);
    Task<bool> DeleteEventTypeAsync(Guid eventTypeId, Guid userId);
}