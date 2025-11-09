namespace MeetFlow_Backend.DTOs.Google;

public class CalendarDto
{
    public string Id { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Primary { get; set; }
    public string? TimeZone { get; set; }
    public string? BackgroundColor { get; set; }
}