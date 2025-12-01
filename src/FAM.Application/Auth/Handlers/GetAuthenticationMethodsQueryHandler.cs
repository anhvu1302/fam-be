using FAM.Application.Auth.DTOs;
using FAM.Application.Auth.Queries;
using FAM.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler để lấy thông tin các phương thức xác thực đang bật
/// </summary>
public class GetAuthenticationMethodsQueryHandler : IRequestHandler<GetAuthenticationMethodsQuery, AuthenticationMethodsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuthenticationMethodsQueryHandler> _logger;

    public GetAuthenticationMethodsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAuthenticationMethodsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuthenticationMethodsResponse> Handle(
        GetAuthenticationMethodsQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        // Parse recovery codes để đếm
        var remainingRecoveryCodes = 0;
        var recoveryCodesConfigured = false;

        if (!string.IsNullOrWhiteSpace(user.TwoFactorBackupCodes))
        {
            try
            {
                var codes = JsonSerializer.Deserialize<List<string>>(user.TwoFactorBackupCodes);
                if (codes != null && codes.Any())
                {
                    remainingRecoveryCodes = codes.Count;
                    recoveryCodesConfigured = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse recovery codes for user {UserId}", request.UserId);
            }
        }

        // Mask email để hiển thị
        var maskedEmail = MaskEmail(user.Email.Value);

        var response = new AuthenticationMethodsResponse
        {
            EmailAuthenticationEnabled = user.IsEmailVerified,
            MaskedEmail = maskedEmail,
            TwoFactorAuthenticatorEnabled = user.TwoFactorEnabled,
            TwoFactorSetupDate = user.TwoFactorSetupDate,
            RecoveryCodesConfigured = recoveryCodesConfigured,
            RemainingRecoveryCodes = remainingRecoveryCodes,
            AvailableMethods = new List<AuthenticationMethodInfo>()
        };

        // Thêm email OTP method nếu email đã verified
        if (user.IsEmailVerified)
        {
            response.AvailableMethods.Add(new AuthenticationMethodInfo
            {
                MethodType = "email_otp",
                DisplayName = "Email OTP",
                IsEnabled = true,
                IsPrimary = !user.TwoFactorEnabled, // Email là primary nếu chưa bật 2FA
                AdditionalInfo = $"Send code to {maskedEmail}"
            });
        }

        // Thêm authenticator app method nếu đã bật 2FA
        if (user.TwoFactorEnabled)
        {
            response.AvailableMethods.Add(new AuthenticationMethodInfo
            {
                MethodType = "authenticator_app",
                DisplayName = "Authenticator App",
                IsEnabled = true,
                IsPrimary = true,
                AdditionalInfo = user.TwoFactorSetupDate.HasValue
                    ? $"Enabled since {user.TwoFactorSetupDate.Value:yyyy-MM-dd}"
                    : "Enabled"
            });
        }

        // Thêm recovery code method nếu có recovery codes
        if (recoveryCodesConfigured)
        {
            response.AvailableMethods.Add(new AuthenticationMethodInfo
            {
                MethodType = "recovery_code",
                DisplayName = "Recovery Code",
                IsEnabled = true,
                IsPrimary = false,
                AdditionalInfo = $"{remainingRecoveryCodes} codes remaining"
            });
        }

        return response;
    }

    /// <summary>
    /// Mask email để bảo mật (vd: ab***@gmail.com)
    /// </summary>
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
