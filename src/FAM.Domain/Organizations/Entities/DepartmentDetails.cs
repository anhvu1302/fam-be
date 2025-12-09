using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Organizations;

/// <summary>
/// Department-specific details  
/// Uses BasicAuditedEntity for basic audit trail
/// </summary>
public class DepartmentDetails : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasDeletionTime
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public CostCenter? CostCenter { get; private set; }
    public int? Headcount { get; private set; }
    public decimal? BudgetYear { get; private set; }

    private DepartmentDetails()
    {
    }

    public static DepartmentDetails Create(string? costCenter = null, int? headcount = null, decimal? budgetYear = null)
    {
        if (headcount.HasValue && headcount.Value < 0)
            throw new DomainException("Headcount cannot be negative");

        if (budgetYear.HasValue && budgetYear.Value < 0)
            throw new DomainException("Budget year cannot be negative");

        return new DepartmentDetails
        {
            CostCenter = costCenter != null ? CostCenter.Create(costCenter) : null,
            Headcount = headcount,
            BudgetYear = budgetYear
        };
    }

    internal void SetNode(OrgNode node)
    {
        NodeId = node.Id;
        Node = node;
    }

    public void Update(string? costCenter, int? headcount, decimal? budgetYear)
    {
        if (headcount.HasValue && headcount.Value < 0)
            throw new DomainException("Headcount cannot be negative");

        if (budgetYear.HasValue && budgetYear.Value < 0)
            throw new DomainException("Budget year cannot be negative");

        CostCenter = costCenter != null ? CostCenter.Create(costCenter) : null;
        Headcount = headcount;
        BudgetYear = budgetYear;
    }

    public virtual void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}