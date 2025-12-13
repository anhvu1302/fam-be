using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Organizations;
using FAM.Domain.Users;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Authorization.Roles.Commands.AssignUsersToRole;

public sealed class AssignUsersToRoleCommandHandler : IRequestHandler<AssignUsersToRoleCommand, int>
{
    private readonly ILogger<AssignUsersToRoleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AssignUsersToRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<AssignUsersToRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(AssignUsersToRoleCommand request, CancellationToken cancellationToken)
    {
        Role? role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            throw new DomainException(ErrorCodes.ROLE_NOT_FOUND, "Role not found");

        OrgNode? node = await _unitOfWork.OrgNodes.GetByIdAsync(request.NodeId, cancellationToken);
        if (node == null)
            throw new DomainException(ErrorCodes.NODE_NOT_FOUND, "Organization node not found");

        IEnumerable<UserNodeRole> existingAssignments = await _unitOfWork.UserNodeRoles
            .FindAsync(unr => unr.RoleId == request.RoleId && unr.NodeId == request.NodeId, cancellationToken);

        var existingUserIds = existingAssignments.Select(unr => unr.UserId).ToHashSet();
        var newUserIds = request.UserIds.Where(uid => !existingUserIds.Contains(uid)).ToList();

        if (!newUserIds.Any())
        {
            _logger.LogInformation("No new users to assign to role {RoleId} at node {NodeId}",
                request.RoleId, request.NodeId);
            return 0;
        }

        foreach (var userId in newUserIds)
        {
            User? user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new DomainException(ErrorCodes.USER_NOT_FOUND, $"User {userId} not found");
        }

        var addedCount = 0;
        foreach (var userId in newUserIds)
        {
            var assignment = UserNodeRole.Create(
                userId,
                request.NodeId,
                request.RoleId,
                request.StartAt,
                request.EndAt,
                request.AssignedById
            );

            await _unitOfWork.UserNodeRoles.AddAsync(assignment, cancellationToken);
            addedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned {Count} users to role {RoleId} at node {NodeId}",
            addedCount, request.RoleId, request.NodeId);

        return addedCount;
    }
}
