using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.Availability;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Interfaces;

namespace MeetFlow_Backend.Services.Implementations;

public class AvailabilityService : IAvailabilityService
{
    private readonly ApplicationDbContext _context;

    public AvailabilityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AvailabilityResponse> CreateAvailabilityAsync(Guid userId, CreateAvailabilityRequest request)
    {
        // Validate time range
        if (request.StartTime >= request.EndTime)
        {
            throw new Exception("Start time must be before end time");
        }
        
        // Check for overlapping availability on the same day
        var hasOverlap = await _context.Availabilities
            .AnyAsync(a => a.UserId == userId
                           && a.DayOfWeek == request.DayOfWeek
                           && ((request.StartTime >= a.StartTime && request.StartTime < a.EndTime)
                               || (request.EndTime > a.StartTime && request.EndTime <= a.EndTime)
                               || (request.StartTime <= a.StartTime && request.EndTime >= a.EndTime)));

        if (hasOverlap)
        {
            throw new Exception("This time slot overlaps with existing availability");
        }
        
        var availability = new Models.Availability
        {
            UserId = userId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };
        
        _context.Availabilities.Add(availability);
        await _context.SaveChangesAsync();

        return MapToResponse(availability);
    }

    public async Task<List<AvailabilityResponse>> GetUserAvailabilitiesAsync(Guid userId)
    {
        var availabilities = await _context.Availabilities
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync();
        
        return availabilities.Select(MapToResponse).ToList();
    }

    public async Task<bool> DeleteAvailabilityAsync(Guid availabilityId, Guid userId)
    {
        var availability = await _context.Availabilities
            .FirstOrDefaultAsync(a => a.Id == availabilityId && a.UserId == userId);

        if (availability == null)
        {
            return false;
        }

        _context.Availabilities.Remove(availability);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> BulkSetAvailabilityAsync(Guid userId, List<CreateAvailabilityRequest> availabilities)
    {
        // Remove all existing availabilities for user
        var existingAvailabilities = await _context.Availabilities
            .Where(a => a.UserId == userId)
            .ToListAsync();

        _context.Availabilities.RemoveRange(existingAvailabilities);
        
        // Add new availabilities
        foreach (var request in availabilities)
        {
            if (request.StartTime >= request.EndTime)
            {
                throw new Exception($"Invalid time range for {GetDayName(request.DayOfWeek)}");
            }

            var availability = new Models.Availability
            {
                UserId = userId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            _context.Availabilities.Add(availability);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    private static AvailabilityResponse MapToResponse(Models.Availability availability)
    {
        return new AvailabilityResponse
        {
            Id = availability.Id,
            UserId = availability.UserId,
            DayOfWeek = availability.DayOfWeek,
            DayOfWeekName = GetDayName(availability.DayOfWeek),
            StartTime = availability.StartTime,
            EndTime = availability.EndTime,
            CreatedAt = availability.CreatedAt,
            UpdatedAt = availability.UpdatedAt
        };
    }
    
    private static string GetDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Sunday",
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            _ => "Unknown"
        };
    }
}