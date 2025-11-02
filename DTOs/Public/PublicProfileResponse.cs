namespace MeetFlow_Backend.DTOs.Public;

public class PublicProfileResponse
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<PublicEventTypeResponse> EventTypes { get; set; } = new();
}

public class PublicEventTypeResponse
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class PublicEventTypeDetailsResponse
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public PublicEventTypeResponse EventType { get; set; } = new();
    public List<PublicAvailabilityResponse> Availability { get; set; } = new();
}

public class PublicAvailabilityResponse
{
    public int DayOfWeek { get; set; }
    public string DayOfWeekName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;  // "09:00:00"
    public string EndTime { get; set; } = string.Empty;    // "17:00:00"
}