using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.Google;

public class GetBusySlotsRequest
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public List<string>? CalendarIds { get; set; } // Optional: specific calendars, null = primary only
}