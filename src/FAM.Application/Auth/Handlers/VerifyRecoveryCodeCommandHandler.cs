using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler để verify recovery code
/// </summary>
public class VerifyRecoveryCodeCommandHandler : IRequestHandler<VerifyRecoveryCodeCommand, VerifyTwoFactorResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<VerifyRecoveryCodeCommandHandler> _logger;

    public VerifyRecoveryCodeCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<VerifyRecoveryCodeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<VerifyTwoFactorResponse> Handle(
        VerifyRecoveryCodeCommand request,
        CancellationToken cancellationToken)
    {
        // Validate và extract userId từ 2FA session token
        var userId = ValidateAndExtractUserId(request.TwoFactorSessionToken);
        if (userId == 0)
            throw new UnauthorizedAccessException("Invalid or expired 2FA session token");

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        if (string.IsNullOrWhiteSpace(user.TwoFactorBackupCodes))
            throw new InvalidOperationException("Recovery codes are not configured");

        // Parse recovery codes
        List<string>? recoveryCodes;
        try
        {
            recoveryCodes = JsonSerializer.Deserialize<List<string>>(user.TwoFactorBackupCodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse recovery codes for user {UserId}", userId);
            throw new InvalidOperationException("Invalid recovery codes configuration");
        }

        if (recoveryCodes == null || !recoveryCodes.Any())
            throw new InvalidOperationException("No recovery codes available");

        // Verify recovery code
        var normalizedInput = request.RecoveryCode.Trim().ToUpperInvariant();
        var codeFound = false;

        for (var i = 0; i < recoveryCodes.Count; i++)
        {
            if (recoveryCodes[i].Trim().ToUpperInvariant() == normalizedInput)
            {
                // Remove the used code
                recoveryCodes.RemoveAt(i);
                codeFound = true;
                break;
            }
        }

        if (!codeFound)
        {
            _logger.LogWarning("Invalid recovery code provided for user {UserId}", userId);
            throw new UnauthorizedAccessException("Invalid recovery code");
        }

        // Update recovery codes (remove used code)
        user.UpdateRecoveryCodes(JsonSerializer.Serialize(recoveryCodes));
        _unitOfWork.Users.Update(user);

        // Generate tokens
        var roles = new List<string>(); // TODO: Load user roles from UserNodeRoles
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username.Value, user.Email.Value, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7);

        // Get or create device
        var device = user.GetOrCreateDevice(
            request.DeviceId,
            request.DeviceName ?? "Unknown Device",
            request.DeviceType ?? "browser",
            request.UserAgent,
            request.IpAddress,
            request.Location
        );

        // Update device with refresh token
        device.UpdateRefreshToken(refreshToken, refreshTokenExpiresAt, request.IpAddress);

        // Update last login info
        user.RecordLogin(request.IpAddress);

        // Save changes
        var existingDevice = await _unitOfWork.UserDevices.GetByIdAsync(device.Id, cancellationToken);
        if (existingDevice != null)
        {
            existingDevice.UpdateRefreshToken(refreshToken, refreshTokenExpiresAt, request.IpAddress);
            existingDevice.RecordLogin(request.IpAddress, request.Location);
            _unitOfWork.UserDevices.Update(existingDevice);
        }
        else
        {
            await _unitOfWork.UserDevices.AddAsync(device, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} successfully completed recovery code verification. Remaining codes: {Count}",
            userId,
            recoveryCodes.Count);

        // Warn if running low on recovery codes
        if (recoveryCodes.Count <= 2)
        {
            _logger.LogWarning("User {UserId} has only {Count} recovery codes remaining", userId, recoveryCodes.Count);
        }

        return new VerifyTwoFactorResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer",
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username.Value,
                Email = user.Email.Value,
                FullName = user.FullName,
                IsEmailVerified = user.IsEmailVerified,
                IsTwoFactorEnabled = user.TwoFactorEnabled
            }
        };
    }

    private long ValidateAndExtractUserId(string token)
    {
        try
        {
            var principal = _jwtService.ValidateTokenAndGetPrincipal(token);
            if (principal == null)
                return 0;

            var userIdClaim = principal.FindFirst("user_id");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                return 0;

            var roleClaim = principal.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "2fa_session")
                return 0;

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate 2FA session token");
            return 0;
        }
    }
}
