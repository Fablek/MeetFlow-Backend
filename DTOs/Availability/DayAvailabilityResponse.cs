namespace MeetFlow_Backend.DTOs.Availability;

public class DayAvailabilityResponse
{
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
    public EventTypeInfo EventType { get; set; } = null!;
}

public class EventTypeInfo
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
}