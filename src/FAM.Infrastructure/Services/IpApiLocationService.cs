using System.Net;
using System.Text.Json;

using FAM.Application.Common.Services;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services;

/// <summary>
/// Location service implementation using ip-api.com
/// Free tier: 45 requests per minute
/// For production, consider using paid services like MaxMind GeoIP2 or IPinfo
/// </summary>
public class IpApiLocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IpApiLocationService> _logger;
    private const string ApiBaseUrl = "http://ip-api.com/json/";

    public IpApiLocationService(HttpClient httpClient, ILogger<IpApiLocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(5); // Short timeout
    }

    public async Task<string?> GetLocationFromIpAsync(string ipAddress)
    {
        try
        {
            // Skip for local/private IPs
            if (IsLocalOrPrivateIp(ipAddress)) return "Local Network";

            LocationInfo? locationInfo = await GetDetailedLocationFromIpAsync(ipAddress);
            return locationInfo?.GetFormattedLocation();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get location for IP: {IpAddress}", ipAddress);
            return null;
        }
    }

    public async Task<LocationInfo?> GetDetailedLocationFromIpAsync(string ipAddress)
    {
        try
        {
            // Skip for local/private IPs
            if (IsLocalOrPrivateIp(ipAddress))
                return new LocationInfo
                {
                    Country = "Local",
                    City = "Local Network"
                };

            var url =
                $"{ApiBaseUrl}{ipAddress}?fields=status,message,country,countryCode,region,regionName,city,zip,lat,lon,timezone,isp";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("IP API returned status code: {StatusCode} for IP: {IpAddress}",
                    response.StatusCode, ipAddress);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            IpApiResponse? apiResponse = JsonSerializer.Deserialize<IpApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Status != "success")
            {
                _logger.LogWarning("IP API failed for IP: {IpAddress}, Message: {Message}",
                    ipAddress, apiResponse?.Message);
                return null;
            }

            return new LocationInfo
            {
                Country = apiResponse.Country,
                CountryCode = apiResponse.CountryCode,
                Region = apiResponse.Region,
                RegionName = apiResponse.RegionName,
                City = apiResponse.City,
                Zip = apiResponse.Zip,
                Latitude = apiResponse.Lat,
                Longitude = apiResponse.Lon,
                Timezone = apiResponse.Timezone,
                Isp = apiResponse.Isp
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get detailed location for IP: {IpAddress}", ipAddress);
            return null;
        }
    }

    private bool IsLocalOrPrivateIp(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
            return true;

        if (ipAddress == "127.0.0.1" || ipAddress == "::1" || ipAddress == "localhost")
            return true;

        if (!IPAddress.TryParse(ipAddress, out IPAddress? ip))
            return true;

        var bytes = ip.GetAddressBytes();

        // IPv4 private ranges
        if (bytes.Length == 4)
            return bytes[0] == 10
                   || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                   || (bytes[0] == 192 && bytes[1] == 168)
                   || bytes[0] == 127;

        // IPv6 local
        return ip.IsIPv6LinkLocal;
    }

    private class IpApiResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? Region { get; set; }
        public string? RegionName { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string? Timezone { get; set; }
        public string? Isp { get; set; }
    }
}
