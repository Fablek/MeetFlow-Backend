using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetFlow_Backend.DTOs.User;
using MeetFlow_Backend.Services.Interfaces;
using System.Security.Claims;

namespace MeetFlow_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        var userProfile = await _userService.GetCurrentUserAsync(userId);

        if (userProfile == null)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(userProfile);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileResponse>> UpdateCurrentUser([FromBody] UpdateProfileRequest request)
    {
        // Get User Id from JWT Token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        try
        {
            var userProfile = await _userService.UpdateProfileAsync(userId, request);

            if (userProfile == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}