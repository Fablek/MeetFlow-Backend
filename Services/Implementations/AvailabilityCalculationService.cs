using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.Availability;
using MeetFlow_Backend.Services.Interfaces;

namespace MeetFlow_Backend.Services.Implementations;

public class AvailabilityCalculationService : IAvailabilityCalculationService
{
    private readonly ApplicationDbContext _context;
    private readonly IGoogleCalendarService _googleCalendarService;

    public AvailabilityCalculationService(
        ApplicationDbContext context,
        IGoogleCalendarService googleCalendarService)
    {
        _context = context;
        _googleCalendarService = googleCalendarService;
    }

    public async Task<DayAvailabilityResponse?> GetAvailableSlotsAsync(
        string username,
        string slug,
        DateOnly date)
    {
        // 1, Get user by username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return null;
        }
        
        // 2. Get event type
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.Slug == slug && e.IsActive);

        if (eventType == null)
        {
            return null;
        }
        
        // 3. Check if date is within allowed range
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var minDate = today.AddDays((int)Math.Ceiling(eventType.MinNoticeHours / 24.0));
        var maxDate = today.AddDays(eventType.MaxDaysInAdvance);

        if (date < minDate || date > maxDate)
        {
            return new DayAvailabilityResponse
            {
                Date = date.ToString("yyyy-MM-dd"),
                AvailableSlots = new List<AvailableSlotDto>(),
                EventType = new EventTypeInfo
                {
                    Name = eventType.Name,
                    Slug = eventType.Slug,
                    DurationMinutes = eventType.DurationMinutes,
                    Location = eventType.Location,
                    Description = eventType.Description
                }
            };
        }
        
        // 4. Get day of week
        var dayOfWeek = (int)date.DayOfWeek;
        if (dayOfWeek == 0) dayOfWeek = 7; // Sunday = 7
        
        // 5. Get availability for this day
        var availability = await _context.Availabilities
            .Where(a => a.UserId == user.Id && a.DayOfWeek == dayOfWeek)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        if (!availability.Any())
        {
            return new DayAvailabilityResponse
            {
                Date = date.ToString("yyyy-MM-dd"),
                AvailableSlots = new List<AvailableSlotDto>(),
                EventType = new EventTypeInfo
                {
                    Name = eventType.Name,
                    Slug = eventType.Slug,
                    DurationMinutes = eventType.DurationMinutes,
                    Location = eventType.Location,
                    Description = eventType.Description
                }
            };
        }
        
        // 6. Get busy slots from Google Calendar (if connected)
        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);
        
        var busySlots = await _googleCalendarService.GetBusySlotsAsync(
            user.Id,
            startOfDay,
            endOfDay
        );
        
        // 7. Calculate available slots
        var availableSlots = new List<AvailableSlotDto>();

        foreach (var slot in availability)
        {
            var slotStart = date.ToDateTime(slot.StartTime);
            var slotEnd = date.ToDateTime(slot.EndTime);
            
            // Generate slots every 15 minutes (or based on duration)
            var slotDuration = TimeSpan.FromMinutes(eventType.DurationMinutes);
            var bufferDuration = TimeSpan.FromMinutes(eventType.BufferMinutes);
            
            var currentTime = slotStart;

            while (currentTime.Add(slotDuration) <= slotEnd)
            {
                var potentialSlotEnd = currentTime.Add(slotDuration);
                
                // Check if this slot conflicts with any busy slots
                var hasConflict = busySlots?.Any(busy =>
                    // Overlap check (with buffer)
                    currentTime.Subtract(bufferDuration) < busy.End &&
                    potentialSlotEnd.Add(bufferDuration) > busy.Start
                ) ?? false;
                
                if (!hasConflict)
                {
                    availableSlots.Add(new AvailableSlotDto
                    {
                        Start = currentTime,
                        End = potentialSlotEnd
                    });
                }
                
                // Move to next slot (15 min increments)
                currentTime = currentTime.AddMinutes(15);
            }
        }
        
        return new DayAvailabilityResponse
        {
            Date = date.ToString("yyyy-MM-dd"),
            AvailableSlots = availableSlots,
            EventType = new EventTypeInfo
            {
                Name = eventType.Name,
                Slug = eventType.Slug,
                DurationMinutes = eventType.DurationMinutes,
                Location = eventType.Location,
                Description = eventType.Description
            }
        };
    }
}