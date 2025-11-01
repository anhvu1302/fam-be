using FAM.Application.Auth.Commands;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Auth.Handlers;

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
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Verify password
        if (!user.Password.Verify(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        // Disable 2FA
        user.DisableTwoFactor();
        
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
