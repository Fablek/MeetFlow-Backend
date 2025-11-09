namespace MeetFlow_Backend.DTOs.Google;

public class BusySlotDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Summary { get; set; }
    public string CalendarId { get; set; } = string.Empty;
}