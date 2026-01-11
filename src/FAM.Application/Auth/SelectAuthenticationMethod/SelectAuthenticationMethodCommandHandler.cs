using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Email;
using FAM.Domain.Abstractions.Services;
using FAM.Domain.Common.Base;
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
    private readonly ITwoFactorSessionService _twoFactorSessionService;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<SelectAuthenticationMethodCommandHandler> _logger;

    public SelectAuthenticationMethodCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ITwoFactorSessionService twoFactorSessionService,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<SelectAuthenticationMethodCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _twoFactorSessionService = twoFactorSessionService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<SelectAuthenticationMethodResponse> Handle(
        SelectAuthenticationMethodCommand request,
        CancellationToken cancellationToken)
    {
        // Validate and extract userId từ 2FA session token
        long userId = await ValidateAndExtractUserIdAsync(request.TwoFactorSessionToken, cancellationToken);
        if (userId == 0)
        {
            throw new UnauthorizedException(ErrorCodes.AUTH_INVALID_TOKEN);
        }

        User? user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedException(ErrorCodes.USER_NOT_FOUND);
        }

        SelectAuthenticationMethodResponse response = new();

        switch (request.SelectedMethod.ToLowerInvariant())
        {
            case "email_otp":
                if (!user.IsEmailVerified)
                {
                    throw new InvalidOperationException(
                        "Email authentication is not available. Please verify your email first.");
                }

                // Generate và gửi OTP qua email
                // SECURITY: Bind OTP với session token
                string otpCode =
                    await _otpService.GenerateOtpAsync(userId, request.TwoFactorSessionToken, 10, cancellationToken);
                await _emailService.SendOtpEmailAsync(user.Email, otpCode, user.Username,
                    cancellationToken);

                response.SelectedMethod = "email_otp";
                response.AdditionalInfo = $"Code sent to {MaskEmail(user.Email)}";
                response.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
                break;

            case "authenticator_app":
                if (!user.TwoFactorEnabled)
                {
                    throw new InvalidOperationException(
                        "Authenticator app is not configured. Please set up 2FA first.");
                }

                response.SelectedMethod = "authenticator_app";
                response.AdditionalInfo = "Enter 6-digit code from your authenticator app";
                break;

            case "recovery_code":
                if (string.IsNullOrWhiteSpace(user.TwoFactorBackupCodes))
                {
                    throw new InvalidOperationException("Recovery codes are not available. Please contact support.");
                }

                response.SelectedMethod = "recovery_code";
                response.AdditionalInfo = "Enter your backup recovery code";
                break;

            default:
                throw new ArgumentException($"Invalid authentication method: {request.SelectedMethod}");
        }

        return response;
    }

    private async Task<long> ValidateAndExtractUserIdAsync(string token, CancellationToken cancellationToken)
    {
        // Use the 2FA session service instead of JWT parsing
        long userId = await _twoFactorSessionService.ValidateAndGetUserIdAsync(token, cancellationToken);
        return userId;
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        string[] parts = email.Split('@');
        if (parts.Length != 2)
        {
            return email;
        }

        string localPart = parts[0];
        string domain = parts[1];

        if (localPart.Length <= 2)
        {
            return $"{localPart[0]}***@{domain}";
        }

        int visibleChars = Math.Min(2, localPart.Length / 2);
        string maskedPart = localPart.Substring(0, visibleChars) + "***";

        return $"{maskedPart}@{domain}";
    }
}
