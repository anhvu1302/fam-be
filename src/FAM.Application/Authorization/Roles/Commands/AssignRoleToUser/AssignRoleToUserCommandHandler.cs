using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleToUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(ErrorCodes.USER_NOT_FOUND, $"User with ID {request.UserId} not found");
        }

        Role? role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(ErrorCodes.ROLE_NOT_FOUND, $"Role with ID {request.RoleId} not found");
        }

        Resource? node = await _unitOfWork.Resources.GetByIdAsync(request.NodeId, cancellationToken);
        if (node == null)
        {
            throw new NotFoundException(ErrorCodes.VAL_INVALID_VALUE,
                $"Organization node with ID {request.NodeId} not found");
        }

        UserNodeRole? existingAssignment = await _unitOfWork.UserNodeRoles
            .GetByUserAndNodeAndRoleAsync(request.UserId, request.NodeId, request.RoleId, cancellationToken);

        if (existingAssignment != null)
        {
            throw new ConflictException(ErrorCodes.ROLE_ALREADY_ASSIGNED_TO_USER,
                "Role is already assigned to this user at this node");
        }

        UserNodeRole userNodeRole = UserNodeRole.Create(
            request.UserId,
            request.NodeId,
            request.RoleId,
            request.StartAt,
            request.EndAt,
            request.AssignedById
        );

        await _unitOfWork.UserNodeRoles.AddAsync(userNodeRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
