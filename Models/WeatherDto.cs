using System.Text.Json.Serialization;

namespace EventApi.Models;

// ──────────────────────────────────────────────────────────────────────────────
// Public DTO — returned to API clients via the controller.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Clean, client-facing weather data for a specific event location.
/// </summary>
public class WeatherDto
{
    public bool Available { get; set; }
    public string? Message { get; set; }
    public string City { get; set; } = string.Empty;
    public double TemperatureCelsius { get; set; }
    public double FeelsLikeCelsius { get; set; }
    public int Humidity { get; set; }
    public string Description { get; set; } = string.Empty;
    public double WindSpeedMetersPerSec { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────
// Internal models — used ONLY by WeatherService to deserialise the raw
// OpenWeatherMap JSON response.  Never exposed to API clients.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Maps the top-level OpenWeatherMap "Current Weather" JSON response.
/// Only the fields we actually use are mapped; everything else is ignored
/// by System.Text.Json automatically.
/// </summary>
internal class OpenWeatherMapResponse
{
    [JsonPropertyName("name")]
    public string CityName { get; set; } = string.Empty;

    [JsonPropertyName("main")]
    public OpenWeatherMain Main { get; set; } = new();

    [JsonPropertyName("weather")]
    public List<OpenWeatherCondition> Weather { get; set; } = new();

    [JsonPropertyName("wind")]
    public OpenWeatherWind Wind { get; set; } = new();
}

internal class OpenWeatherMain
{
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

internal class OpenWeatherCondition
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

internal class OpenWeatherWind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}
