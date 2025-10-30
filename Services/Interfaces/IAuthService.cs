using MeetFlow_Backend.DTOs.Auth;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request);
}