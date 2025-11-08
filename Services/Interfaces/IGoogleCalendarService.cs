using MeetFlow_Backend.Models;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IGoogleCalendarService
{
    Task<string> GetAuthorizationUrlAsync(Guid userId);
    Task<GoogleIntegration> HandleCallbackAsync(string code, Guid userId);
    Task<bool> RefreshTokenAsync(Guid userId);
}