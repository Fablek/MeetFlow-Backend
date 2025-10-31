using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.User;
using MeetFlow_Backend.Services.Interfaces;

namespace MeetFlow_Backend.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileResponse?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Username = user.Username,
            GoogleId = user.GoogleId,
            HasPassword = !string.IsNullOrEmpty(user.PasswordHash),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }
        
        // Check if new usersame exist
        if (user.Username != request.Username.ToLower())
        {
            var usernameExists = await _context.Users.AnyAsync(u => u.Username == request.Username.ToLower());
            if (usernameExists)
            {
                throw new Exception("Username is already taken");
            }

            user.Username = request.Username.ToLower();
        }
        
        // Update data
        user.FullName = request.FullName;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Username = user.Username,
            GoogleId = user.GoogleId,
            HasPassword = !string.IsNullOrEmpty(user.PasswordHash),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}