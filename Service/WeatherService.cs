using System.Text.Json;
using EventApi.Models;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace EventApi.Services;

public interface IWeatherService
{
    Task<WeatherDto> GetWeatherAsync(string location, CancellationToken cancellationToken = default);
}

public class WeatherService : IWeatherService
{
    private const string UnavailableMessage = "Weather information is currently unavailable.";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<WeatherDto> GetWeatherAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location must not be empty.", nameof(location));

        var apiKey = _configuration["OpenWeatherMap:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Weather request skipped because the provider API key is not configured.");
            return Unavailable();
        }

        try
        {
            var encodedLocation = Uri.EscapeDataString(location);
            var requestUri =
                $"https://api.openweathermap.org/data/2.5/weather?q={encodedLocation}&appid={Uri.EscapeDataString(apiKey)}&units=metric";

            _logger.LogInformation("Fetching weather for location {Location}.", location);
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Weather provider returned status code {StatusCode} for location {Location}.",
                    (int)response.StatusCode,
                    location);
                return Unavailable();
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var raw = JsonSerializer.Deserialize<OpenWeatherMapResponse>(json);
            if (raw is null ||
                string.IsNullOrWhiteSpace(raw.CityName) ||
                raw.Main is null ||
                raw.Wind is null ||
                raw.Weather.Count == 0)
            {
                _logger.LogWarning(
                    "Weather provider returned a malformed response for location {Location}.",
                    location);
                return Unavailable();
            }

            return new WeatherDto
            {
                Available = true,
                City = raw.CityName,
                TemperatureCelsius = raw.Main.Temperature,
                FeelsLikeCelsius = raw.Main.FeelsLike,
                Humidity = raw.Main.Humidity,
                Description = raw.Weather[0].Description,
                WindSpeedMetersPerSec = raw.Wind.Speed
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Weather provider request timed out for location {Location}.", location);
            return Unavailable();
        }
        catch (TimeoutRejectedException)
        {
            _logger.LogWarning("Weather provider request timed out for location {Location}.", location);
            return Unavailable();
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("Weather provider request failed for location {Location}.", location);
            return Unavailable();
        }
        catch (JsonException)
        {
            _logger.LogWarning("Weather provider returned invalid JSON for location {Location}.", location);
            return Unavailable();
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning("Weather provider circuit is open.");
            return Unavailable();
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("Weather provider response could not be processed.");
            return Unavailable();
        }
    }

    private static WeatherDto Unavailable() => new()
    {
        Available = false,
        Message = UnavailableMessage
    };
}
