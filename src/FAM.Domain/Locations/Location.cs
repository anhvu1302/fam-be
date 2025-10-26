using FAM.Domain.Common;

namespace FAM.Domain.Locations;

/// <summary>
/// Địa điểm/vị trí (hỗ trợ phân cấp hierarchical tree) - Aggregate Root
/// </summary>
public class Location : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public int? CompanyId { get; private set; }
    public string? Code { get; private set; }
    public int? ParentId { get; private set; }
    public string? Type { get; private set; }
    public string? FullPath { get; private set; }
    public string? PathIds { get; private set; }
    public int? CountryId { get; private set; }
    public string? Description { get; private set; }

    // Navigation properties
    public Companies.Company? Company { get; set; }
    public Geography.Country? Country { get; set; }
    public Location? Parent { get; set; }
    public ICollection<Location> Children { get; set; } = new List<Location>();
    public ICollection<Assets.Asset> Assets { get; set; } = new List<Assets.Asset>();

    private Location() { }

    public static Location Create(string name, string? code = null, int? companyId = null, int? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Location name cannot be empty");

        return new Location
        {
            Name = name,
            Code = code,
            CompanyId = companyId,
            ParentId = parentId
        };
    }

    public void Update(string name, string? description, int? countryId = null)
    {
        Name = name;
        Description = description;
        CountryId = countryId;
    }

    public void SetParent(int? parentId)
    {
        ParentId = parentId;
    }

    public void BuildPath(string fullPath, string pathIds)
    {
        FullPath = fullPath;
        PathIds = pathIds;
    }
}
