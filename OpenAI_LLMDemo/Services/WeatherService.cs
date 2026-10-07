using System.Text.Json;

namespace OpenAI_LLMDemo.Services;

// Uses the free Open-Meteo API (https://open-meteo.com). No API key needed.
public class WeatherService(HttpClient http)
{
    public async Task<string> GetWeatherAsync(string city)
    {
        JsonElement? place = await FindCityAsync(city);
        if (place is null)
        {
            return $"Could not find a city called {city}.";
        }

        // Keep the raw text ("12.97") so the URL always uses a dot, whatever the PC's language settings are.
        string lat = place.Value.GetProperty("latitude").GetRawText();
        string lon = place.Value.GetProperty("longitude").GetRawText();

        string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}" +
                     "&current=temperature_2m,relative_humidity_2m,wind_speed_10m,weather_code";

        JsonElement json = await http.GetFromJsonAsync<JsonElement>(url);
        JsonElement current = json.GetProperty("current");

        double temperature = current.GetProperty("temperature_2m").GetDouble();
        int humidity = current.GetProperty("relative_humidity_2m").GetInt32();
        double wind = current.GetProperty("wind_speed_10m").GetDouble();
        string sky = DescribeWeather(current.GetProperty("weather_code").GetInt32());

        return $"{city}: {temperature}°C, {sky}, humidity {humidity}%, wind {wind} km/h.";
    }

    public async Task<string> GetTimeAsync(string city)
    {
        JsonElement? place = await FindCityAsync(city);
        if (place is null)
        {
            return $"Could not find a city called {city}.";
        }

        string timeZoneId = place.Value.GetProperty("timezone").GetString()!; // e.g. "Europe/London"
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        return $"{city}: {localTime:dddd, hh:mm tt} ({timeZoneId}).";
    }

    // Turns a city name into latitude, longitude and time zone.
    private async Task<JsonElement?> FindCityAsync(string city)
    {
        string url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";

        JsonElement json = await http.GetFromJsonAsync<JsonElement>(url);
        if (!json.TryGetProperty("results", out JsonElement results))
        {
            return null;
        }

        return results[0];
    }

    // Open-Meteo returns a number (weather code) instead of text.
    private static string DescribeWeather(int code) => code switch
    {
        0 => "clear sky",
        <= 3 => "cloudy",
        <= 48 => "foggy",
        <= 67 => "rainy",
        <= 77 => "snowy",
        <= 82 => "rain showers",
        _ => "thunderstorm"
    };
}
