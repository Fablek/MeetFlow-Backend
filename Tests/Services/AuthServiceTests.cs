using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.DTOs.Auth;
using MeetFlow_Backend.Services.Implementations;
using MeetFlow_Backend.Services.Interfaces;
using MeetFlow_Backend.Tests.Helpers;
using Xunit;

namespace MeetFlow_Backend.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;
    
    public AuthServiceTests()
    {
        _context = TestDbContextFactory.Create();

        _jwtServiceMock = new Mock<IJwtService>();
        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Models.User>()))
            .Returns("fake-jwt-token");

        _configurationMock = new Mock<IConfiguration>();

        _authService = new AuthService(_context, _jwtServiceMock.Object, _configurationMock.Object);
    }
    
    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
    
    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User",
            Username = "test-user"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(request.Email);
        result.User.FullName.Should().Be(request.FullName);
        result.User.Username.Should().Be(request.Username.ToLower());
        result.Token.Should().Be("fake-jwt-token");

        var userInDb = await _context.Users.FindAsync(result.User.Id);
        userInDb.Should().NotBeNull();
        userInDb!.PasswordHash.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var existingUser = new Models.User
        {
            Email = "existing@example.com",
            Username = "existing",
            FullName = "Existing User",
            PasswordHash = "hash"
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePass123!",
            FullName = "New User",
            Username = "new-user"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => 
            await _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldThrowException()
    {
        // Arrange
        var existingUser = new Models.User
        {
            Email = "user1@example.com",
            Username = "test-user",
            FullName = "User 1",
            PasswordHash = "hash"
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "user2@example.com",
            Password = "SecurePass123!",
            FullName = "User 2",
            Username = "test-user"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => 
            await _authService.RegisterAsync(request));
    }
    
    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User",
            Username = "test-user"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        var userInDb = await _context.Users.FindAsync(result.User.Id);
        userInDb!.PasswordHash.Should().NotBe(request.Password);
        userInDb.PasswordHash.Should().StartWith("$2a$");
    }
    
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User",
            Username = "test-user"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(loginRequest.Email);
        result.Token.Should().Be("fake-jwt-token");
    }
    
    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SecurePass123!"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => 
            await _authService.LoginAsync(loginRequest));
        
        exception.Message.Should().Contain("Invalid credentials");
    }
    
    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "CorrectPassword123!",
            FullName = "Test User",
            Username = "test-user"
        };
        await _authService.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () => 
            await _authService.LoginAsync(loginRequest));
        
        exception.Message.Should().Contain("Invalid credentials");
    }
}