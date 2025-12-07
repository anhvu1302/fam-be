using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FAM.Application.Auth.ChangePassword;
using FAM.Application.Auth.Confirm2FA;
using FAM.Application.Auth.Disable2FA;
using FAM.Application.Auth.DisableTwoFactorWithBackup;
using FAM.Application.Auth.Enable2FA;
using FAM.Application.Auth.ForgotPassword;
using FAM.Application.Auth.GetAuthenticationMethods;
using FAM.Application.Auth.Login;
using FAM.Application.Auth.Logout;
using FAM.Application.Auth.RefreshToken;
using FAM.Application.Auth.SelectAuthenticationMethod;
using FAM.Application.Auth.Shared;
using FAM.Application.Auth.VerifyTwoFactor;
using FAM.Application.Auth.VerifyEmailOtp;
using FAM.Application.Common.Services;
using FAM.Domain.Common;
using FAM.WebApi.Configuration;
using FAM.WebApi.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApiContracts = FAM.WebApi.Contracts.Auth;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Authentication controller - handles login, logout, token refresh, password change
/// Web API layer: validates shape/format (ModelState), then delegates to Application
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly ILocationService _locationService;

    public AuthController(IMediator mediator, ILogger<AuthController> logger, ILocationService locationService)
    {
        _mediator = mediator;
        _logger = logger;
        _locationService = locationService;
    }

    /// <summary>
    /// Login with username/email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitConfiguration.AuthenticationPolicy)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] WebApiContracts.LoginRequest request)
    {
        // Web API validation: ModelState checks DataAnnotations
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var ipAddress = GetClientIpAddress();
            var location = await _locationService.GetLocationFromIpAsync(ipAddress);

            var command = new LoginCommand
            {
                Identity = request.Identity,
                Password = request.Password,
                DeviceId = GenerateDeviceId(),
                DeviceName = GetDeviceName(),
                DeviceType = GetDeviceType(),
                IpAddress = ipAddress,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                RememberMe = request.RememberMe,
                Location = location
            };

            var response = await _mediator.Send(command);
            return OkResponse(response, "Login successful");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed for identity: {Identity}", request.Identity);
            return UnauthorizedResponse(ex.Message, "INVALID_CREDENTIALS");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for identity: {Identity}", request.Identity);
            return InternalErrorResponse("An error occurred during login", "LOGIN_ERROR");
        }
    }

    /// <summary>
    /// Verify 2FA code to complete login
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyTwoFactorResponse>> VerifyTwoFactor(
        [FromBody] WebApiContracts.VerifyTwoFactorRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var ipAddress = GetClientIpAddress();
            var location = await _locationService.GetLocationFromIpAsync(ipAddress);

            var command = new VerifyTwoFactorCommand
            {
                TwoFactorSessionToken = request.TwoFactorSessionToken,
                TwoFactorCode = request.TwoFactorCode,
                DeviceId = GenerateDeviceId(),
                DeviceName = GetDeviceName(),
                DeviceType = GetDeviceType(),
                IpAddress = ipAddress,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Location = location,
                RememberMe = request.RememberMe
            };

            var response = await _mediator.Send(command);
            return OkResponse(response, "2FA verification successful");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "2FA verification failed");
            return UnauthorizedResponse(ex.Message, "INVALID_2FA_CODE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification");
            return InternalErrorResponse("An error occurred during 2FA verification", "2FA_VERIFICATION_ERROR");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] WebApiContracts.RefreshTokenRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken,
                IpAddress = GetClientIpAddress(),
                Location = null // Could parse from IP using GeoIP service
            };

            var response = await _mediator.Send(command);
            return OkResponse(response, "Token refreshed successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return UnauthorizedResponse(ex.Message, "INVALID_REFRESH_TOKEN");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return InternalErrorResponse("An error occurred during token refresh", "TOKEN_REFRESH_ERROR");
        }
    }

    /// <summary>
    /// Logout from current device
    /// Device is automatically detected from request headers
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var command = new LogoutCommand
            {
                RefreshToken = null, // Will be handled by deviceId
                DeviceId = GenerateDeviceId(),
                IpAddress = GetClientIpAddress()
            };

            await _mediator.Send(command);
            return OkResponse("Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return InternalErrorResponse("An error occurred during logout", "LOGOUT_ERROR");
        }
    }

    /// <summary>
    /// Logout from all devices
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<ActionResult> LogoutAll([FromBody] LogoutAllRequest? request = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var currentDeviceId = GenerateDeviceId();

            var command = new LogoutAllDevicesCommand
            {
                UserId = userId,
                ExceptDeviceId = request?.ExceptCurrentDevice == true ? currentDeviceId : null
            };

            await _mediator.Send(command);
            return OkResponse("Logged out from all devices successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout all");
            return InternalErrorResponse("An error occurred during logout", "LOGOUT_ALL_ERROR");
        }
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] WebApiContracts.ChangePasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();

            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword,
                LogoutAllDevices = request.LogoutAllDevices,
                CurrentDeviceId = request.LogoutAllDevices ? GenerateDeviceId() : null
            };

            await _mediator.Send(command);
            return OkResponse("Password changed successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Password change failed");
            return UnauthorizedResponse(ex.Message, "INVALID_PASSWORD");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return InternalErrorResponse("An error occurred while changing password", "PASSWORD_CHANGE_ERROR");
        }
    }

    /// <summary>
    /// Get authentication methods for current user
    /// </summary>
    [HttpGet("authentication-methods")]
    [Authorize]
    public async Task<ActionResult<AuthenticationMethodsResponse>> GetAuthenticationMethods()
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetAuthenticationMethodsQuery { UserId = userId };
            var response = await _mediator.Send(query);
            return OkResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication methods");
            return InternalErrorResponse("An error occurred while fetching authentication methods", "GET_AUTH_METHODS_ERROR");
        }
    }

    /// <summary>
    /// Select authentication method for 2FA
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.AuthenticationPolicy)]
    [HttpPost("select-authentication-method")]
    [AllowAnonymous]
    public async Task<ActionResult<SelectAuthenticationMethodResponse>> SelectAuthenticationMethod(
        [FromBody] WebApiContracts.SelectAuthenticationMethodRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new SelectAuthenticationMethodCommand
            {
                TwoFactorSessionToken = request.TwoFactorSessionToken,
                SelectedMethod = request.SelectedMethod
            };

            var response = await _mediator.Send(command);
            return OkResponse(response, "Authentication method selected successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication method selection failed");
            return UnauthorizedResponse(ex.Message, "INVALID_SESSION_TOKEN");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting authentication method");
            return InternalErrorResponse("An error occurred while selecting authentication method", "SELECT_AUTH_METHOD_ERROR");
        }
    }

    /// <summary>
    /// Verify email OTP during login
    /// Request must include the email address and OTP code sent to that email
    /// </summary>
    /// <remarks>
    /// After calling /api/auth/login with unverified email, you'll receive:
    /// - requiresEmailVerification: true
    /// - emailVerificationSessionToken
    /// - maskedEmail
    /// 
    /// Then call this endpoint with the email and OTP received
    /// </remarks>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("verify-email-otp")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyEmailOtp([FromBody] WebApiContracts.VerifyEmailOtpRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new VerifyEmailOtpLoginCommand
            {
                EmailOtp = request.EmailOtp,
                Email = request.Email
            };

            var response = await _mediator.Send(command);
            return OkResponse(response, "Email OTP verification successful");
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Email OTP verification failed for {Email}", request.Email);
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email OTP for {Email}", request.Email);
            return InternalErrorResponse("An error occurred while verifying OTP", "EMAIL_OTP_ERROR");
        }
    }

    /// <summary>
    /// Verify recovery code
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("verify-recovery-code")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyRecoveryCode([FromBody] WebApiContracts.VerifyRecoveryCodeRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var ipAddress = GetClientIpAddress();
            var location = await _locationService.GetLocationFromIpAsync(ipAddress);

            var command = new VerifyRecoveryCodeCommand
            {
                TwoFactorSessionToken = request.TwoFactorSessionToken,
                RecoveryCode = request.RecoveryCode,
                RememberMe = request.RememberMe,
                DeviceId = GenerateDeviceId(),
                DeviceName = GetDeviceName(),
                DeviceType = GetDeviceType(),
                IpAddress = ipAddress,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Location = location
            };

            var response = await _mediator.Send(command);
            return OkResponse(response, "Recovery code verification successful");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Recovery code verification failed");
            return UnauthorizedResponse(ex.Message, "INVALID_RECOVERY_CODE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying recovery code");
            return InternalErrorResponse("An error occurred while verifying recovery code", "RECOVERY_CODE_ERROR");
        }
    }

    /// <summary>
    /// Request password reset (forgot password)
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.AuthenticationPolicy)]
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] WebApiContracts.ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new ForgotPasswordCommand { Email = request.Email };
            var response = await _mediator.Send(command);
            return OkResponse(response, "Password reset email sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return InternalErrorResponse("An error occurred while processing password reset", "FORGOT_PASSWORD_ERROR");
        }
    }

    /// <summary>
    /// Verify reset token
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("verify-reset-token")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyResetTokenResponse>> VerifyResetToken([FromBody] WebApiContracts.VerifyResetTokenRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new VerifyResetTokenCommand
            {
                Email = request.Email,
                ResetToken = request.ResetToken
            };
            var response = await _mediator.Send(command);
            return OkResponse(response, "Token verified successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Token verification failed");
            return UnauthorizedResponse(ex.Message, "INVALID_OR_EXPIRED_TOKEN");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying reset token");
            return InternalErrorResponse("An error occurred while verifying token", "TOKEN_VERIFICATION_ERROR");
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromBody] WebApiContracts.ResetPasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new ResetPasswordCommand
            {
                Email = request.Email,
                ResetToken = request.ResetToken,
                NewPassword = request.NewPassword
            };
            var response = await _mediator.Send(command);
            return OkResponse(response, "Password reset successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Password reset failed");
            return UnauthorizedResponse(ex.Message, "INVALID_OR_EXPIRED_TOKEN");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return InternalErrorResponse("An error occurred while resetting password", "PASSWORD_RESET_ERROR");
        }
    }

    /// <summary>
    /// Enable 2FA - generates new secret and QR code
    /// </summary>
    [HttpPost("enable-2fa")]
    [Authorize]
    public async Task<ActionResult<Enable2FAResponse>> Enable2FA([FromBody] WebApiContracts.Enable2FARequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();

            var command = new Enable2FACommand
            {
                UserId = userId,
                Password = request.Password
            };

            var response = await _mediator.Send(command);
            return OkResponse(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Enable 2FA failed for user: {UserId}", GetCurrentUserId());
            return UnauthorizedResponse(ex.Message, "INVALID_PASSWORD");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enable 2FA for user: {UserId}", GetCurrentUserId());
            return InternalErrorResponse("An error occurred while enabling 2FA", "ENABLE_2FA_ERROR");
        }
    }

    /// <summary>
    /// Confirm 2FA setup by verifying code
    /// <summary>
    /// Confirm 2FA setup and receive backup codes
    /// </summary>
    [HttpPost("confirm-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(Confirm2FAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Confirm2FAResponse>> Confirm2FA([FromBody] WebApiContracts.Confirm2FARequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();

            var command = new Confirm2FACommand
            {
                UserId = userId,
                Secret = request.Secret,
                Code = request.Code
            };

            var response = await _mediator.Send(command);
            return OkResponse(response, "Two-factor authentication enabled successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Confirm 2FA failed for user: {UserId}", GetCurrentUserId());
            return UnauthorizedResponse(ex.Message, "UNAUTHORIZED");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid 2FA code for user: {UserId}", GetCurrentUserId());
            return BadRequestResponse(ex.Message, "INVALID_2FA_CODE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during confirm 2FA for user: {UserId}", GetCurrentUserId());
            return InternalErrorResponse("An error occurred while confirming 2FA", "CONFIRM_2FA_ERROR");
        }
    }

    /// <summary>
    /// Disable 2FA
    /// </summary>
    [HttpPost("disable-2fa")]
    [Authorize]
    public async Task<ActionResult> Disable2FA([FromBody] WebApiContracts.Disable2FARequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var userId = GetCurrentUserId();

            var command = new Disable2FACommand
            {
                UserId = userId,
                Password = request.Password
            };

            await _mediator.Send(command);
            return OkResponse("2FA disabled successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Disable 2FA failed for user: {UserId}", GetCurrentUserId());
            return UnauthorizedResponse(ex.Message, "INVALID_PASSWORD");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disable 2FA for user: {UserId}", GetCurrentUserId());
            return InternalErrorResponse("An error occurred while disabling 2FA", "DISABLE_2FA_ERROR");
        }
    }

    /// <summary>
    /// Disable 2FA using backup code (for account recovery when device is lost)
    /// This endpoint does not require authentication - only username, password, and backup code
    /// </summary>
    [HttpPost("disable-2fa-with-backup")]
    [AllowAnonymous]
    public async Task<ActionResult> DisableTwoFactorWithBackup(
        [FromBody] WebApiContracts.DisableTwoFactorWithBackupRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var command = new DisableTwoFactorWithBackupCommand
            {
                Username = request.Username,
                Password = request.Password,
                BackupCode = request.BackupCode
            };

            await _mediator.Send(command);
            return OkResponse("Two-factor authentication has been disabled successfully. You can now login without 2FA.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Disable 2FA with backup code failed for username: {Username}", request.Username);
            return UnauthorizedResponse(ex.Message, "INVALID_CREDENTIALS_OR_CODE");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during disable 2FA with backup for username: {Username}",
                request.Username);
            return BadRequestResponse(ex.Message, "INVALID_BACKUP_CODE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disable 2FA with backup for username: {Username}", request.Username);
            return InternalErrorResponse("An error occurred while disabling 2FA", "DISABLE_2FA_BACKUP_ERROR");
        }
    }

    /// <summary>
    /// Get current user info (test endpoint to verify JWT authentication)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            userId,
            username,
            email,
            roles,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    #region Helper Methods

    protected new string GetClientIpAddress()
    {
        // Priority order for getting real client IP:
        // 1. CF-Connecting-IP (Cloudflare)
        // 2. True-Client-IP (Cloudflare Enterprise)
        // 3. X-Real-IP (Nginx proxy)
        // 4. X-Forwarded-For (Standard proxy header)
        // 5. X-Client-IP
        // 6. RemoteIpAddress (Direct connection)

        // Cloudflare header (most reliable when using Cloudflare)
        var cfConnectingIp = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfConnectingIp)) return cfConnectingIp.Trim();

        // Cloudflare Enterprise header
        var trueClientIp = Request.Headers["True-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(trueClientIp)) return trueClientIp.Trim();

        // X-Real-IP from Nginx or other reverse proxy
        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp)) return realIp.Trim();

        // X-Forwarded-For (can contain multiple IPs: client, proxy1, proxy2)
        // Take the first IP which is the original client
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ips.Length > 0)
            {
                var clientIp = ips[0];
                // Validate it's a proper IP address
                if (IPAddress.TryParse(clientIp, out _)) return clientIp;
            }
        }

        // X-Client-IP header
        var clientIp2 = Request.Headers["X-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clientIp2)) return clientIp2.Trim();

        // Fallback to RemoteIpAddress (direct connection, no proxy)
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // Handle IPv6 loopback (::1) and map it to IPv4
            if (remoteIp.IsIPv4MappedToIPv6) remoteIp = remoteIp.MapToIPv4();

            // Convert IPv6 loopback to IPv4
            if (remoteIp.ToString() == "::1") return "127.0.0.1";

            return remoteIp.ToString();
        }

        return "Unknown";
    }

    private string GenerateDeviceId()
    {
        // Generate a unique device ID based on user agent and IP
        var userAgent = Request.Headers["User-Agent"].ToString();
        var ip = GetClientIpAddress();
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes($"{userAgent}_{ip}_{DateTime.UtcNow.Ticks}"));
        return Convert.ToBase64String(hash)[..32];
    }

    private string GetDeviceName()
    {
        var userAgent = Request.Headers["User-Agent"].ToString();

        // Simple device name extraction from User-Agent
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg"))
            return "Chrome Browser";
        if (userAgent.Contains("Firefox"))
            return "Firefox Browser";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
            return "Safari Browser";
        if (userAgent.Contains("Edg"))
            return "Edge Browser";
        if (userAgent.Contains("Postman"))
            return "Postman";
        if (userAgent.Contains("curl"))
            return "curl";

        return "Unknown Device";
    }

    private string GetDeviceType()
    {
        var userAgent = Request.Headers["User-Agent"].ToString();

        // Determine device type from User-Agent
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
            return "mobile";
        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
            return "tablet";

        return "desktop";
    }

    #endregion

    #region Session & Theme Management

    /// <summary>
    /// Get current user's login sessions
    /// </summary>
    [HttpGet("me/sessions")]
    [Authorize]
    [ProducesResponseType(typeof(FAM.WebApi.Contracts.Users.UserSessionsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySessions()
    {
        var userId = GetCurrentUserId();
        var query = new FAM.Application.Users.Queries.GetUserSessions.GetUserSessionsQuery(userId);
        var result = await _mediator.Send(query);

        var response = new FAM.WebApi.Contracts.Users.UserSessionsResponse(
            result.Sessions.Select(s => new FAM.WebApi.Contracts.Users.UserSessionResponse(
                s.Id,
                s.DeviceId,
                s.DeviceName,
                s.DeviceType,
                s.IpAddress,
                s.Location,
                s.Browser,
                s.OperatingSystem,
                s.LastLoginAt,
                s.LastActivityAt,
                s.IsActive,
                s.IsTrusted,
                s.IsCurrentDevice
            )).ToList()
        );

        return Ok(response);
    }

    /// <summary>
    /// Delete a specific login session
    /// </summary>
    [HttpDelete("me/sessions/{sessionId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(Guid sessionId)
    {
        var userId = GetCurrentUserId();
        var command = new FAM.Application.Users.Commands.DeleteSession.DeleteSessionCommand(userId, sessionId);
        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Delete all login sessions except current
    /// </summary>
    [HttpDelete("me/sessions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAllSessions([FromQuery] string? currentDeviceId = null)
    {
        var userId = GetCurrentUserId();
        var command = new FAM.Application.Users.Commands.DeleteAllSessions.DeleteAllSessionsCommand(userId, currentDeviceId);
        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Get current user's theme preferences
    /// </summary>
    [HttpGet("me/theme")]
    [Authorize]
    [ProducesResponseType(typeof(FAM.WebApi.Contracts.Users.UserThemeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTheme()
    {
        var userId = GetCurrentUserId();
        var query = new FAM.Application.Users.Queries.GetUserTheme.GetUserThemeQuery(userId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Theme not found. Using default theme." });

        var response = new FAM.WebApi.Contracts.Users.UserThemeResponse(
            result.Id,
            result.UserId,
            result.Theme,
            result.PrimaryColor,
            result.Transparency,
            result.BorderRadius,
            result.DarkTheme,
            result.PinNavbar,
            result.CompactMode
        );

        return Ok(response);
    }

    /// <summary>
    /// Update current user's theme preferences
    /// </summary>
    [HttpPut("me/theme")]
    [Authorize]
    [ProducesResponseType(typeof(FAM.WebApi.Contracts.Users.UserThemeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMyTheme([FromBody] FAM.WebApi.Contracts.Users.UpdateUserThemeRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new FAM.Application.Users.Commands.UpdateUserTheme.UpdateUserThemeCommand(
            userId,
            request.Theme,
            request.PrimaryColor,
            request.Transparency,
            request.BorderRadius,
            request.DarkTheme,
            request.PinNavbar,
            request.CompactMode
        );

        var result = await _mediator.Send(command);

        var response = new FAM.WebApi.Contracts.Users.UserThemeResponse(
            result.Id,
            result.UserId,
            result.Theme,
            result.PrimaryColor,
            result.Transparency,
            result.BorderRadius,
            result.DarkTheme,
            result.PinNavbar,
            result.CompactMode
        );

        return Ok(response);
    }

    #endregion
}

#region Request DTOs

public class LogoutAllRequest
{
    /// <summary>
    /// If true, keeps current device logged in (auto-detected from request headers)
    /// </summary>
    public bool ExceptCurrentDevice { get; set; } = false;
}

#endregion