using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using MediatR;

namespace FAM.Application.Authorization.Roles.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, long>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var existingRole = await _unitOfWork.Roles.GetByCodeAsync(request.Code, cancellationToken);
        if (existingRole != null)
            throw new ConflictException(ErrorCodes.ROLE_CODE_EXISTS, $"Role with code '{request.Code}' already exists");

        var role = Role.Create(
            request.Code,
            request.Name,
            request.Rank,
            request.Description,
            request.IsSystemRole
        );

        await _unitOfWork.Roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}