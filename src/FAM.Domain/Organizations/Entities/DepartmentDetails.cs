using FAM.Domain.Common;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Organizations;

/// <summary>
/// Department-specific details
/// </summary>
public class DepartmentDetails : Entity
{
    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public CostCenter? CostCenter { get; private set; }
    public int? Headcount { get; private set; }
    public decimal? BudgetYear { get; private set; }

    private DepartmentDetails() { }

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
}