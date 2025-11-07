using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetFlow_Backend.Models;

public class Availability
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    
    [Required]
    [Range(0, 6)] // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    public int DayOfWeek { get; set; }
    
    [Required]
    public TimeOnly StartTime { get; set; } // for example 09:00
    
    [Required]
    public TimeOnly EndTime { get; set; } // for example 17:00
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}