using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.Booking;

public class CreateBookingRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string GuestEmail { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string GuestName { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? GuestPhone { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
}