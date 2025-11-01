using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.EventType;

public class UpdateEventTypeRequest
{
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }

    [StringLength(100, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers and hyphens")]
    public string? Slug { get; set; }

    [Range(5, 480)]
    public int? DurationMinutes { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Location { get; set; }
    
    [StringLength(500)]
    public string? LocationDetails { get; set; }

    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color")]
    public string? Color { get; set; }

    public bool? IsActive { get; set; }

    [Range(0, 120)]
    public int? BufferMinutes { get; set; }

    [Range(0, 168)]
    public int? MinNoticeHours { get; set; }

    [Range(1, 365)]
    public int? MaxDaysInAdvance { get; set; }
}