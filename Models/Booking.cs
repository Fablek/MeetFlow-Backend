using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetFlow_Backend.Models;

public class Booking
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid EventTypeId { get; set; }
    
    [ForeignKey(nameof(EventTypeId))]
    public EventType EventType { get; set; } = null!;
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
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
    
    [Required]
    public DateTime EndTime { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = BookingStatus.Confirmed;
    
    public string? CancellationReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class BookingStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";
    public const string Completed = "Completed";
}