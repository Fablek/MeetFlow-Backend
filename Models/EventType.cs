using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetFlow_Backend.Models;

public class EventType
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;
    
    [Required]
    public int DurationMinutes { get; set; } // 15, 30, 45, 60, 90, 120
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Location { get; set; } = "Online"; // Online, In-person, Phone
    
    [MaxLength(500)]
    public string? LocationDetails { get; set; } // Link Zoom, address, etc.
    
    [Required]
    [MaxLength(7)]
    public string Color { get; set; } = "#3b82f6"; // Hex color for UI
    
    public bool IsActive { get; set; } = true;
    
    // Advanced settings
    public int BufferMinutes { get; set; } = 0; // Break before/after the meeting
    public int MinNoticeHours { get; set; } = 24; // Minimum lead time
    public int MaxDaysInAdvance { get; set; } = 60; // How far can you book?
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}