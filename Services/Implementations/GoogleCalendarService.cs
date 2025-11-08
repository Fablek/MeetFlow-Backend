using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using MeetFlow_Backend.Data;
using MeetFlow_Backend.Models;
using MeetFlow_Backend.Services.Interfaces;
using MeetFlow_Backend.DTOs.Google;

namespace MeetFlow_Backend.Services.Implementations;

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public GoogleCalendarService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> GetAuthorizationUrlAsync(Guid userId)
    {
        var clientId = _configuration["GoogleOAuth:ClientId"];
        var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
        var redirectUri = _configuration["GoogleOAuth:CalendarRedirectUri"];

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
        
        var authUrl = flow.CreateAuthorizationCodeRequest(redirectUri)
            .Build()
            .ToString();
        
        // Add state parameter with userId for security
        authUrl += $"&state={userId}&access_type=offline&prompt=consent";

        return authUrl;
    }

    public async Task<GoogleIntegration> HandleCallbackAsync(string code, Guid userId)
    {
        var clientId = _configuration["GoogleOAuth:ClientId"];
        var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
        var redirectUri = _configuration["GoogleOAuth:CalendarRedirectUri"];

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
        
        var token = await flow.ExchangeCodeForTokenAsync(
            userId.ToString(),
            code,
            redirectUri,
            CancellationToken.None
        );
        
        // Get user info from calendar
        var credential = new UserCredential(flow, userId.ToString(), token);
        var service = new CalendarService(new Google.Apis.Services.BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
        
        var calendarList = await service.CalendarList.List().ExecuteAsync();
        var primaryCalendar = calendarList.Items.FirstOrDefault(c => c.Primary == true);

        // Save or update integration
        var integration = await _context.GoogleIntegrations
            .FirstOrDefaultAsync(g => g.UserId == userId);

        if (integration == null)
        {
            integration = new GoogleIntegration
            {
                UserId = userId,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken ?? string.Empty,
                TokenExpiresAt = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600),
                GoogleEmail = primaryCalendar?.Summary ?? "unknown@gmail.com",
                CalendarId = primaryCalendar?.Id ?? "primary",
                IsActive = true
            };
            _context.GoogleIntegrations.Add(integration);
        }
        else
        {
            integration.AccessToken = token.AccessToken;
            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                integration.RefreshToken = token.RefreshToken;
            }
            integration.TokenExpiresAt = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600);
            integration.GoogleEmail = primaryCalendar?.Summary ?? integration.GoogleEmail;
            integration.CalendarId = primaryCalendar?.Id ?? integration.CalendarId;
            integration.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return integration;
    }

    public async Task<bool> RefreshTokenAsync(Guid userId)
    {
        var integration = await _context.GoogleIntegrations
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive);
        
        if (integration == null || string.IsNullOrEmpty(integration.RefreshToken))
        {
            return false;
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

        try
        {
            var token = await flow.RefreshTokenAsync(
                userId.ToString(),
                integration.RefreshToken,
                CancellationToken.None
            );

            integration.AccessToken = token.AccessToken;
            integration.TokenExpiresAt = token.IssuedUtc.AddSeconds(token.ExpiresInSeconds ?? 3600);
            integration.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<GoogleIntegrationResponse> GetIntegrationStatusAsync(Guid userId)
    {
        var integration = await _context.GoogleIntegrations
            .FirstOrDefaultAsync(g => g.UserId == userId);

        if (integration == null || !integration.IsActive)
        {
            return new GoogleIntegrationResponse
            {
                IsConnected = false
            };
        }

        return new GoogleIntegrationResponse
        {
            IsConnected = true,
            GoogleEmail = integration.GoogleEmail,
            CalendarId = integration.CalendarId,
            ConnectedAt = integration.CreatedAt
        };
    }
    
    public async Task<bool> DisconnectAsync(Guid userId)
    {
        var integration = await _context.GoogleIntegrations
            .FirstOrDefaultAsync(g => g.UserId == userId);

        if (integration == null)
        {
            return false;
        }

        _context.GoogleIntegrations.Remove(integration);
        await _context.SaveChangesAsync();
    
        return true;
    }
}