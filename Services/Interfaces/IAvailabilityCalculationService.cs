using MeetFlow_Backend.DTOs.Availability;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IAvailabilityCalculationService
{
    Task<DayAvailabilityResponse?> GetAvailableSlotsAsync(string username, string slug, DateOnly date);
}