using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        Role? role = await _unitOfWork.Roles.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
            throw new NotFoundException(ErrorCodes.ROLE_NOT_FOUND, $"Role with ID {request.Id} not found");

        role.Update(request.Name, request.Rank, request.Description);

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
