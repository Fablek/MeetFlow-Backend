using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetFlow_Backend.Models;

public class GoogleIntegration
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))] 
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(2000)]
    public string AccessToken { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string RefreshToken { get; set; } = string.Empty;
    
    [Required]
    public DateTime TokenExpiresAt { get; set; }
    
    [Required]
    [MaxLength(250)]
    [EmailAddress]
    public string GoogleEmail { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? CalendarId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}