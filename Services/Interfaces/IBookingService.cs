using MeetFlow_Backend.DTOs.Booking;
using MeetFlow_Backend.Models;

namespace MeetFlow_Backend.Services.Interfaces;

public interface IBookingService
{
    Task<BookingConfirmationResponse?> CreateBookingAsync(
        string username, 
        string slug, 
        CreateBookingRequest request);
}