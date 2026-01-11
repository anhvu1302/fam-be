using FAM.Domain.Organizations;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Seeders;

/// <summary>
/// Seeds initial organization structure (company, departments)
/// </summary>
public class OrganizationSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public OrganizationSeeder(PostgreSqlDbContext dbContext, ILogger<OrganizationSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251129140006_OrganizationSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Checking for existing organizations...");

        bool hasOrgs = await _dbContext.OrgNodes.AnyAsync(o => !o.IsDeleted, cancellationToken);

        if (hasOrgs)
        {
            LogInfo("Organizations already exist, skipping seed");
            return;
        }

        LogInfo("Seeding organizations...");

        // Create root company
        CompanyDetails companyDetails = CompanyDetails.Create(
            "0123456789",
            "fam-corp.com",
            "123 Business District, Ho Chi Minh City, Vietnam",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        OrgNode company = OrgNode.CreateCompany("FAM Corporation", companyDetails);

        await _dbContext.OrgNodes.AddAsync(company, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create departments
        List<OrgNode> departments = new()
        {
            CreateDepartment("IT Department", company.Id, "IT-001", 25, 500000),
            CreateDepartment("Human Resources", company.Id, "HR-001", 10, 200000),
            CreateDepartment("Finance", company.Id, "FIN-001", 15, 300000),
            CreateDepartment("Operations", company.Id, "OPS-001", 50, 800000),
            CreateDepartment("Sales & Marketing", company.Id, "SALES-001", 30, 600000),
            CreateDepartment("Administration", company.Id, "ADM-001", 8, 150000)
        };

        await _dbContext.OrgNodes.AddRangeAsync(departments, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        int totalNodes = 1 + departments.Count;
        LogInfo($"Created {totalNodes} organization nodes (1 company, {departments.Count} departments)");
    }

    private OrgNode CreateDepartment(string name, long parentId, string? costCenter = null, int? headcount = null,
        decimal? budgetYear = null)
    {
        OrgNode? parent = _dbContext.OrgNodes.Find(parentId);
        if (parent == null)
        {
            throw new InvalidOperationException($"Parent node with ID {parentId} not found");
        }

        DepartmentDetails details = DepartmentDetails.Create(costCenter, headcount, budgetYear);
        return OrgNode.CreateDepartment(name, details, parent);
    }
}
