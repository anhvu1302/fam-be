using FAM.Domain.Common;
using FAM.Domain.Organizations;
using FAM.Domain.Users;

namespace FAM.Domain.Authorization;

/// <summary>
/// User role assignment at organization node
/// </summary>
public class UserNodeRole : Entity
{
    public long UserId { get; private set; }
    public User User { get; private set; } = null!;
    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public long RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public DateTime? StartAt { get; private set; }
    public DateTime? EndAt { get; private set; }

    private UserNodeRole()
    {
    }

    public static UserNodeRole Create(User user, OrgNode node, Role role, DateTime? startAt = null,
        DateTime? endAt = null)
    {
        if (endAt.HasValue && startAt.HasValue && endAt.Value <= startAt.Value)
            throw new DomainException("End date must be after start date");

        return new UserNodeRole
        {
            UserId = user.Id,
            User = user,
            NodeId = node.Id,
            Node = node,
            RoleId = role.Id,
            Role = role,
            StartAt = startAt,
            EndAt = endAt
        };
    }

    public void UpdateDates(DateTime? startAt, DateTime? endAt)
    {
        if (endAt.HasValue && startAt.HasValue && endAt.Value <= startAt.Value)
            throw new DomainException("End date must be after start date");

        StartAt = startAt;
        EndAt = endAt;
    }
}