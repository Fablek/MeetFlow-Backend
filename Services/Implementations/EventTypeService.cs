using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.EventType;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Interfaces;

namespace MeetFlow_Backend.Services.Implementations;

public class EventTypeService : IEventTypeService
{
    private readonly ApplicationDbContext _context;

    public EventTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventTypeResponse> CreateEventTypeAsync(Guid userId, CreateEventTypeRequest request)
    {
        // Generate slug if not provided
        string slug;
        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            // Auto-generate from name
            var baseSlug = GenerateSlugFromName(request.Name);
            slug = await GenerateUniqueSlugAsync(userId, baseSlug);
        }
        else
        {
            // Use provided slug (lowercase)
            slug = request.Slug.ToLower();
        
            // Check if already exists
            var slugExists = await _context.EventTypes
                .AnyAsync(e => e.UserId == userId && e.Slug == slug);

            if (slugExists)
            {
                throw new Exception("Event type with this slug already exists");
            }
        }
        
        var eventType = new EventType
        {
            UserId = userId,
            Name = request.Name,
            Slug = slug,
            DurationMinutes = request.DurationMinutes,
            Description = request.Description,
            Location = request.Location,
            LocationDetails = request.LocationDetails,
            Color = request.Color,
            BufferMinutes = request.BufferMinutes,
            MinNoticeHours = request.MinNoticeHours,
            MaxDaysInAdvance = request.MaxDaysInAdvance,
            IsActive = true
        };
        
        _context.EventTypes.Add(eventType);
        await _context.SaveChangesAsync();

        return MapToResponse(eventType);
    }
    
    public async Task<List<EventTypeResponse>> GetUserEventTypesAsync(Guid userId)
    {
        var eventTypes = await _context.EventTypes
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return eventTypes.Select(MapToResponse).ToList();
    }
    
    public async Task<EventTypeResponse?> GetEventTypeByIdAsync(Guid eventTypeId, Guid userId)
    {
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(e => e.Id == eventTypeId && e.UserId == userId);

        return eventType == null ? null : MapToResponse(eventType);
    }
    
    public async Task<EventTypeResponse?> UpdateEventTypeAsync(Guid eventTypeId, Guid userId, UpdateEventTypeRequest request)
    {
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(e => e.Id == eventTypeId && e.UserId == userId);

        if (eventType == null)
        {
            return null;
        }

        // Check if new slug conflicts with existing one
        if (!string.IsNullOrEmpty(request.Slug) && request.Slug != eventType.Slug)
        {
            var slugExists = await _context.EventTypes
                .AnyAsync(e => e.UserId == userId && e.Slug == request.Slug.ToLower() && e.Id != eventTypeId);

            if (slugExists)
            {
                throw new Exception("Event type with this slug already exists");
            }

            eventType.Slug = request.Slug.ToLower();
        }

        // Update only provided fields
        if (request.Name != null) eventType.Name = request.Name;
        if (request.DurationMinutes.HasValue) eventType.DurationMinutes = request.DurationMinutes.Value;
        if (request.Description != null) eventType.Description = request.Description;
        if (request.Location != null) eventType.Location = request.Location;
        if (request.LocationDetails != null) eventType.LocationDetails = request.LocationDetails;
        if (request.Color != null) eventType.Color = request.Color;
        if (request.IsActive.HasValue) eventType.IsActive = request.IsActive.Value;
        if (request.BufferMinutes.HasValue) eventType.BufferMinutes = request.BufferMinutes.Value;
        if (request.MinNoticeHours.HasValue) eventType.MinNoticeHours = request.MinNoticeHours.Value;
        if (request.MaxDaysInAdvance.HasValue) eventType.MaxDaysInAdvance = request.MaxDaysInAdvance.Value;

        eventType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(eventType);
    }
    
    public async Task<bool> DeleteEventTypeAsync(Guid eventTypeId, Guid userId)
    {
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(e => e.Id == eventTypeId && e.UserId == userId);

        if (eventType == null)
        {
            return false;
        }

        _context.EventTypes.Remove(eventType);
        await _context.SaveChangesAsync();

        return true;
    }
    
    private static EventTypeResponse MapToResponse(EventType eventType)
    {
        return new EventTypeResponse
        {
            Id = eventType.Id,
            UserId = eventType.UserId,
            Name = eventType.Name,
            Slug = eventType.Slug,
            DurationMinutes = eventType.DurationMinutes,
            Description = eventType.Description,
            Location = eventType.Location,
            LocationDetails = eventType.LocationDetails,
            Color = eventType.Color,
            IsActive = eventType.IsActive,
            BufferMinutes = eventType.BufferMinutes,
            MinNoticeHours = eventType.MinNoticeHours,
            MaxDaysInAdvance = eventType.MaxDaysInAdvance,
            CreatedAt = eventType.CreatedAt,
            UpdatedAt = eventType.UpdatedAt
        };
    }

    /// <summary>
    /// Generate a URL-friendly slug from a string
    /// </summary>
    private static string GenerateSlugFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exception("Name cannot be empty");
        }
        
        // Convert to lowercase and replace spaces with hyphens
        var slug = name.ToLowerInvariant()
            .Trim()
            .Replace(" ", "-")
            .Replace("_", "-");
        
        // Remove invalid characters (keep only lowercase letters, numbers, and hyphens)
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove duplicate hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        // Remove leading/trailing hyphens
        slug = slug.Trim('-');
        
        // Limit length
        if (slug.Length > 100)
        {
            slug = slug.Substring(0, 100).TrimEnd('-');
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new Exception("Could not generate valid slug from name");
        }

        return slug;
    }
    
    /// <summary>
    /// Generate a unique slug for a user by adding a suffix if needed
    /// </summary>
    private async Task<string> GenerateUniqueSlugAsync(Guid userId, string baseSlug)
    {
        var slug = baseSlug;
        var counter = 2;

        // Check if slug exists
        while (await _context.EventTypes.AnyAsync(e => e.UserId == userId && e.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;

            // Safety limit
            if (counter > 100)
            {
                throw new Exception("Could not generate unique slug");
            }
        }

        return slug;
    }
}