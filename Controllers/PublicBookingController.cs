using Microsoft.AspNetCore.Mvc;
using MeetFlow_Backend.Services.Interfaces;
using MeetFlow_Backend.DTOs.Availability;

namespace MeetFlow_Backend.Controllers;

[ApiController]
[Route("api/public/{username}/{slug}")]
public class PublicBookingController : ControllerBase
{
    private readonly IAvailabilityCalculationService _availabilityCalculationService;
    
    public PublicBookingController(IAvailabilityCalculationService availabilityCalculationService)
    {
        _availabilityCalculationService = availabilityCalculationService;
    }

    /// <summary>
    /// Get available booking slots for a specific date (PUBLIC - no auth required)
    /// </summary>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(DayAvailabilityResponse), 200)]
    public async Task<ActionResult<DayAvailabilityResponse>> GetAvailability(
        string username,
        string slug,
        [FromQuery] string date) // Format: YYYY-MM-DD
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsedDate))
        {
            return BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD" });
        }

        try
        {
            var availability = await _availabilityCalculationService.GetAvailableSlotsAsync(
                username,
                slug,
                parsedDate
            );

            if (availability == null)
            {
                return NotFound(new { error = "User or event type not found" });
            }

            return Ok(availability);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to fetch availability: {ex.Message}" });
        }
    }
}