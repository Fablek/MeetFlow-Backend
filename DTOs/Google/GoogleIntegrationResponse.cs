namespace MeetFlow_Backend.DTOs.Google;

public class GoogleIntegrationResponse
{
    public bool IsConnected { get; set; }
    public string? GoogleEmail { get; set; }
    public string? CalendarId { get; set; }
    public DateTime? ConnectedAt { get; set; }
}