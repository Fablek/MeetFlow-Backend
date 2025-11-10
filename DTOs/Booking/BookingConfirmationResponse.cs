namespace MeetFlow_Backend.DTOs.Booking;

public class BookingConfirmationResponse
{
    public Guid BookingId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? LocationDetails { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? GoogleCalendarEventId { get; set; }
}