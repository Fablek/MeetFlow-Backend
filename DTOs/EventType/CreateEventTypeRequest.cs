using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.EventType;

public class CreateEventTypeRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers and hyphens")]
    public string Slug { get; set; } = string.Empty;
    
    [Required]
    [Range(5, 480)] // 5 min to 8 h
    public int DurationMinutes { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Location { get; set; } = "Online";
    
    [StringLength(500)]
    public string? LocationDetails { get; set; }
    
    [Required]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g., #3b82f6)")]
    public string Color { get; set; } = "#3b82f6";
    
    [Range(0, 120)]
    public int BufferMinutes { get; set; } = 0;

    [Range(0, 168)] // 0 to 7 days
    public int MinNoticeHours { get; set; } = 24;
    
    [Range(1, 365)]
    public int MaxDaysInAdvance { get; set; } = 60;
}