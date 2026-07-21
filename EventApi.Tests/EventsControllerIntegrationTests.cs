using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using EventApi.Data;
using EventApi.Models;
using EventApi.Tests.Infrastructure;
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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await response.Content.ReadFromJsonAsync<List<EventResponseDto>>();
        Assert.NotNull(events);
        Assert.True(events.Count >= 10);
        Assert.Equal(events.OrderBy(item => item.EventDate).Select(item => item.Id), events.Select(item => item.Id));
        Assert.All(events, item => Assert.False(string.IsNullOrWhiteSpace(item.CategoryName)));
    }

    [Fact]
    public async Task GetEventById_ExistingEvent_ReturnsBody()
    {
        var response = await _client.GetAsync("/api/Events/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var ev = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.NotNull(ev);
        Assert.Equal(1, ev.Id);
        Assert.Equal(".NET Conf 2026", ev.Title);
        Assert.Equal("Conference", ev.CategoryName);
    }

    [Fact]
    public async Task GetEventById_MissingEvent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/Events/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("not found", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateEvent_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/Events", CreateValidForm());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsStandardUser_ReturnsForbidden()
    {
        AuthorizeAs(Roles.User);

        var response = await _client.PostAsync("/api/Events", CreateValidForm());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsAdmin_ReturnsCreatedAndPersistsEvent()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.PostAsync("/api/Events", CreateValidForm("Integration Created"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<EventResponseDto>();
        Assert.NotNull(created);
        Assert.Equal("Integration Created", created.Title);
        Assert.EndsWith($"/{created.Id}", response.Headers.Location!.OriginalString);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persisted = await db.Events.AsNoTracking().SingleAsync(item => item.Id == created.Id);
        Assert.Equal("Integration Created", persisted.Title);
        Assert.Equal(1, persisted.UserId);
    }

    [Fact]
    public async Task UpdateEvent_AsAdmin_ReturnsNoContentAndPersistsChanges()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.PutAsync("/api/Events/1", CreateValidUpdateForm("Updated Conference"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updated = await db.Events.AsNoTracking().SingleAsync(item => item.Id == 1);
        Assert.Equal("Updated Conference", updated.Title);
        Assert.Equal("Karachi", updated.Location);
        Assert.Equal(2, updated.CategoryId);
    }

    [Fact]
    public async Task UpdateEvent_MissingEvent_ReturnsNotFound()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.PutAsync("/api/Events/99999", CreateValidUpdateForm());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsAdmin_ReturnsNoContentAndRemovesEvent()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.DeleteAsync("/api/Events/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Events.AnyAsync(item => item.Id == 1));
    }

    [Fact]
    public async Task DeleteEvent_MissingEvent_ReturnsNotFound()
    {
        AuthorizeAs(Roles.Admin);

        var response = await _client.DeleteAsync("/api/Events/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
