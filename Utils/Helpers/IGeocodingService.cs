namespace Utils.Helpers;

public interface IGeocodingService
{
    Task<(double? Lat, double? Lon)> GeocodeAsync(string address);
}