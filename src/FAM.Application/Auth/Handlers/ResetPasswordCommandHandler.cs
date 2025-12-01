using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler để reset password với token
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("Invalid reset link");

        // Verify reset token
        if (!user.IsPasswordResetTokenValid(request.ResetToken))
        {
            _logger.LogWarning("Invalid or expired reset token for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Reset link is invalid or has expired");
        }

        // Validate new password (Password.Create will throw if invalid)
        var newPassword = Password.Create(request.NewPassword);

        // Update password and clear reset token
        user.UpdatePassword(newPassword.Hash, newPassword.Salt);
        _unitOfWork.Users.Update(user);

        // Logout all devices for security
        await _unitOfWork.UserDevices.DeactivateAllUserDevicesAsync(user.Id, null, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send confirmation email
        try
        {
            await _emailService.SendPasswordChangedEmailAsync(
                user.Email.Value,
                user.Username.Value,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password changed email to user {UserId}", user.Id);
            // Don't fail the operation if email fails
        }

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);

        return new ResetPasswordResponse
        {
            Success = true,
            Message = "Password has been reset successfully. Please login with your new password."
        };
    }
}