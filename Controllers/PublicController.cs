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

    /// <summary>
    /// Get public event type details with availability
    /// </summary>
    [HttpGet("{username}/{slug}")]
    public async Task<ActionResult<PublicEventTypeDetailsResponse>> GetPublicEventType(string username, string slug)
    {
        var normalizedUsername = username.ToLower();
        var normalizedSlug = slug.ToLower();
        
        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername);

        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }
        
        // Find event type
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(e => e.UserId == user.Id 
                                      && e.Slug == normalizedSlug 
                                      && e.IsActive);

        if (eventType == null)
        {
            return NotFound(new { error = "Event type not found" });
        }
        
        // Get user's availability
        var availabilities = await _context.Availabilities
            .Where(a => a.UserId == user.Id)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        var response = new PublicEventTypeDetailsResponse
        {
            Username = user.Username,
            FullName = user.FullName,
            EventType = new PublicEventTypeResponse
            {
                Name = eventType.Name,
                Slug = eventType.Slug,
                DurationMinutes = eventType.DurationMinutes,
                Description = eventType.Description,
                Location = eventType.Location,
                Color = eventType.Color
            },
            Availability = availabilities.Select(a => new PublicAvailabilityResponse
            {
                DayOfWeek = a.DayOfWeek,
                DayOfWeekName = GetDayName(a.DayOfWeek),
                StartTime = a.StartTime.ToString("HH:mm:ss"),
                EndTime = a.EndTime.ToString("HH:mm:ss")
            }).ToList()
        };

        return Ok(response);
    }
    
    private static string GetDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Sunday",
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            _ => "Unknown"
        };
    }
}