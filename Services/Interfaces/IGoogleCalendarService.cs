using MeetFlow_Backend.Models;
using MeetFlow_Backend.DTOs.Google;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IGoogleCalendarService
{
    Task<string> GetAuthorizationUrlAsync(Guid userId);
    Task<GoogleIntegration> HandleCallbackAsync(string code, Guid userId);
    Task<bool> RefreshTokenAsync(Guid userId);
    Task<GoogleIntegrationResponse> GetIntegrationStatusAsync(Guid userId);
    Task<bool> DisconnectAsync(Guid userId);
    Task<List<CalendarDto>?> GetCalendarsAsync(Guid userId);
}