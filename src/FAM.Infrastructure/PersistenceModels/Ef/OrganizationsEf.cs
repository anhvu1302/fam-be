using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for OrgNode
/// </summary>
[Table("org_nodes")]
public class OrgNodeEf : BaseEntityEf
{
    public int Type { get; set; } // OrgNodeType as int
    public string Name { get; set; } = string.Empty;

    public long? ParentId { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }


    // Navigation properties
    public OrgNodeEf? Parent { get; set; }
    public ICollection<OrgNodeEf> Children { get; set; } = new List<OrgNodeEf>();

    // Details navigation
    public CompanyDetailsEf? CompanyDetails { get; set; }
    public DepartmentDetailsEf? DepartmentDetails { get; set; }

    // Authorization navigation
    public ICollection<UserNodeRoleEf> UserNodeRoles { get; set; } = new List<UserNodeRoleEf>();
    public ICollection<ResourceEf> Resources { get; set; } = new List<ResourceEf>();
}

/// <summary>
/// EF persistence model for CompanyDetails
/// </summary>
[Table("company_details")]
public class CompanyDetailsEf : BaseEntityEf
{
    public long NodeId { get; set; }
    public string? TaxCode { get; set; }
    public string? Domain { get; set; }
    public string? Address { get; set; }
    public DateTime? EstablishedOn { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }
    public OrgNodeEf? Node { get; set; }
}

/// <summary>
/// EF persistence model for DepartmentDetails
/// </summary>
[Table("department_details")]
public class DepartmentDetailsEf : BaseEntityEf
{
    public long NodeId { get; set; }
    public string? CostCenter { get; set; }
    public int? Headcount { get; set; }
    public decimal? BudgetYear { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }
    public OrgNodeEf? Node { get; set; }
}
