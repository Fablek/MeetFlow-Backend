namespace MeetFlow_Backend.DTOs.EventType;

public class EventTypeResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? LocationDetails { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int BufferMinutes { get; set; }
    public int MinNoticeHours { get; set; }
    public int MaxDaysInAdvance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}