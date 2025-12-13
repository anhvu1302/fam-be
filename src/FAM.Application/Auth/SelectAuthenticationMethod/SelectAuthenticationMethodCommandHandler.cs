using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.SelectAuthenticationMethod;

/// <summary>
/// Handler để chọn phương thức xác thực và gửi OTP nếu cần
/// </summary>
public class
    SelectAuthenticationMethodCommandHandler : IRequestHandler<SelectAuthenticationMethodCommand,
    SelectAuthenticationMethodResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<SelectAuthenticationMethodCommandHandler> _logger;

    public SelectAuthenticationMethodCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<SelectAuthenticationMethodCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<SelectAuthenticationMethodResponse> Handle(
        SelectAuthenticationMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Validate và extract userId từ 2FA session token
        var userId = ValidateAndExtractUserId(request.TwoFactorSessionToken);
        if (userId == 0)
            throw new UnauthorizedAccessException("Invalid or expired 2FA session token");

        User? user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        var response = new SelectAuthenticationMethodResponse();

        switch (request.SelectedMethod.ToLowerInvariant())
        {
            case "email_otp":
                if (!user.IsEmailVerified)
                    throw new InvalidOperationException(
                        "Email authentication is not available. Please verify your email first.");

                // Generate và gửi OTP qua email
                // SECURITY: Bind OTP với session token
                var otpCode =
                    await _otpService.GenerateOtpAsync(userId, request.TwoFactorSessionToken, 10, cancellationToken);
                await _emailService.SendOtpEmailAsync(user.Email.Value, otpCode, user.Username.Value,
                    cancellationToken);

                response.SelectedMethod = "email_otp";
                response.AdditionalInfo = $"Code sent to {MaskEmail(user.Email.Value)}";
                response.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
                break;

            case "authenticator_app":
                if (!user.TwoFactorEnabled)
                    throw new InvalidOperationException(
                        "Authenticator app is not configured. Please set up 2FA first.");

                response.SelectedMethod = "authenticator_app";
                response.AdditionalInfo = "Enter 6-digit code from your authenticator app";
                break;

            case "recovery_code":
                if (string.IsNullOrWhiteSpace(user.TwoFactorBackupCodes))
                    throw new InvalidOperationException("Recovery codes are not available. Please contact support.");

                response.SelectedMethod = "recovery_code";
                response.AdditionalInfo = "Enter your backup recovery code";
                break;

            default:
                throw new ArgumentException($"Invalid authentication method: {request.SelectedMethod}");
        }

        return response;
    }

    private long ValidateAndExtractUserId(string token)
    {
        try
        {
            // JWT middleware already validates token signature, just extract user ID
            var userId = _jwtService.GetUserIdFromToken(token);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract user ID from 2FA session token");
            return 0;
        }
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
