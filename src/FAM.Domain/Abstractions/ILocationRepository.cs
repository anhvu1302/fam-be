using FAM.Domain.Locations;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository cho Location
/// </summary>
public interface ILocationRepository : IRepository<Location>
{
    Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> GetChildrenAsync(int parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Location>> GetRootLocationsAsync(CancellationToken cancellationToken = default);
    Task<Location?> GetWithChildrenAsync(int id, CancellationToken cancellationToken = default);
    Task<string> GenerateFullPathAsync(int locationId, CancellationToken cancellationToken = default);
}
