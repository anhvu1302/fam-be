using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Authorization.Roles.Commands.ReplaceUserRoles;

public sealed class ReplaceUserRolesCommandHandler : IRequestHandler<ReplaceUserRolesCommand>
{
    private readonly ILogger<ReplaceUserRolesCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ReplaceUserRolesCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ReplaceUserRolesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ReplaceUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new DomainException(ErrorCodes.USER_NOT_FOUND, "User not found");

        var node = await _unitOfWork.OrgNodes.GetByIdAsync(request.NodeId, cancellationToken);
        if (node == null)
            throw new DomainException(ErrorCodes.NODE_NOT_FOUND, "Organization node not found");

        foreach (var roleId in request.RoleIds)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
                throw new DomainException(ErrorCodes.ROLE_NOT_FOUND, $"Role {roleId} not found");
        }

        var existingAssignments = await _unitOfWork.UserNodeRoles
            .FindAsync(unr => unr.UserId == request.UserId && unr.NodeId == request.NodeId, cancellationToken);

        foreach (var assignment in existingAssignments) _unitOfWork.UserNodeRoles.Delete(assignment);

        foreach (var roleId in request.RoleIds)
        {
            var assignment = UserNodeRole.Create(
                request.UserId,
                request.NodeId,
                roleId,
                request.StartAt,
                request.EndAt,
                request.AssignedById
            );

            await _unitOfWork.UserNodeRoles.AddAsync(assignment, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Replaced roles for user {UserId} at node {NodeId} with {Count} roles",
            request.UserId, request.NodeId, request.RoleIds.Length);
    }
}