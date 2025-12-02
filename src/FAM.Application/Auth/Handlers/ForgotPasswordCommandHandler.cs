using System.Security.Cryptography;
using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler để gửi email reset password
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponse> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);

        // Always return success message (for security - don't reveal if email exists)
        var maskedEmail = MaskEmail(request.Email);
        var response = new ForgotPasswordResponse
        {
            Success = true,
            Message = "If this email exists in our system, you will receive a password reset link shortly.",
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

        // Save reset token (expires in 1 hour)
        user.SetPasswordResetToken(resetToken, 60);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email
        try
        {
            // TODO: Get reset URL from configuration or pass from frontend
            var resetUrl = "https://fam.yourdomain.com/reset-password";

            await _emailService.SendPasswordResetEmailAsync(
                user.Email.Value,
                resetToken,
                user.Username.Value,
                resetUrl,
                cancellationToken);

            _logger.LogInformation("Password reset email sent to user {UserId}", user.Id);
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