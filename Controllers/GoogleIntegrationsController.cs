using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetFlow_Backend.Services.Interfaces;
using MeetFlow_Backend.DTOs.Google;
using System.Security.Claims;
using MeetFlow_Backend.Data;
using Microsoft.EntityFrameworkCore; 

namespace MeetFlow_Backend.Controllers;

[ApiController]
[Route("api/integrations/google")]
[Authorize]
public class GoogleIntegrationsController : ControllerBase
{
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly ApplicationDbContext _context;

    public GoogleIntegrationsController(IGoogleCalendarService googleCalendarService, ApplicationDbContext context)
    {
        _googleCalendarService = googleCalendarService;
        _context = context;
    }

    /// <summary>
    /// Step 1: Get Google OAuth authorization URL
    /// </summary>
    [HttpGet("connect")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> GetAuthorizationUrl()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        try
        {
            var authUrl = await _googleCalendarService.GetAuthorizationUrlAsync(userId);
            return Ok(new
            {
                authorizationUrl = authUrl,
                message = "Redirect user to this URL to authorize Google Calendar access"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to generate authorization URL: {ex.Message}" });
        }
    }

    /// <summary>
    /// Step 2: Handle OAuth callback and exchange code for tokens
    /// </summary>
    [HttpPost("callback")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> HandleCallback([FromBody] ConnectGoogleRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }
        
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { error = "Authorization code is required" });
        }

        try
        {
            var integration = await _googleCalendarService.HandleCallbackAsync(request.Code, userId);

            return Ok(new
            {
                message = "Google Calendar connected successfully!",
                googleEmail = integration.Email,
                calendarId = integration.CalendarId,
                expiresAt = integration.TokenExpiresAt
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Failed to connect Google Calendar: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get user's Google Calendars
    /// </summary>
    [HttpGet("calendars")]
    [ProducesResponseType(typeof(List<CalendarDto>), 200)]
    public async Task<ActionResult<List<CalendarDto>>> GetCalendars()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        try
        {
            var calendars = await _googleCalendarService.GetCalendarsAsync(userId);

            if (calendars == null)
            {
                return NotFound(new
                {
                    error = "Google Calendar not connected. Please connect first.",
                    hint = "Use GET /api/integrations/google/connect to get authorization URL"
                });
            }

            return Ok(calendars);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to fetch calendars: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get busy slots from Google Calendar
    /// </summary>
    [HttpPost("busy-slots")]
    [ProducesResponseType(typeof(List<BusySlotDto>), 200)]
    public async Task<ActionResult<List<BusySlotDto>>> GetBusySlots([FromBody] GetBusySlotsRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }
        
        if (request.StartDate >= request.EndDate)
        {
            return BadRequest(new { error = "StartDate must be before EndDate" });
        }
        
        if ((request.EndDate - request.StartDate).TotalDays > 90)
        {
            return BadRequest(new { error = "Date range cannot exceed 90 days" });
        }

        try
        {
            var busySlots = await _googleCalendarService.GetBusySlotsAsync(
                userId,
                request.StartDate,
                request.EndDate,
                request.CalendarIds
            );

            if (busySlots == null)
            {
                return NotFound(new
                {
                    error = "Google Calendar not connected. Please connect first.",
                    hint = "Use GET /api/integrations/google/connect to get authorization URL"
                });
            }

            return Ok(busySlots);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to fetch busy slots: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get Google Calendar integration status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> GetIntegrationStatus()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        try
        {
            var integration = await _context.GoogleIntegrations
                .FirstOrDefaultAsync(g => g.UserId == userId);
            
            if (integration == null || !integration.IsActive)
            {
                return Ok(new
                {
                    isConnected = false
                });
            }
            
            return Ok(new
            {
                isConnected = true,
                calendarId = integration.CalendarId,
                calendarName = integration.CalendarName,
                email = integration.Email,
                connectedAt = integration.CreatedAt
            });
        } 
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to get integration status: {ex.Message}" });
        }
    }
    
    /// <summary>
    /// Disconnect Google Calendar
    /// </summary>
    [HttpDelete("disconnect")]
    public async Task<ActionResult> DisconnectGoogle()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        try
        {
            var integration = await _context.GoogleIntegrations
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (integration == null)
            {
                return NotFound(new { error = "No Google Calendar integration found" });
            }

            _context.GoogleIntegrations.Remove(integration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Google Calendar disconnected successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to disconnect: {ex.Message}" });
        }
    }
}