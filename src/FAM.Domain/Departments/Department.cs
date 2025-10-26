using FAM.Domain.Common;

namespace FAM.Domain.Departments;

/// <summary>
/// Phòng ban
/// </summary>
public class Department : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }

    // Navigation properties
    public ICollection<Assets.Asset> Assets { get; set; } = new List<Assets.Asset>();

    private Department() { }

    public static Department Create(string name, string? code = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Department name cannot be empty");

        return new Department
        {
            Name = name,
            Code = code,
            Description = description
        };
    }
}
