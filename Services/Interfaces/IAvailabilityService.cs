using MeetFlow_Backend.DTOs.Availability;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IAvailabilityService
{
    Task<AvailabilityResponse> CreateAvailabilityAsync(Guid userId, CreateAvailabilityRequest request);
    Task<List<AvailabilityResponse>> GetUserAvailabilitiesAsync(Guid userId);
    Task<bool> DeleteAvailabilityAsync(Guid availabilityId, Guid userId);
    Task<bool> BulkSetAvailabilityAsync(Guid userId, List<CreateAvailabilityRequest> availabilities);
}