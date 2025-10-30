using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Username can only contain lowercase letters, numbers and hyphens")]
    public string Username { get; set; } = string.Empty;
}