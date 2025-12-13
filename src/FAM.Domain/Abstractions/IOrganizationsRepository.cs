using FAM.Domain.Organizations;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface for OrgNode entity
/// </summary>
public interface IOrgNodeRepository : IRepository<OrgNode>
{
    Task<OrgNode?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrgNode>> GetByParentIdAsync(long? parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrgNode>> GetByTypeAsync(OrgNodeType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrgNode>> GetHierarchyAsync(long rootNodeId, CancellationToken cancellationToken = default);
    Task<bool> HasChildrenAsync(long nodeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for CompanyDetails entity
/// </summary>
public interface ICompanyDetailsRepository : IRepository<CompanyDetails>
{
    Task<CompanyDetails?> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);
    Task<CompanyDetails?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default);
    Task<CompanyDetails?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for DepartmentDetails entity
/// </summary>
public interface IDepartmentDetailsRepository : IRepository<DepartmentDetails>
{
    Task<DepartmentDetails?> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);
    Task<DepartmentDetails?> GetByCostCenterAsync(string costCenter, CancellationToken cancellationToken = default);

    Task<IEnumerable<DepartmentDetails>> GetByParentNodeIdAsync(long parentNodeId,
        CancellationToken cancellationToken = default);
}
