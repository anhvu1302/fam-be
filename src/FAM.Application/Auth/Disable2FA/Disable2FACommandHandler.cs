using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using MediatR;

namespace FAM.Application.Auth.Disable2FA;

public sealed class Disable2FACommandHandler : IRequestHandler<Disable2FACommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public Disable2FACommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(Disable2FACommand request, CancellationToken cancellationToken)
    {
        // Get user by ID
        User? user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) throw new UnauthorizedException(ErrorCodes.USER_NOT_FOUND);

        // Verify password
        if (!user.Password.Verify(request.Password)) throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_PASSWORD);

        // Disable 2FA
        user.DisableTwoFactor();

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
