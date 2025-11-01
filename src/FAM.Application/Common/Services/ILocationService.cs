namespace FAM.Application.Common.Services;

/// <summary>
/// Service for detecting location from IP address
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Get location information from IP address
    /// </summary>
    /// <param name="ipAddress">IP address to lookup</param>
    /// <returns>Location string (e.g., "Hanoi, Vietnam" or "New York, US")</returns>
    Task<string?> GetLocationFromIpAsync(string ipAddress);

    /// <summary>
    /// Get detailed location information from IP address
    /// </summary>
    Task<LocationInfo?> GetDetailedLocationFromIpAsync(string ipAddress);
}

/// <summary>
/// Detailed location information
/// </summary>
public class LocationInfo
{
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? Region { get; set; }
    public string? RegionName { get; set; }
    public string? City { get; set; }
    public string? Zip { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    public string? Isp { get; set; }

    /// <summary>
    /// Get formatted location string
    /// </summary>
    public string GetFormattedLocation()
    {
        if (!string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(Country))
        {
            return $"{City}, {Country}";
        }
        if (!string.IsNullOrEmpty(Country))
        {
            return Country;
        }
        return "Unknown";
    }
}
