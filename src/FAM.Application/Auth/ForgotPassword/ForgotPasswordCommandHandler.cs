using System.Security.Cryptography;

using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Application.Settings;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;

using MediatR;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FAM.Application.Auth.ForgotPassword;

/// <summary>
/// Handler để gửi email reset password
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly FrontendOptions _frontendOptions;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger,
        IOptions<FrontendOptions> frontendOptions)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<ForgotPasswordResponse> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        User? user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);

        // Always return success message (for security - don't reveal if email exists)
        var maskedEmail = MaskEmail(request.Email);
        var response = new ForgotPasswordResponse
        {
            MaskedEmail = maskedEmail
        };

        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return response;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Password reset requested for inactive user: {UserId}", user.Id);
            return response;
        }

        // Generate reset token
        var resetToken = GenerateSecureToken();

        // Save reset token (expires based on configuration, default 15 minutes)
        user.SetPasswordResetToken(resetToken, _frontendOptions.PasswordResetTokenExpiryMinutes);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email
        try
        {
            // Get reset URL from configuration
            var resetUrl = _frontendOptions.GetResetPasswordUrl();

            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                resetToken,
                user.Username,
                resetUrl,
                _frontendOptions.PasswordResetTokenExpiryMinutes,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to user {UserId}", user.Id);
            // Don't reveal error to user for security
        }

        return response;
    }

    private static string GenerateSecureToken()
    {
        // Generate a cryptographically secure random token
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
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
