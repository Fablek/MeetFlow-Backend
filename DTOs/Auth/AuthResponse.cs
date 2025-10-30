namespace MeetFlow_Backend.DTOs.Auth;

public class AuthResponse
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}