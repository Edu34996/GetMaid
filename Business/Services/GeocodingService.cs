using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Utils.Helpers;

namespace Business.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _http;
        public GeocodingService(HttpClient http) { _http = http; }

        public async Task<(double? Lat, double? Lon)> GeocodeAsync(string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                    return (null, null);

                var url = $"https://nominatim.openstreetmap.org/search?format=json&limit=1&q={Uri.EscapeDataString(address)}";
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("GetMaid/1.0");

                var response = await _http.GetFromJsonAsync<NominatimResult[]>(url);

                if (response is { Length: > 0 } &&
                    double.TryParse(response[0].Lat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                    double.TryParse(response[0].Lon, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                {
                    return (lat, lon);
                }

                return (null, null);
            }
            catch
            {
                // Never crash registration/profile update due to transient geocoding failures
                return (null, null);
            }
        }

        private record NominatimResult(
            [property: JsonPropertyName("lat")] string Lat,
            [property: JsonPropertyName("lon")] string Lon
        );
        
    }
}