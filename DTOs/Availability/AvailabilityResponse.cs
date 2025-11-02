namespace MeetFlow_Backend.DTOs.Availability;

public class AvailabilityResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int DayOfWeek { get; set; }
    public string DayOfWeekName { get; set; } = string.Empty; // "Monday", "Tuesday", etc.
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}