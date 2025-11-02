using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.Availability;

public class CreateAvailabilityRequest
{
    [Required]
    [Range(0, 6, ErrorMessage = "DayOfWeek must be between 0 (Sunday) and 6 (Saturday)")]
    public int DayOfWeek { get; set; }
    
    [Required]
    public TimeOnly StartTime { get; set; }
    
    [Required]
    public TimeOnly EndTime { get; set; }
}