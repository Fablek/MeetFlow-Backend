using MeetFlow_Backend.Models;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
}