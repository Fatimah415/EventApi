using System.Net;
using System.Net.Http.Json;
using EventApi.Data;
using EventApi.Models;
using EventApi.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventApi.Tests;

public class EventsControllerIntegrationTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public EventsControllerIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetEvents_ReturnsSeededEvents()
    {
        // Act
        var response = await _client.GetAsync("/api/Events");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        Assert.NotNull(events);
        Assert.True(events.Count >= 10, "Should contain at least the 10 seeded EF Core HasData events");
    }

    [Fact]
    public async Task GetEvents_IncludesDynamicallyAddedEvents()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category = db.Categories.First();
            db.Events.Add(new Event
            {
                Title = "Integration Test Event",
                Description = "Testing the API",
                Location = "Online",
                EventDate = DateTime.UtcNow.AddDays(1),
                Category = category
            });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/Events");

        // Assert
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        Assert.NotNull(events);
        Assert.Contains(events, e => e.Title == "Integration Test Event");
    }

    [Fact]
    public async Task CreateEvent_ReturnsUnauthorizedOrRedirect_WhenNoTokenProvided()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("New Event"), "Title");

        // Act
        // Because the default scheme is Cookie, a missing token on an API 
        // endpoint might return 302 Redirect (to Login) or 401 Unauthorized depending on configuration.
        // We just assert it does NOT succeed.
        var response = await _client.PostAsync("/api/Events", content);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.RedirectMethod);
    }
}
