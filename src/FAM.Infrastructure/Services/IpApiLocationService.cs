using System.Text.Json;

using FAM.Application.Common.Services;
using FAM.Domain.Geography;
using FAM.Domain.ValueObjects;
using FAM.Infrastructure.Services.Dtos;

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
    private const string ApiBaseUrl = "https://get.geojs.io/v1/ip/geo/";

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
            var ip = IPAddress.Create(ipAddress);
            if (ip.IsLocalOrPrivate()) return "Local Network";

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
            var ip = IPAddress.Create(ipAddress);
            if (ip.IsLocalOrPrivate())
                return new LocationInfo
                {
                    Country = "Local",
                    City = "Local Network",
                    Ip = ipAddress
                };

            var url =
                $"{ApiBaseUrl}{ipAddress}.json";
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

            if (apiResponse == null)
            {
                _logger.LogWarning("IP API returned null response for IP: {IpAddress}", ipAddress);
                return null;
            }

            return new LocationInfo
            {
                City = apiResponse.City,
                AreaCode = apiResponse.AreaCode,
                OrganizationName = apiResponse.OrganizationName,
                Country = apiResponse.Country,
                CountryCode3 = apiResponse.CountryCode3,
                ContinentCode = apiResponse.ContinentCode,
                Asn = apiResponse.Asn,
                Region = apiResponse.Region,
                Ip = apiResponse.Ip,
                Longitude = apiResponse.Longitude,
                Accuracy = apiResponse.Accuracy,
                CountryCode = apiResponse.CountryCode,
                Timezone = apiResponse.Timezone,
                Latitude = apiResponse.Latitude,
                Organization = apiResponse.Organization
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get detailed location for IP: {IpAddress}", ipAddress);
            return null;
        }
    }
}
