using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Authorization.Roles.Commands.RemoveUsersFromRole;

public sealed class RemoveUsersFromRoleCommandHandler : IRequestHandler<RemoveUsersFromRoleCommand, int>
{
    private readonly ILogger<RemoveUsersFromRoleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUsersFromRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemoveUsersFromRoleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(RemoveUsersFromRoleCommand request, CancellationToken cancellationToken)
    {
        if (!request.UserIds.Any())
        {
            throw new DomainException(ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED, "No user IDs provided");
        }

        IEnumerable<UserNodeRole> assignments = await _unitOfWork.UserNodeRoles.FindAsync(
            unr => unr.RoleId == request.RoleId
                   && unr.NodeId == request.NodeId
                   && request.UserIds.Contains(unr.UserId),
            cancellationToken);

        if (!assignments.Any())
        {
            _logger.LogWarning("No assignments found for role {RoleId} at node {NodeId} with specified users",
                request.RoleId, request.NodeId);
            return 0;
        }

        int removedCount = 0;
        foreach (UserNodeRole assignment in assignments)
        {
            _unitOfWork.UserNodeRoles.Delete(assignment);
            removedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return removedCount;
    }
}
