using FAM.Domain.Common;
using FAM.Domain.Organizations;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Authorization;

/// <summary>
/// Business resource
/// </summary>
public class Resource : BaseEntity
{
    public ResourceType Type { get; private set; } = null!;
    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;

    private Resource()
    {
    }

    public static Resource Create(string type, OrgNode node, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Resource name cannot be empty");

        return new Resource
        {
            Type = ResourceType.Create(type),
            NodeId = node.Id,
            Node = node,
            Name = name.Trim()
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Resource name cannot be empty");

        Name = name.Trim();
    }
}