using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.Auth;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Interfaces;
using BCrypt.Net;

namespace MeetFlow_Backend.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new Exception("Email already exists");
        }

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            throw new Exception("Username already exists");
        }
        
        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        // Create user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Username = request.Username.ToLower(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            },
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || user.PasswordHash == null)
        {
            throw new Exception("Invalid credentials");
        }
        
        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials");
        }
        
        // Generate JWT token
        var token = _jwtService.GenerateToken(user);
        
        return new AuthResponse
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            },
            Token = token
        };
    }
}