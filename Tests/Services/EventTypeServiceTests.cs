using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Implementations;
using MeetFlow_Backend.DTOs.EventType;
using MeetFlow_Backend.Tests.Helpers;

namespace MeetFlow_Backend.Tests.Services;

public class EventTypeServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateEventTypeAsync_WithValidData_ShouldCreateEventType()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();

        var request = new CreateEventTypeRequest
        {
            Name = "30 Minute Meeting",
            Slug = null, // Should auto-generate
            DurationMinutes = 30,
            Description = "Quick consultation",
            Location = "Online",
            Color = "#3b82f6",
            BufferMinutes = 5,
            MinNoticeHours = 24,
            MaxDaysInAdvance = 60
        };

        // Act
        var result = await service.CreateEventTypeAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("30 Minute Meeting");
        result.Slug.Should().Be("30-minute-meeting"); // Auto-generated
        result.DurationMinutes.Should().Be(30);
        result.IsActive.Should().BeTrue();
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateEventTypeAsync_WithCustomSlug_ShouldUseProvidedSlug()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();

        var request = new CreateEventTypeRequest
        {
            Name = "My Meeting",
            Slug = "custom-slug",
            DurationMinutes = 45,
            Location = "Online",
            Color = "#3b82f6"
        };

        // Act
        var result = await service.CreateEventTypeAsync(userId, request);

        // Assert
        result.Slug.Should().Be("custom-slug");
    }

    [Fact]
    public async Task CreateEventTypeAsync_WithDuplicateSlug_ShouldAddSuffix()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();

        var request1 = new CreateEventTypeRequest
        {
            Name = "Meeting",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };

        var request2 = new CreateEventTypeRequest
        {
            Name = "Meeting",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };

        // Act
        var result1 = await service.CreateEventTypeAsync(userId, request1);
        var result2 = await service.CreateEventTypeAsync(userId, request2);

        // Assert
        result1.Slug.Should().Be("meeting");
        result2.Slug.Should().Be("meeting-2"); // Auto-suffixed
    }

    [Fact]
    public async Task CreateEventTypeAsync_WithSpecialCharacters_ShouldSanitizeSlug()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();

        var request = new CreateEventTypeRequest
        {
            Name = "Café & Coffee ☕ Talk!",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };

        // Act
        var result = await service.CreateEventTypeAsync(userId, request);

        // Assert
        result.Slug.Should().Be("caf-coffee-talk"); // Sanitized (é removed)
    }

    [Fact]
    public async Task GetUserEventTypesAsync_ShouldReturnAllUserEventTypes()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        // Add event types for user
        context.EventTypes.AddRange(
            new EventType
            {
                UserId = userId,
                Name = "Meeting 1",
                Slug = "meeting-1",
                DurationMinutes = 30,
                Location = "Online",
                Color = "#3b82f6"
            },
            new EventType
            {
                UserId = userId,
                Name = "Meeting 2",
                Slug = "meeting-2",
                DurationMinutes = 60,
                Location = "Online",
                Color = "#10b981"
            },
            new EventType
            {
                UserId = otherUserId,
                Name = "Other Meeting",
                Slug = "other-meeting",
                DurationMinutes = 30,
                Location = "Online",
                Color = "#ef4444"
            }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserEventTypesAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(et => et.UserId == userId);
        result.Select(et => et.Name).Should().Contain(new[] { "Meeting 1", "Meeting 2" });
    }

    [Fact]
    public async Task GetEventTypeByIdAsync_WithValidId_ShouldReturnEventType()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();

        var eventType = new EventType
        {
            UserId = userId,
            Name = "Test Meeting",
            Slug = "test-meeting",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };
        context.EventTypes.Add(eventType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventTypeByIdAsync(eventType.Id, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Meeting");
        result.Slug.Should().Be("test-meeting");
    }

    [Fact]
    public async Task GetEventTypeByIdAsync_WithWrongUser_ShouldReturnNull()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var eventType = new EventType
        {
            UserId = userId,
            Name = "Test Meeting",
            Slug = "test-meeting",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };
        context.EventTypes.Add(eventType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventTypeByIdAsync(eventType.Id, otherUserId);

        // Assert
        result.Should().BeNull(); // Wrong user
    }

    [Fact]
    public async Task UpdateEventTypeAsync_WithValidData_ShouldUpdateEventType()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();

        var eventType = new EventType
        {
            UserId = userId,
            Name = "Original Name",
            Slug = "original-slug",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6",
            IsActive = true
        };
        context.EventTypes.Add(eventType);
        await context.SaveChangesAsync();

        var updateRequest = new UpdateEventTypeRequest
        {
            Name = "Updated Name",
            Slug = "updated-slug",
            DurationMinutes = 60,
            Description = "New description",
            Location = "Phone",
            Color = "#10b981",
            IsActive = false,
            BufferMinutes = 10,
            MinNoticeHours = 48,
            MaxDaysInAdvance = 90
        };

        // Act
        var result = await service.UpdateEventTypeAsync(eventType.Id, userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Slug.Should().Be("updated-slug");
        result.DurationMinutes.Should().Be(60);
        result.Description.Should().Be("New description");
        result.Location.Should().Be("Phone");
        result.Color.Should().Be("#10b981");
        result.IsActive.Should().BeFalse();
        result.BufferMinutes.Should().Be(10);
    }

    [Fact]
    public async Task UpdateEventTypeAsync_WithWrongUser_ShouldReturnNull()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var eventType = new EventType
        {
            UserId = userId,
            Name = "Test Meeting",
            Slug = "test-meeting",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };
        context.EventTypes.Add(eventType);
        await context.SaveChangesAsync();

        var updateRequest = new UpdateEventTypeRequest
        {
            Name = "Hacked Name",
            Slug = "hacked",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#ef4444"
        };

        // Act
        var result = await service.UpdateEventTypeAsync(eventType.Id, otherUserId, updateRequest);

        // Assert
        result.Should().BeNull(); // Wrong user - no access
    }

    [Fact]
    public async Task DeleteEventTypeAsync_WithValidId_ShouldDeleteEventType()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();

        var eventType = new EventType
        {
            UserId = userId,
            Name = "Test Meeting",
            Slug = "test-meeting",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };
        context.EventTypes.Add(eventType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteEventTypeAsync(eventType.Id, userId);

        // Assert
        result.Should().BeTrue();
        
        var deletedEventType = await context.EventTypes.FindAsync(eventType.Id);
        deletedEventType.Should().BeNull(); // Should be deleted
    }

    [Fact]
    public async Task DeleteEventTypeAsync_WithWrongUser_ShouldReturnFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var eventType = new EventType
        {
            UserId = userId,
            Name = "Test Meeting",
            Slug = "test-meeting",
            DurationMinutes = 30,
            Location = "Online",
            Color = "#3b82f6"
        };
        context.EventTypes.Add(eventType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteEventTypeAsync(eventType.Id, otherUserId);

        // Assert
        result.Should().BeFalse(); // Wrong user
        
        var existingEventType = await context.EventTypes.FindAsync(eventType.Id);
        existingEventType.Should().NotBeNull(); // Should still exist
    }

    [Fact]
    public async Task DeleteEventTypeAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new EventTypeService(context);
        var userId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await service.DeleteEventTypeAsync(nonExistentId, userId);

        // Assert
        result.Should().BeFalse();
    }
}