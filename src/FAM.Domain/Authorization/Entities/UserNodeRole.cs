using FAM.Domain.Common;
using FAM.Domain.Organizations;
using FAM.Domain.Users;

namespace FAM.Domain.Authorization;

/// <summary>
/// User role assignment at organization node
/// Represents user's role within a specific organizational context
/// Composite key: (UserId, NodeId, RoleId)
/// </summary>
public class UserNodeRole : JunctionEntity
{
    public long UserId { get; private set; }
    public User User { get; private set; } = null!;
    public long NodeId { get; private set; }
    public OrgNode Node { get; private set; } = null!;
    public long RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public DateTime? StartAt { get; private set; }
    public DateTime? EndAt { get; private set; }
    public long? AssignedById { get; private set; }

    private UserNodeRole()
    {
    }

    /// <summary>
    /// Assign role to user at organization node
    /// </summary>
    public static UserNodeRole Create(long userId, long nodeId, long roleId,
        DateTime? startAt = null, DateTime? endAt = null, long? assignedById = null)
    {
        ValidateDateRange(startAt, endAt);

        return new UserNodeRole
        {
            UserId = userId,
            NodeId = nodeId,
            RoleId = roleId,
            StartAt = startAt,
            EndAt = endAt,
            AssignedById = assignedById
        };
    }

    /// <summary>
    /// Assign role to user with full entities
    /// </summary>
    public static UserNodeRole Create(User user, OrgNode node, Role role,
        DateTime? startAt = null, DateTime? endAt = null, long? assignedById = null)
    {
        ValidateDateRange(startAt, endAt);

        return new UserNodeRole
        {
            UserId = user.Id,
            User = user,
            NodeId = node.Id,
            Node = node,
            RoleId = role.Id,
            Role = role,
            StartAt = startAt,
            EndAt = endAt,
            AssignedById = assignedById
        };
    }

    /// <summary>
    /// Update role assignment dates
    /// </summary>
    public void UpdateDates(DateTime? startAt, DateTime? endAt)
    {
        ValidateDateRange(startAt, endAt);

        StartAt = startAt;
        EndAt = endAt;
    }

    /// <summary>
    /// Check if role assignment is currently active
    /// </summary>
    public bool IsActive()
    {
        var now = DateTime.UtcNow;

        if (StartAt.HasValue && now < StartAt.Value)
            return false;

        if (EndAt.HasValue && now > EndAt.Value)
            return false;

        return true;
    }

    private static void ValidateDateRange(DateTime? startAt, DateTime? endAt)
    {
        if (endAt.HasValue && startAt.HasValue && endAt.Value <= startAt.Value)
            throw new DomainException(
                ErrorCodes.ROLE_ASSIGNMENT_INVALID_DATE_RANGE,
                "End date must be after start date");
    }
}