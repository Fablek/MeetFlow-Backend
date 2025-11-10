using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetFlow_Backend.Services.Interfaces;
using MeetFlow_Backend.DTOs.Booking;
using System.Security.Claims;

namespace MeetFlow_Backend.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Get all bookings for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BookingListItemDto>), 200)]
    public async Task<ActionResult<List<BookingListItemDto>>> GetBookings(
        [FromQuery] string? filter = null) // upcoming, past, cancelled, all
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        try
        {
            var bookings = await _bookingService.GetUserBookingsAsync(userId, filter);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to fetch bookings: {ex.Message}" });
        }
    }
    
    /// <summary>
    /// Get a specific booking by ID
    /// </summary>
    [HttpGet("{bookingId}")]
    [ProducesResponseType(typeof(BookingListItemDto), 200)]
    public async Task<ActionResult<BookingListItemDto>> GetBookingById(Guid bookingId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId, userId);

            if (booking == null)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(booking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to fetch booking: {ex.Message}" });
        }
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpDelete("{bookingId}")]
    public async Task<ActionResult> CancelBooking(
        Guid bookingId,
        [FromBody] CancelBookingRequest? request = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        try
        {
            var success = await _bookingService.CancelBookingAsync(
                bookingId, 
                userId, 
                request?.Reason
            );

            if (!success)
            {
                return NotFound(new { error = "Booking not found or cannot be cancelled" });
            }

            return Ok(new { message = "Booking cancelled successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to cancel booking: {ex.Message}" });
        }
    }
}

public record CancelBookingRequest(string? Reason);