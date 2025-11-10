using MeetFlow_Backend.DTOs.Booking;
using MeetFlow_Backend.Models;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IBookingService
{
    Task<BookingConfirmationResponse?> CreateBookingAsync(
        string username, 
        string slug, 
        CreateBookingRequest request);
    Task<List<BookingListItemDto>> GetUserBookingsAsync(Guid userId, string? filter = null);
    Task<BookingListItemDto?> GetBookingByIdAsync(Guid bookingId, Guid userId);
    Task<bool> CancelBookingAsync(Guid bookingId, Guid userId, string? reason = null);
}