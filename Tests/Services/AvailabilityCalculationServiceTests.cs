using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Implementations;
using MeetFlow_Backend.Services.Interfaces;
using MeetFlow_Backend.DTOs.Google;
using MeetFlow_Backend.Tests.Helpers;

namespace MeetFlow_Backend.Tests.Services;

public class AvailabilityCalculationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IGoogleCalendarService> _mockGoogleCalendar;
    private readonly AvailabilityCalculationService _service;
    
    public AvailabilityCalculationServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _mockGoogleCalendar = new Mock<IGoogleCalendarService>();
        _service = new AvailabilityCalculationService(_context, _mockGoogleCalendar.Object);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }

    // Helper methods to get future dates
    private static DateOnly GetNextMonday()
    {
        var today = DateTime.UtcNow;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        
        return DateOnly.FromDateTime(today.AddDays(daysUntilMonday + 7));
    }

    private static DateOnly GetNextSunday()
    {
        var today = DateTime.UtcNow;
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0) daysUntilSunday = 7;
        
        return DateOnly.FromDateTime(today.AddDays(daysUntilSunday + 7));
    }

    [Fact]
    public async Task GetAvailableSlots_EmptyCalendar_ReturnsAllSlots()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var slug = "test-meeting";
        var date = GetNextMonday();

        // Create user
        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        // Create event type
        var eventType = new EventType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Meeting",
            Slug = slug,
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = true,
            MinNoticeHours = 0,
            MaxDaysInAdvance = 60,
            BufferMinutes = 0
        };
        _context.EventTypes.Add(eventType);

        // Create availability: Monday 9-17
        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DayOfWeek = 1, // Monday
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            CreatedAt = DateTime.UtcNow
        };
        _context.Availabilities.Add(availability);
        await _context.SaveChangesAsync();

        // Mock Google Calendar - no busy slots
        _mockGoogleCalendar
            .Setup(x => x.GetBusySlotsAsync(
                userId, 
                It.IsAny<DateTime>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<List<string>>()))
            .ReturnsAsync(new List<BusySlotDto>());

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, slug, date);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableSlots.Should().NotBeEmpty();
        result.AvailableSlots.Should().HaveCountGreaterThan(10);
        
        // First slot should be at 9:00
        var firstSlot = result.AvailableSlots.First();
        firstSlot.Start.Hour.Should().Be(9);
        firstSlot.Start.Minute.Should().Be(0);
        
        // Event type info should be present
        result.EventType.Name.Should().Be("Test Meeting");
        result.EventType.DurationMinutes.Should().Be(30);
    }

    [Fact]
    public async Task GetAvailableSlots_WithBusySlot_ExcludesConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var slug = "test-meeting";
        var date = GetNextMonday();

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        var eventType = new EventType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Meeting",
            Slug = slug,
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = true,
            MinNoticeHours = 0,
            MaxDaysInAdvance = 60,
            BufferMinutes = 0
        };
        _context.EventTypes.Add(eventType);

        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DayOfWeek = 1,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0),
            CreatedAt = DateTime.UtcNow
        };
        _context.Availabilities.Add(availability);
        await _context.SaveChangesAsync();

        // Mock Google Calendar - busy slot at 10:00-10:30
        var busySlot = new BusySlotDto
        {
            Start = date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc),
            End = date.ToDateTime(new TimeOnly(10, 30), DateTimeKind.Utc)
        };

        _mockGoogleCalendar
            .Setup(x => x.GetBusySlotsAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new List<BusySlotDto> { busySlot });

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, slug, date);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableSlots.Should().NotBeEmpty();
        
        // Should NOT contain 10:00 slot
        result.AvailableSlots.Should().NotContain(s => 
            s.Start.Hour == 10 && s.Start.Minute == 0);
        
        // Should contain 9:00
        result.AvailableSlots.Should().Contain(s => 
            s.Start.Hour == 9 && s.Start.Minute == 0);
    }

    [Fact]
    public async Task GetAvailableSlots_NoAvailability_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var slug = "test-meeting";
        var date = GetNextMonday();

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        var eventType = new EventType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Meeting",
            Slug = slug,
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = true,
            MinNoticeHours = 0,
            MaxDaysInAdvance = 60,
            BufferMinutes = 0
        };
        _context.EventTypes.Add(eventType);
        
        // NO availability added!
        await _context.SaveChangesAsync();

        _mockGoogleCalendar
            .Setup(x => x.GetBusySlotsAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new List<BusySlotDto>());

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, slug, date);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableSlots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableSlots_InvalidUsername_ReturnsNull()
    {
        // Arrange
        var date = GetNextMonday();

        // Act
        var result = await _service.GetAvailableSlotsAsync("nonexistent-user", "some-slug", date);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableSlots_InvalidSlug_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var date = GetNextMonday();

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, "nonexistent-slug", date);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableSlots_InactiveEventType_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var slug = "inactive-meeting";
        var date = GetNextMonday();

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        var eventType = new EventType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Inactive Meeting",
            Slug = slug,
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = false, // INACTIVE!
            MinNoticeHours = 0,
            MaxDaysInAdvance = 60,
            BufferMinutes = 0
        };
        _context.EventTypes.Add(eventType);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, slug, date);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableSlots_DateTooSoon_ReturnsEmptySlots()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var slug = "test-meeting";
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        var eventType = new EventType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Meeting",
            Slug = slug,
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = true,
            MinNoticeHours = 72, // 3 days notice required!
            MaxDaysInAdvance = 60,
            BufferMinutes = 0
        };
        _context.EventTypes.Add(eventType);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, slug, tomorrow);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableSlots.Should().BeEmpty(); // Too soon!
    }

    [Fact]
    public async Task GetAvailableSlots_DateTooFar_ReturnsEmptySlots()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var slug = "test-meeting";
        var farFuture = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100));

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        var eventType = new EventType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Meeting",
            Slug = slug,
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = true,
            MinNoticeHours = 0,
            MaxDaysInAdvance = 60, // Only 60 days allowed!
            BufferMinutes = 0
        };
        _context.EventTypes.Add(eventType);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, slug, farFuture);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableSlots.Should().BeEmpty(); // Too far!
    }

    [Fact]
    public async Task GetAvailableSlots_Sunday_HandlesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "test-user";
        var slug = "test-meeting";
        var sunday = GetNextSunday();

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        var eventType = new EventType
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Meeting",
            Slug = slug,
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = true,
            MinNoticeHours = 0,
            MaxDaysInAdvance = 60,
            BufferMinutes = 0
        };
        _context.EventTypes.Add(eventType);

        // Sunday availability (DayOfWeek = 7 in your system)
        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DayOfWeek = 7, // Sunday
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(14, 0),
            CreatedAt = DateTime.UtcNow
        };
        _context.Availabilities.Add(availability);
        await _context.SaveChangesAsync();

        _mockGoogleCalendar
            .Setup(x => x.GetBusySlotsAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new List<BusySlotDto>());

        // Act
        var result = await _service.GetAvailableSlotsAsync(username, slug, sunday);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableSlots.Should().NotBeEmpty();
        result.AvailableSlots.First().Start.Hour.Should().Be(10);
    }
}