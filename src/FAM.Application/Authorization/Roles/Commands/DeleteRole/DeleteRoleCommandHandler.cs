using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.DeleteRole;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        Role? role = await _unitOfWork.Roles.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(ErrorCodes.ROLE_NOT_FOUND, $"Role with ID {request.Id} not found");
        }

        role.ValidateCanDelete();

        IEnumerable<UserNodeRole> userRoles =
            await _unitOfWork.UserNodeRoles.GetByRoleIdAsync(request.Id, cancellationToken);
        if (userRoles.Any())
        {
            throw new ConflictException(ErrorCodes.ROLE_IN_USE, "Role is assigned to users and cannot be deleted");
        }

        await _unitOfWork.RolePermissions.DeleteByRoleIdAsync(request.Id, cancellationToken);
        _unitOfWork.Roles.Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
