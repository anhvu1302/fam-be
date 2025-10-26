using FAM.Domain.Companies;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface cho Company aggregate
/// </summary>
public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Company>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsNameTakenAsync(string name, long? excludeCompanyId = null, CancellationToken cancellationToken = default);
}