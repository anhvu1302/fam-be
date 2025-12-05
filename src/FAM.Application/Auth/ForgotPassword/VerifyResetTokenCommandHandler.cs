using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.ForgotPassword;

/// <summary>
/// Handler để verify reset token
/// </summary>
public class VerifyResetTokenCommandHandler : IRequestHandler<VerifyResetTokenCommand, VerifyResetTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyResetTokenCommandHandler> _logger;

    public VerifyResetTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<VerifyResetTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<VerifyResetTokenResponse> Handle(
        VerifyResetTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Reset token verification attempted for non-existent email: {Email}", request.Email);
            return new VerifyResetTokenResponse
            {
                IsValid = false,
                Code = ErrorCodes.AUTH_INVALID_RESET_TOKEN,
                Message = "Invalid or expired reset token."
            };
        }

        // Verify the reset token
        if (!user.IsPasswordResetTokenValid(request.ResetToken))
        {
            _logger.LogWarning("Invalid or expired reset token for user: {UserId}", user.Id);
            return new VerifyResetTokenResponse
            {
                IsValid = false,
                Code = ErrorCodes.AUTH_INVALID_RESET_TOKEN,
                Message = "Invalid or expired reset token."
            };
        }

        _logger.LogInformation("Reset token verified successfully for user: {UserId}", user.Id);

        return new VerifyResetTokenResponse
        {
            IsValid = true,
            Code = ErrorCodes.AUTH_RESET_TOKEN_VALID,
            Message = "Reset token is valid.",
            MaskedEmail = MaskEmail(user.Email.Value)
        };
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return email;

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"{localPart[0]}***@{domain}";

        var visibleChars = Math.Min(2, localPart.Length / 2);
        var maskedPart = localPart.Substring(0, visibleChars) + "***";

        return $"{maskedPart}@{domain}";
    }
}
