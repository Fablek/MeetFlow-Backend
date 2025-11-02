using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.Public;

namespace MeetFlow_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public PublicController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get public profile by username
    /// </summary>
    [HttpGet("{username}")]
    public async Task<ActionResult<PublicProfileResponse>> GetPublicProfile(string username)
    {
        // Normalize username
        var normalizedUsername = username.ToLower();
        
        // Find user by username
        var user = await _context.Users
            .Include(u => u.EventTypes.Where(e => e.IsActive))
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername);

        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }
        
        // Map to public response
        var response = new PublicProfileResponse
        {
            Username = user.Username,
            FullName = user.FullName,
            EventTypes = user.EventTypes
                .OrderBy(e => e.Name)
                .Select(e => new PublicEventTypeResponse
                {
                    Name = e.Name,
                    Slug = e.Slug,
                    DurationMinutes = e.DurationMinutes,
                    Description = e.Description,
                    Location = e.Location,
                    Color = e.Color
                })
                .ToList()
        };
        
        return Ok(response);
    }
}