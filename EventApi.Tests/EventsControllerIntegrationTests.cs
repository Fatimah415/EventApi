using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using EventApi.Data;
using EventApi.Models;
using EventApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EventApi.Tests;

public class EventsControllerIntegrationTests : IDisposable
{
    private const string JwtKey = "EventApi.IntegrationTests.SigningKey.MustBeAtLeast32Bytes!";
    private const string JwtIssuer = "EventApi.Tests";
    private const string JwtAudience = "EventApi.Tests.Client";
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public EventsControllerIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetEvents_ReturnsSeededEventsOrderedByDate()
    {
        var response = await _client.GetAsync("/api/Events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        events.Should().NotBeNull();
        events!.Should().HaveCountGreaterThanOrEqualTo(10);
        events.Select(item => item.Id).Should()
            .Equal(events.OrderBy(item => item.EventDate).Select(item => item.Id));
        events.Should().OnlyContain(item => !string.IsNullOrWhiteSpace(item.CategoryName));
    }

    [Fact]
    public async Task GetEventById_ExistingEvent_ReturnsBody()
    {
        var response = await _client.GetAsync("/api/Events/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ev = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        ev.Should().NotBeNull();
        ev!.Should().BeEquivalentTo(new
        {
            Id = 1,
            Title = ".NET Conf 2026",
            CategoryName = "Conference"
        });
    }

    [Fact]
    public async Task GetEventById_MissingEvent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/Events/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().ContainEquivalentOf("not found");
    }

    [Fact]
    public async Task CreateEvent_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/Events", CreateValidForm());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateEvent_AsStandardUser_ReturnsForbidden()
    {
        AuthorizeAs(Roles.User);

        var response = await _client.PostAsync("/api/Events", CreateValidForm());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidForm_ReturnsBadRequest()
    {
        AuthorizeAs(Roles.Admin);
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(DateTime.UtcNow.AddDays(30).ToString("O")), "EventDate");
        form.Add(new StringContent("Lahore"), "Location");
        form.Add(new StringContent("1"), "CategoryId");
        form.Add(new StringContent("1"), "UserId");

        var response = await _client.PostAsync("/api/Events", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEvent_AsAdmin_ReturnsCreatedAndPersistsEvent()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.PostAsync("/api/Events", CreateValidForm("Integration Created"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Integration Created");
        response.Headers.Location!.OriginalString.Should().EndWith($"/{created.Id}");

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persisted = await db.Events.AsNoTracking().SingleAsync(item => item.Id == created.Id);
        persisted.Title.Should().Be("Integration Created");
        persisted.UserId.Should().Be(1);
    }

    [Fact]
    public async Task UpdateEvent_AsAdmin_ReturnsNoContentAndPersistsChanges()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.PutAsync("/api/Events/1", CreateValidUpdateForm("Updated Conference"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updated = await db.Events.AsNoTracking().SingleAsync(item => item.Id == 1);
        updated.Should().BeEquivalentTo(new
        {
            Title = "Updated Conference",
            Location = "Karachi",
            CategoryId = 2
        });
    }

    [Fact]
    public async Task UpdateEvent_MissingEvent_ReturnsNotFound()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.PutAsync("/api/Events/99999", CreateValidUpdateForm());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEvent_AsAdmin_ReturnsNoContentAndRemovesEvent()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.DeleteAsync("/api/Events/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Events.AnyAsync(item => item.Id == 1)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEvent_MissingEvent_ReturnsNotFound()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.DeleteAsync("/api/Events/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private void AuthorizeAs(string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "tester@eventapi.test"),
            new Claim(ClaimTypes.Role, role)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            JwtIssuer,
            JwtAudience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token));
    }

    private static MultipartFormDataContent CreateValidForm(string title = "New Event")
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(title), "Title");
        form.Add(new StringContent("Created by an integration test"), "Description");
        form.Add(new StringContent(DateTime.UtcNow.AddDays(30).ToString("O")), "EventDate");
        form.Add(new StringContent("Lahore"), "Location");
        form.Add(new StringContent("1"), "CategoryId");
        form.Add(new StringContent("1"), "UserId");
        return form;
    }

    private static MultipartFormDataContent CreateValidUpdateForm(string title = "Updated Event")
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(title), "Title");
        form.Add(new StringContent("Updated by an integration test"), "Description");
        form.Add(new StringContent(DateTime.UtcNow.AddDays(60).ToString("O")), "EventDate");
        form.Add(new StringContent("Karachi"), "Location");
        form.Add(new StringContent("2"), "CategoryId");
        return form;
    }
}
