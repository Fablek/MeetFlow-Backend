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