using System.ComponentModel.DataAnnotations;

namespace MeetFlow_Backend.DTOs.Auth;

public class GoogleLoginRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public string RedirectUri { get; set; } = string.Empty;
}