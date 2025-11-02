using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetFlow_Backend.DTOs.Availability;
using MeetFlow_Backend.Services.Interfaces;
using System.Security.Claims;

namespace MeetFlow_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    
    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    /// <summary>
    /// Create a new availability slot
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AvailabilityResponse>> CreateAvailability([FromBody] CreateAvailabilityRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }
        
        try
        {
            var availability = await _availabilityService.CreateAvailabilityAsync(userId, request);
            return CreatedAtAction(nameof(GetAvailabilities), availability);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get all availability slots for current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AvailabilityResponse>>> GetAvailabilities()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        var availabilities = await _availabilityService.GetUserAvailabilitiesAsync(userId);
        return Ok(availabilities);
    }
    
    /// <summary>
    /// Delete availability slot
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAvailability(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        var deleted = await _availabilityService.DeleteAvailabilityAsync(id, userId);

        if (!deleted)
        {
            return NotFound(new { error = "Availability slot not found" });
        }

        return NoContent();
    }
    
    /// <summary>
    /// Bulk set availability (replaces all existing slots)
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult> BulkSetAvailability([FromBody] List<CreateAvailabilityRequest> availabilities)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        try
        {
            await _availabilityService.BulkSetAvailabilityAsync(userId, availabilities);
            return Ok(new { message = "Availability updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}