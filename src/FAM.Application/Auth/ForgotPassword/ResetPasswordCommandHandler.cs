using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.ForgotPassword;

/// <summary>
/// Handler để reset password với token
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        User? user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
            throw new UnauthorizedException(
                ErrorCodes.AUTH_INVALID_RESET_TOKEN,
                ErrorMessages.GetMessage(ErrorCodes.AUTH_INVALID_RESET_TOKEN));
        }

        // Verify the reset token
        if (!user.IsPasswordResetTokenValid(request.ResetToken))
        {
            _logger.LogWarning("Invalid or expired reset token for password reset: {UserId}", user.Id);
            throw new UnauthorizedException(
                ErrorCodes.AUTH_RESET_TOKEN_EXPIRED,
                ErrorMessages.GetMessage(ErrorCodes.AUTH_RESET_TOKEN_EXPIRED));
        }

        // Create new password (Password.Create will throw if invalid)
        var newPassword = Password.Create(request.NewPassword);

        // Update the password (this also clears the reset token)
        user.UpdatePassword(newPassword.Hash, newPassword.Salt);

        // Save changes
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successfully for user: {UserId}. Reset token has been invalidated.",
            user.Id);

        return new ResetPasswordResponse();
    }
}
