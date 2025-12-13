using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.RevokeRoleFromUser;

public sealed class RevokeRoleFromUserCommandHandler : IRequestHandler<RevokeRoleFromUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeRoleFromUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RevokeRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        UserNodeRole? userNodeRole = await _unitOfWork.UserNodeRoles.GetByUserAndNodeAndRoleAsync(
            request.UserId, request.NodeId, request.RoleId, cancellationToken);

        if (userNodeRole == null)
            throw new NotFoundException(ErrorCodes.ROLE_ASSIGNMENT_NOT_FOUND,
                $"Role assignment not found for User {request.UserId}, Node {request.NodeId}, Role {request.RoleId}");

        _unitOfWork.UserNodeRoles.Delete(userNodeRole);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
