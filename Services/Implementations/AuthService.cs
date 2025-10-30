using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.Auth;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Interfaces;
using BCrypt.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MeetFlow_Backend.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IJwtService jwtService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
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

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        // Step 1: code to access token
        var tokenEndpoint = "https://oauth2.googleapis.com/token";
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        
        var clientId = _configuration["GoogleOAuth:ClientId"]!;
        var clientSecret = _configuration["GoogleOAuth:ClientSecret"]!;
        
        var tokenParams = new Dictionary<string, string>
        {
            { "code", request.Code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", request.RedirectUri },
            { "grant_type", "authorization_code" }
        };
        
        tokenRequest.Content = new FormUrlEncodedContent(tokenParams);

        using var httpClient = new HttpClient();
        var tokenResponse = await httpClient.SendAsync(tokenRequest);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new Exception("Failed to exchange code for token");
        }
        
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);
        var accessToken = tokenData.GetProperty("access_token").GetString()!;
        
        // Step 2: Get User info from Google
        var userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userInfoResponse = await httpClient.SendAsync(userInfoRequest);

        if (!userInfoResponse.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get user info from Google");
        }

        var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoJson);
        
        var googleId = userInfo.GetProperty("id").GetString()!;
        var email = userInfo.GetProperty("email").GetString()!;
        var name = userInfo.GetProperty("name").GetString()!;
        
        // Step 3: Check if user exist
        var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email);

        if (user == null)
        {
            // Step 4: Create new user
            var username = email.Split('@')[0].ToLower();
            
            // Check username unique
            var existingUsername = await _context.Users.AnyAsync(u => u.Username == username);
            if (existingUsername)
            {
                username = $"{username}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            
            user = new User
            {
                Email = email,
                GoogleId = googleId,
                FullName = name,
                Username = username,
                PasswordHash = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Step 5: Update GoogleID if null
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = googleId;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        
        // Step 6: Generate JWT
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