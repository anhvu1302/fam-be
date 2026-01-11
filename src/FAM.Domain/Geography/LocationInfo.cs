namespace FAM.Domain.Geography;

/// <summary>
/// Detailed location information
/// </summary>
public class LocationInfo
{
    public string? City { get; set; }
    public string? AreaCode { get; set; }
    public string? OrganizationName { get; set; }
    public string? Country { get; set; }
    public string? CountryCode3 { get; set; }
    public string? ContinentCode { get; set; }
    public int? Asn { get; set; }
    public string? Region { get; set; }
    public string? Ip { get; set; }
    public string? Longitude { get; set; }
    public int? Accuracy { get; set; }
    public string? CountryCode { get; set; }
    public string? Timezone { get; set; }
    public string? Latitude { get; set; }
    public string? Organization { get; set; }

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
