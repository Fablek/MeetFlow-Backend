namespace MeetFlow_Backend.DTOs.Booking;

public class BookingListItemDto
{
    public Guid Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string? GuestPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EventTypeName { get; set; } = string.Empty;
    public string EventTypeSlug { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
}