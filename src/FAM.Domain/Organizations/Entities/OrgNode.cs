using FAM.Domain.Authorization;
using FAM.Domain.Common;

namespace FAM.Domain.Organizations;

/// <summary>
/// Organizational node in the hierarchy: Company -> Department -> Group
/// </summary>
public class OrgNode : AggregateRoot
{
    public OrgNodeType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public long? ParentId { get; private set; }
    public OrgNode? Parent { get; private set; }
    public ICollection<OrgNode> Children { get; private set; } = new List<OrgNode>();

    // Navigation for different types
    public CompanyDetails? CompanyDetails { get; private set; }
    public DepartmentDetails? DepartmentDetails { get; private set; }

    // Authorization
    public ICollection<UserNodeRole> UserNodeRoles { get; set; } = new List<UserNodeRole>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();

    private OrgNode()
    {
    }

    public static OrgNode CreateCompany(string name, CompanyDetails details)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("OrgNode name cannot be empty");

        var node = new OrgNode
        {
            Type = OrgNodeType.Company,
            Name = name.Trim(),
            ParentId = null
        };

        details.SetNode(node);
        node.CompanyDetails = details;

        return node;
    }

    public static OrgNode CreateDepartment(string name, DepartmentDetails details, OrgNode parent)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("OrgNode name cannot be empty");

        if (parent.Type != OrgNodeType.Company)
            throw new DomainException("Department parent must be a Company");

        var node = new OrgNode
        {
            Type = OrgNodeType.Department,
            Name = name.Trim(),
            ParentId = parent.Id,
            Parent = parent
        };

        details.SetNode(node);
        node.DepartmentDetails = details;

        return node;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("OrgNode name cannot be empty");

        Name = name.Trim();
    }
}