using MeetFlow_Backend.DTOs.User;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse?> GetCurrentUserAsync(Guid userId);
}