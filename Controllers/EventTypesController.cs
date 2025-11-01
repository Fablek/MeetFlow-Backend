using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetFlow_Backend.DTOs.EventType;
using MeetFlow_Backend.Services.Interfaces;
using System.Security.Claims;

namespace MeetFlow_Backend.Controllers;

[ApiController]
[Route("api/event-types")]
[Authorize]
public class EventTypesController : ControllerBase
{
    private readonly IEventTypeService _eventTypeService;
    
    public EventTypesController(IEventTypeService eventTypeService)
    {
        _eventTypeService = eventTypeService;
    }

    /// <summary>
    /// Create a new event type
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EventTypeResponse>> CreateEventType([FromBody] CreateEventTypeRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        try
        {
            var eventType = await _eventTypeService.CreateEventTypeAsync(userId, request);
            return CreatedAtAction(nameof(GetEventType), new { id = eventType.Id }, eventType);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all event types for current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EventTypeResponse>>> GetEventTypes()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }
        
        var eventTypes = await _eventTypeService.GetUserEventTypesAsync(userId);
        return Ok(eventTypes);
    }

    /// <summary>
    /// Get event type by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EventTypeResponse>> GetEventType(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }
        
        var eventType = await _eventTypeService.GetEventTypeByIdAsync(id, userId);

        if (eventType == null)
        {
            return NotFound(new { error = "Event type not found" });
        }

        return Ok(eventType);
    }

    /// <summary>
    /// Update event type
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<EventTypeResponse>> UpdateEventType(Guid id, [FromBody] UpdateEventTypeRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }
        
        try
        {
            var eventType = await _eventTypeService.UpdateEventTypeAsync(id, userId, request);

            if (eventType == null)
            {
                return NotFound(new { error = "Event type not found" });
            }

            return Ok(eventType);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete event type
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEventType(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        var deleted = await _eventTypeService.DeleteEventTypeAsync(id, userId);

        if (!deleted)
        {
            return NotFound(new { error = "Event type not found" });
        }

        return NoContent();
    }
}