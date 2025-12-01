using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler để verify reset token trước khi user nhập password mới
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
        var user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            return new VerifyResetTokenResponse
            {
                IsValid = false,
                Message = "Invalid reset link"
            };
        }

        var isValid = user.IsPasswordResetTokenValid(request.ResetToken);

        if (!isValid)
        {
            _logger.LogWarning("Invalid or expired reset token for user {UserId}", user.Id);
            return new VerifyResetTokenResponse
            {
                IsValid = false,
                Message = "Reset link is invalid or has expired"
            };
        }

        return new VerifyResetTokenResponse
        {
            IsValid = true,
            Message = "Reset link is valid",
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
