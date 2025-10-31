using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.User;

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Username can only contain lowercase letters, numbers and hyphens")]
    public string Username { get; set; } = string.Empty;
}