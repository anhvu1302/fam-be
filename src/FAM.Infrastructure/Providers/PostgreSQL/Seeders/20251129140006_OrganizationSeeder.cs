using FAM.Domain.Organizations;
using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.PersistenceModels.Ef;
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
        var company = new OrgNodeEf
        {
            Type = (int)OrgNodeType.Company,
            Name = "FAM Corporation",
            ParentId = null,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.OrgNodes.AddAsync(company, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create company details
        var companyDetails = new CompanyDetailsEf
        {
            NodeId = company.Id,
            TaxCode = "0123456789",
            Domain = "fam-corp.com",
            Address = "123 Business District, Ho Chi Minh City, Vietnam",
            EstablishedOn = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.CompanyDetails.AddAsync(companyDetails, cancellationToken);

        // Create departments
        var departments = new List<OrgNodeEf>
        {
            CreateDepartment("IT Department", company.Id),
            CreateDepartment("Human Resources", company.Id),
            CreateDepartment("Finance", company.Id),
            CreateDepartment("Operations", company.Id),
            CreateDepartment("Sales & Marketing", company.Id),
            CreateDepartment("Administration", company.Id)
        };

        await _dbContext.OrgNodes.AddRangeAsync(departments, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create department details
        var deptDetails = new List<DepartmentDetailsEf>
        {
            CreateDepartmentDetails(departments[0].Id, "IT-001", 25, 500000), // IT
            CreateDepartmentDetails(departments[1].Id, "HR-001", 10, 200000), // HR
            CreateDepartmentDetails(departments[2].Id, "FIN-001", 15, 300000), // Finance
            CreateDepartmentDetails(departments[3].Id, "OPS-001", 50, 800000), // Operations
            CreateDepartmentDetails(departments[4].Id, "SALES-001", 30, 600000), // Sales
            CreateDepartmentDetails(departments[5].Id, "ADM-001", 8, 150000) // Admin
        };

        await _dbContext.DepartmentDetails.AddRangeAsync(deptDetails, cancellationToken);

        // Create sub-departments for IT
        var itDept = departments[0];
        var itSubDepts = new List<OrgNodeEf>
        {
            CreateDepartment("Software Development", itDept.Id),
            CreateDepartment("Infrastructure", itDept.Id),
            CreateDepartment("IT Support", itDept.Id)
        };

        await _dbContext.OrgNodes.AddRangeAsync(itSubDepts, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var itSubDeptDetails = new List<DepartmentDetailsEf>
        {
            CreateDepartmentDetails(itSubDepts[0].Id, "IT-DEV-001", 15, 300000),
            CreateDepartmentDetails(itSubDepts[1].Id, "IT-INFRA-001", 5, 100000),
            CreateDepartmentDetails(itSubDepts[2].Id, "IT-SUP-001", 5, 100000)
        };

        await _dbContext.DepartmentDetails.AddRangeAsync(itSubDeptDetails, cancellationToken);

        // Create sub-departments for Sales
        var salesDept = departments[4];
        var salesSubDepts = new List<OrgNodeEf>
        {
            CreateDepartment("Domestic Sales", salesDept.Id),
            CreateDepartment("International Sales", salesDept.Id),
            CreateDepartment("Marketing", salesDept.Id)
        };

        await _dbContext.OrgNodes.AddRangeAsync(salesSubDepts, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var salesSubDeptDetails = new List<DepartmentDetailsEf>
        {
            CreateDepartmentDetails(salesSubDepts[0].Id, "SALES-DOM-001", 12, 250000),
            CreateDepartmentDetails(salesSubDepts[1].Id, "SALES-INT-001", 8, 200000),
            CreateDepartmentDetails(salesSubDepts[2].Id, "MKT-001", 10, 150000)
        };

        await _dbContext.DepartmentDetails.AddRangeAsync(salesSubDeptDetails, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var totalNodes = 1 + departments.Count + itSubDepts.Count + salesSubDepts.Count;
        LogInfo(
            $"Created {totalNodes} organization nodes (1 company, {departments.Count + itSubDepts.Count + salesSubDepts.Count} departments)");
    }

    private static OrgNodeEf CreateDepartment(string name, long parentId)
    {
        return new OrgNodeEf
        {
            Type = (int)OrgNodeType.Department,
            Name = name,
            ParentId = parentId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static DepartmentDetailsEf CreateDepartmentDetails(long nodeId, string costCenter, int? headcount = null,
        decimal? budgetYear = null)
    {
        return new DepartmentDetailsEf
        {
            NodeId = nodeId,
            CostCenter = costCenter,
            Headcount = headcount,
            BudgetYear = budgetYear,
            CreatedAt = DateTime.UtcNow
        };
    }
}