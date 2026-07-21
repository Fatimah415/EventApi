using System.Text.Json;
using EventApi.Models;

namespace EventApi.Services;

public interface IWeatherService
{
    /// <summary>
    /// Fetches the current weather for the given city/location from
    /// OpenWeatherMap and returns a clean WeatherDto.
    /// Throws HttpRequestException on transient failures (handled by Polly).
    /// Throws KeyNotFoundException when the location is not found (404).
    /// </summary>
    Task<WeatherDto> GetWeatherAsync(string location, CancellationToken cancellationToken = default);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _httpClient    = httpClient;
        _configuration = configuration;
        _logger        = logger;
    }

    public async Task<WeatherDto> GetWeatherAsync(string location, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location must not be empty.", nameof(location));

        // Read the API key from configuration (appsettings.json → OpenWeatherMap:ApiKey).
        var apiKey = _configuration["OpenWeatherMap:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenWeatherMap:ApiKey is not configured in appsettings.json.");

        // Build the request URL.
        // units=metric → temperature in Celsius, wind speed in m/s.
        var encodedLocation = Uri.EscapeDataString(location);
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={encodedLocation}&appid={apiKey}&units=metric";

        _logger.LogInformation("Fetching weather for location: {Location}", location);

        // Send the request. Polly policies (Retry, Circuit Breaker, Timeout)
        // are applied automatically by IHttpClientFactory — no manual policy
        // invocation needed here.
        var response = await _httpClient.GetAsync(url, cancellationToken);

        // Handle non-success status codes.
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException(
                $"Weather data not found for location: '{location}'. " +
                "Verify the city name is correct.");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("OpenWeatherMap API key is invalid or expired.");
            throw new InvalidOperationException(
                "OpenWeatherMap API key is invalid. Check appsettings.json.");
        }

        // For any other non-success code, let HttpRequestException propagate
        // so Polly can retry if the status is transient (5xx).
        response.EnsureSuccessStatusCode();

        // Deserialise the raw JSON into the internal model.
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var raw = JsonSerializer.Deserialize<OpenWeatherMapResponse>(json)
            ?? throw new InvalidOperationException(
                "Failed to deserialise the OpenWeatherMap response.");

        // Map the internal model to the public WeatherDto.
        return new WeatherDto
        {
            City                 = raw.CityName,
            TemperatureCelsius   = raw.Main.Temperature,
            FeelsLikeCelsius     = raw.Main.FeelsLike,
            Humidity             = raw.Main.Humidity,
            Description          = raw.Weather.FirstOrDefault()?.Description ?? "N/A",
            WindSpeedMetersPerSec = raw.Wind.Speed
        };
    }
}
