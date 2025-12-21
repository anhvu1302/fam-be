using FAM.Domain.Organizations;
using FAM.Infrastructure.Common.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

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

        var hasOrgs = await _dbContext.OrgNodes.AnyAsync(o => !o.IsDeleted, cancellationToken);

        if (hasOrgs)
        {
            LogInfo("Organizations already exist, skipping seed");
            return;
        }

        LogInfo("Seeding organizations...");

        // Create root company
        var companyDetails = CompanyDetails.Create(
            taxCode: "0123456789",
            domain: "fam-corp.com",
            address: "123 Business District, Ho Chi Minh City, Vietnam",
            establishedOn: new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var company = OrgNode.CreateCompany("FAM Corporation", companyDetails);

        await _dbContext.OrgNodes.AddAsync(company, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create departments
        var departments = new List<OrgNode>
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

        // Create sub-departments for IT
        OrgNode itDept = departments[0];
        var itSubDepts = new List<OrgNode>
        {
            CreateDepartment("Software Development", itDept.Id, "IT-DEV-001", 15, 300000),
            CreateDepartment("Infrastructure", itDept.Id, "IT-INFRA-001", 5, 100000),
            CreateDepartment("IT Support", itDept.Id, "IT-SUP-001", 5, 100000)
        };

        await _dbContext.OrgNodes.AddRangeAsync(itSubDepts, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create sub-departments for Sales
        OrgNode salesDept = departments[4];
        var salesSubDepts = new List<OrgNode>
        {
            CreateDepartment("Domestic Sales", salesDept.Id, "SALES-DOM-001", 12, 250000),
            CreateDepartment("International Sales", salesDept.Id, "SALES-INT-001", 8, 200000),
            CreateDepartment("Marketing", salesDept.Id, "MKT-001", 10, 150000)
        };

        await _dbContext.OrgNodes.AddRangeAsync(salesSubDepts, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var totalNodes = 1 + departments.Count + itSubDepts.Count + salesSubDepts.Count;
        LogInfo(
            $"Created {totalNodes} organization nodes (1 company, {departments.Count + itSubDepts.Count + salesSubDepts.Count} departments)");
    }

    private OrgNode CreateDepartment(string name, long parentId, string? costCenter = null, int? headcount = null, decimal? budgetYear = null)
    {
        var parent = _dbContext.OrgNodes.Find(parentId);
        if (parent == null)
            throw new InvalidOperationException($"Parent node with ID {parentId} not found");

        var details = DepartmentDetails.Create(costCenter, headcount, budgetYear);
        return OrgNode.CreateDepartment(name, details, parent);
    }

}
