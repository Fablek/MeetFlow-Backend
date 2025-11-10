using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.Booking;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MeetFlow_Backend.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly IGoogleCalendarService _googleCalendarService;
    private readonly IConfiguration _configuration;

    public BookingService(
        ApplicationDbContext context,
        IGoogleCalendarService googleCalendarService,
        IConfiguration configuration)
    {
        _context = context;
        _googleCalendarService = googleCalendarService;
        _configuration = configuration;
    }

    public async Task<BookingConfirmationResponse?> CreateBookingAsync(
        string username,
        string slug,
        CreateBookingRequest request)
    {
        // 1. Get user and event type
        var user = await _context.Users
            .Include(u => u.GoogleIntegration)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return null;
        }
        
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.Slug == slug && e.IsActive);

        if (eventType == null)
        {
            return null;
        }
        
        // 2. Calculate end time
        var endTime = request.StartTime.AddMinutes(eventType.DurationMinutes);
        
        // 3. Validate: Check if slot is available
        var isAvailable = await IsSlotAvailableAsync(user.Id, request.StartTime, endTime);
        if (!isAvailable)
        {
            throw new InvalidOperationException("Selected time slot is no longer available");
        }
        
        // 4. Create booking in database
        var booking = new Models.Booking
        {
            EventTypeId = eventType.Id,
            GuestEmail = request.GuestEmail,
            GuestName = request.GuestName,
            GuestPhone = request.GuestPhone,
            Notes = request.Notes,
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        
        // 5. Create event in Google Calendar (if connected)
        string? googleEventId = null;
        if (user.GoogleIntegration != null && user.GoogleIntegration.IsActive)
        {
            try
            {
                googleEventId = await CreateGoogleCalendarEventAsync(
                    user,
                    eventType,
                    booking
                );
            }
            catch (Exception ex)
            {
                // Log error but don't fail the booking
                Console.WriteLine($"Failed to create Google Calendar event: {ex.Message}");
            }
        }
        
        // 6. Return confirmation
        return new BookingConfirmationResponse
        {
            BookingId = booking.Id,
            GuestName = booking.GuestName,
            GuestEmail = booking.GuestEmail,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            EventTypeName = eventType.Name,
            Location = eventType.Location,
            LocationDetails = eventType.LocationDetails,
            DurationMinutes = eventType.DurationMinutes,
            Status = booking.Status,
            Message = "Booking confirmed! You will receive a confirmation email shortly.",
            GoogleCalendarEventId = googleEventId
        };
    }

    private async Task<bool> IsSlotAvailableAsync(Guid userId, DateTime startTime, DateTime endTime)
    {
        // Check for existing bookings that overlap
        var hasConflict = await _context.Bookings
            .AnyAsync(b =>
                b.EventType.UserId == userId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartTime < endTime &&
                b.EndTime > startTime
            );

        if (hasConflict)
        {
            return false;
        }
        
        // Check Google Calendar busy slots
        var busySlots = await _googleCalendarService.GetBusySlotsAsync(
            userId,
            startTime.Date,
            startTime.Date.AddDays(1)
        );
        
        if (busySlots != null)
        {
            var hasGoogleConflict = busySlots.Any(busy =>
                startTime < busy.End && endTime > busy.Start
            );

            if (hasGoogleConflict)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<string?> CreateGoogleCalendarEventAsync(
        User user,
        EventType eventType,
        Models.Booking booking)
    {
        var integration = user.GoogleIntegration;
        if (integration == null)
        {
            return null;
        }
        
        // Refresh token if expired
        if (integration.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
        {
            await _googleCalendarService.RefreshTokenAsync(user.Id);
            integration = await _context.GoogleIntegrations
                .FirstOrDefaultAsync(g => g.UserId == user.Id);
        }

        if (integration == null)
        {
            return null;
        }
        
        var clientId = _configuration["GoogleOAuth:ClientId"];
        var clientSecret = _configuration["GoogleOAuth:ClientSecret"];

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[]
            {
                CalendarService.Scope.Calendar,
                CalendarService.Scope.CalendarEvents
            }
        });
        
        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
        {
            AccessToken = integration.AccessToken,
            RefreshToken = integration.RefreshToken,
            ExpiresInSeconds = (long)(integration.TokenExpiresAt - DateTime.UtcNow).TotalSeconds
        };
        
        var credential = new UserCredential(flow, user.Id.ToString(), token);

        var service = new CalendarService(new Google.Apis.Services.BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MeetFlow"
        });
        
        // Create event
        var calendarEvent = new Event
        {
            Summary = $"{eventType.Name} - {booking.GuestName}",
            Description = $"Meeting with {booking.GuestName} ({booking.GuestEmail})\n\n" +
                          $"{eventType.Description}\n\n" +
                          $"Notes: {booking.Notes ?? "None"}",
            Location = eventType.Location == "Online"
                ? eventType.LocationDetails ?? "Online Meeting"
                : eventType.LocationDetails,
            Start = new EventDateTime
            {
                DateTimeDateTimeOffset = booking.StartTime,
                TimeZone = "UTC"
            },
            End = new EventDateTime
            {
                DateTimeDateTimeOffset = booking.EndTime,
                TimeZone = "UTC"
            },
            Attendees = new[]
            {
                new EventAttendee
                {
                    Email = booking.GuestEmail,
                    DisplayName = booking.GuestName,
                    ResponseStatus = "accepted"
                }
            },
            Reminders = new Event.RemindersData
            {
                UseDefault = false,
                Overrides = new[]
                {
                    new EventReminder { Method = "email", Minutes = 24 * 60 },
                    new EventReminder { Method = "popup", Minutes = 30 }
                }
            }
        };
        
        var request = service.Events.Insert(calendarEvent, integration.CalendarId ?? "primary");
        request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;

        var createdEvent = await request.ExecuteAsync();

        return createdEvent.Id;
    }
}