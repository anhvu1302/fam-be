using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
using FAM.Application.Auth.VerifyEmailOtp;
using FAM.Application.Auth.VerifyTwoFactor;
using FAM.Application.Common.Services;
using FAM.Application.Users.Commands.DeleteAllSessions;
using FAM.Application.Users.Commands.DeleteSession;
using FAM.Application.Users.Commands.UpdateUserTheme;
using FAM.Application.Users.Queries.GetUserById;
using FAM.Application.Users.Queries.GetUserSessions;
using FAM.Application.Users.Queries.GetUserTheme;
using FAM.Application.Users.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.WebApi.Attributes;
using FAM.WebApi.Configuration;
using FAM.WebApi.Contracts.Auth;
using FAM.WebApi.Contracts.Common;
using FAM.WebApi.Contracts.Users;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(IMediator mediator, ILogger<AuthController> logger, ILocationService locationService,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _locationService = locationService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Login with username/email and password
    /// </summary>
    /// <remarks>
    /// Returns access token, refresh token, and user information upon successful authentication.
    /// </remarks>
    /// <response code="200">Login successful - Returns {success: true, message: string, result: LoginResponse}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="401">Unauthorized - Invalid credentials - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="500">Internal server error - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitConfiguration.AuthenticationPolicy)]
    [ProducesResponseType(typeof(ApiSuccessResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            string ipAddress = GetClientIpAddress();
            string? location = await _locationService.GetLocationFromIpAsync(ipAddress);

            // Generate device ID and store in cookie for future reference (logout, etc.)
            string deviceId = GenerateDeviceId();

            LoginCommand command = new()
            {
                Identity = request.Identity,
                Password = request.Password,
                DeviceId = deviceId,
                DeviceName = GetDeviceName(),
                DeviceType = GetDeviceType(),
                IpAddress = ipAddress,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                RememberMe = request.RememberMe,
                Location = location
            };

            LoginResponse response = await _mediator.Send(command);

            return OkResponse(response, "Login successful");
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_CREDENTIALS");
        }
        catch (Exception ex)
        {
            return InternalErrorResponse(ex.Message, "LOGIN_ERROR");
        }
    }

    /// <summary>
    /// Verify 2FA code to complete login
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<VerifyTwoFactorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VerifyTwoFactorResponse>> VerifyTwoFactor(
        [FromBody] VerifyTwoFactorRequest request)
    {
        try
        {
            string ipAddress = GetClientIpAddress();
            string? location = await _locationService.GetLocationFromIpAsync(ipAddress);

            VerifyTwoFactorCommand command = new()
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

            VerifyTwoFactorResponse response = await _mediator.Send(command);
            return OkResponse(response, "2FA verification successful");
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_2FA_CODE");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred during 2FA verification", "2FA_VERIFICATION_ERROR");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <remarks>
    /// Generates new access token and refresh token pair using valid refresh token.
    /// </remarks>
    /// <response code="200">Token refresh successful - Returns {success: true, message: string, result: LoginResponse}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="401">Unauthorized - Invalid refresh token - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="500">Internal server error - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            RefreshTokenCommand command = new()
            {
                RefreshToken = request.RefreshToken,
                IpAddress = GetClientIpAddress(),
                Location = null // Could parse from IP using GeoIP service
            };

            LoginResponse response = await _mediator.Send(command);
            return OkResponse(response, "Token refreshed successfully");
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_REFRESH_TOKEN");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred during token refresh", "TOKEN_REFRESH_ERROR");
        }
    }

    /// <summary>
    /// Logout from current device
    /// Device is automatically detected from request headers or cookies
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [RequireDeviceId]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Logout()
    {
        try
        {
            // Extract access token from Authorization header
            string accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Extract token expiration from JWT
            DateTime? expirationTime = ExtractTokenExpiration(accessToken);

            string deviceId = GetDeviceId();

            // This is a fallback - client should ideally store and send device_id
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogWarning("No device_id found in cookie or header. Logout may fail to find correct device.");
                // Generate one for now, but this may not match stored device
                deviceId = GenerateDeviceId();
            }

            LogoutCommand command = new()
            {
                RefreshToken = null, // Will be handled by deviceId
                DeviceId = deviceId,
                IpAddress = GetClientIpAddress(),
                AccessToken = accessToken, // Pass token to blacklist it
                AccessTokenExpiration = expirationTime // Pass expiration time
            };

            await _mediator.Send(command);
            return OkResponse(new { logoutTime = DateTime.UtcNow }, "Logged out successfully");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred during logout", "LOGOUT_ERROR");
        }
    }

    /// <summary>
    /// Logout from all devices
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    [RequireDeviceId]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> LogoutAll([FromBody] LogoutAllRequest? request = null)
    {
        try
        {
            long userId = GetCurrentUserId();
            string currentDeviceId = GenerateDeviceId();

            LogoutAllDevicesCommand command = new()
            {
                UserId = userId,
                ExceptDeviceId = request?.ExceptCurrentDevice == true ? currentDeviceId : null
            };

            await _mediator.Send(command);
            return OkResponse("Logged out from all devices successfully");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred during logout", "LOGOUT_ALL_ERROR");
        }
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            long userId = GetCurrentUserId();

            ChangePasswordCommand command = new()
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
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_PASSWORD");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while changing password", "PASSWORD_CHANGE_ERROR");
        }
    }

    /// <summary>
    /// Get authentication methods for current user
    /// </summary>
    [HttpGet("authentication-methods")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<AuthenticationMethodsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthenticationMethodsResponse>> GetAuthenticationMethods()
    {
        try
        {
            long userId = GetCurrentUserId();
            GetAuthenticationMethodsQuery query = new() { UserId = userId };
            AuthenticationMethodsResponse response = await _mediator.Send(query);
            return OkResponse(response);
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while fetching authentication methods",
                "GET_AUTH_METHODS_ERROR");
        }
    }

    /// <summary>
    /// Select authentication method for 2FA
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.AuthenticationPolicy)]
    [HttpPost("select-authentication-method")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<SelectAuthenticationMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SelectAuthenticationMethodResponse>> SelectAuthenticationMethod(
        [FromBody] SelectAuthenticationMethodRequest request)
    {
        try
        {
            SelectAuthenticationMethodCommand command = new()
            {
                TwoFactorSessionToken = request.TwoFactorSessionToken,
                SelectedMethod = request.SelectedMethod
            };

            SelectAuthenticationMethodResponse response = await _mediator.Send(command);
            return OkResponse(response, "Authentication method selected successfully");
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_SESSION_TOKEN");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while selecting authentication method",
                "SELECT_AUTH_METHOD_ERROR");
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
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpRequest request)
    {
        try
        {
            VerifyEmailOtpLoginCommand command = new()
            {
                EmailOtp = request.EmailOtp,
                Email = request.Email
            };

            VerifyEmailOtpLoginResponse response = await _mediator.Send(command);
            return OkResponse(response, "Email OTP verification successful");
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while verifying OTP", "EMAIL_OTP_ERROR");
        }
    }

    /// <summary>
    /// Verify recovery code
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("verify-recovery-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<VerifyTwoFactorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> VerifyRecoveryCode([FromBody] VerifyRecoveryCodeRequest request)
    {
        try
        {
            string ipAddress = GetClientIpAddress();
            string? location = await _locationService.GetLocationFromIpAsync(ipAddress);

            VerifyRecoveryCodeCommand command = new()
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

            VerifyTwoFactorResponse response = await _mediator.Send(command);
            return OkResponse(response, "Recovery code verification successful");
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_RECOVERY_CODE");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while verifying recovery code", "RECOVERY_CODE_ERROR");
        }
    }

    /// <summary>
    /// Request password reset (forgot password)
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.AuthenticationPolicy)]
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<ForgotPasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        try
        {
            ForgotPasswordCommand command = new() { Email = request.Email };
            ForgotPasswordResponse response = await _mediator.Send(command);
            return OkResponse(response, ErrorMessages.GetMessage(ErrorCodes.AUTH_RESET_EMAIL_SENT));
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while processing password reset", "FORGOT_PASSWORD_ERROR");
        }
    }

    /// <summary>
    /// Verify reset token
    /// </summary>
    [ProducesResponseType(typeof(ApiSuccessResponse<VerifyResetTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("verify-reset-token")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyResetTokenResponse>> VerifyResetToken(
        [FromBody] VerifyResetTokenRequest request)
    {
        try
        {
            VerifyResetTokenCommand command = new()
            {
                Email = request.Email,
                ResetToken = request.ResetToken
            };
            VerifyResetTokenResponse response = await _mediator.Send(command);
            return OkResponse(response, ErrorMessages.GetMessage(ErrorCodes.AUTH_RESET_TOKEN_VALID));
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while verifying token", "TOKEN_VERIFICATION_ERROR");
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request)
    {
        try
        {
            ResetPasswordCommand command = new()
            {
                Email = request.Email,
                ResetToken = request.ResetToken,
                NewPassword = request.NewPassword
            };
            ResetPasswordResponse response = await _mediator.Send(command);
            return OkResponse(response, ErrorMessages.GetMessage(ErrorCodes.AUTH_PASSWORD_RESET_SUCCESS));
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while resetting password", "PASSWORD_RESET_ERROR");
        }
    }

    /// <summary>
    /// Enable 2FA - generates new secret and QR code
    /// </summary>
    [HttpPost("enable-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<Enable2FAResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Enable2FAResponse>> Enable2FA([FromBody] Enable2FARequest request)
    {
        try
        {
            long userId = GetCurrentUserId();

            Enable2FACommand command = new()
            {
                UserId = userId,
                Password = request.Password
            };

            Enable2FAResponse response = await _mediator.Send(command);
            return OkResponse(response);
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_PASSWORD");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while enabling 2FA", "ENABLE_2FA_ERROR");
        }
    }

    /// <summary>
    /// Confirm 2FA setup and receive backup codes
    /// </summary>
    [HttpPost("confirm-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<Confirm2FAResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Confirm2FAResponse>> Confirm2FA([FromBody] Confirm2FARequest request)
    {
        try
        {
            long userId = GetCurrentUserId();

            Confirm2FACommand command = new()
            {
                UserId = userId,
                Secret = request.Secret,
                Code = request.Code
            };

            Confirm2FAResponse response = await _mediator.Send(command);
            return OkResponse(response, "Two-factor authentication enabled successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "UNAUTHORIZED");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_2FA_CODE");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while confirming 2FA", "CONFIRM_2FA_ERROR");
        }
    }

    /// <summary>
    /// Disable 2FA
    /// </summary>
    [HttpPost("disable-2fa")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Disable2FA([FromBody] Disable2FARequest request)
    {
        try
        {
            long userId = GetCurrentUserId();

            Disable2FACommand command = new()
            {
                UserId = userId,
                Password = request.Password
            };

            await _mediator.Send(command);
            return OkResponse("2FA disabled successfully");
        }
        catch (UnauthorizedException ex)
        {
            return UnauthorizedResponse(ex.Message, ex.ErrorCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_PASSWORD");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while disabling 2FA", "DISABLE_2FA_ERROR");
        }
    }

    /// <summary>
    /// Disable 2FA using backup code (for account recovery when device is lost)
    /// This endpoint does not require authentication - only username, password, and backup code
    /// </summary>
    [HttpPost("disable-2fa-with-backup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DisableTwoFactorWithBackup(
        [FromBody] DisableTwoFactorWithBackupRequest request)
    {
        try
        {
            DisableTwoFactorWithBackupCommand command = new()
            {
                Username = request.Username,
                Password = request.Password,
                BackupCode = request.BackupCode
            };

            await _mediator.Send(command);
            return OkResponse(
                "Two-factor authentication has been disabled successfully. You can now login without 2FA.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "INVALID_CREDENTIALS_OR_CODE");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_BACKUP_CODE");
        }
        catch (Exception)
        {
            return InternalErrorResponse("An error occurred while disabling 2FA", "DISABLE_2FA_BACKUP_ERROR");
        }
    }

    /// <summary>
    /// Get current user profile information
    /// Requires device_id to prevent accessing user info without active device session
    [ProducesResponseType(typeof(ApiSuccessResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [RequireDeviceId]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        long userId = GetCurrentUserId();
        string deviceId = GetDeviceId();
        string ipAddress = GetClientIpAddress();

        // Update last activity for current device
        if (!string.IsNullOrEmpty(deviceId))
        {
            await _unitOfWork.UserDevices.UpdateLastActivityAsync(deviceId, ipAddress);
            await _unitOfWork.SaveChangesAsync();
        }

        GetUserByIdQuery query = new(userId);
        UserDto? user = await _mediator.Send(query);

        if (user == null)
        {
            return NotFoundResponse("User not found", "USER_NOT_FOUND");
        }

        return OkResponse(user, "User profile retrieved successfully");
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
        string? cfConnectingIp = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfConnectingIp))
        {
            return cfConnectingIp.Trim();
        }

        // Cloudflare Enterprise header
        string? trueClientIp = Request.Headers["True-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(trueClientIp))
        {
            return trueClientIp.Trim();
        }

        // X-Real-IP from Nginx or other reverse proxy
        string? realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // X-Forwarded-For (can contain multiple IPs: client, proxy1, proxy2)
        // Take the first IP which is the original client
        string? forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            string[] ips = forwardedFor.Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ips.Length > 0)
            {
                string clientIp = ips[0];
                // Validate it's a proper IP address
                if (IPAddress.TryParse(clientIp, out _))
                {
                    return clientIp;
                }
            }
        }

        // X-Client-IP header
        string? clientIp2 = Request.Headers["X-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clientIp2))
        {
            return clientIp2.Trim();
        }

        // Fallback to RemoteIpAddress (direct connection, no proxy)
        IPAddress? remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // Handle IPv6 loopback (::1) and map it to IPv4
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            // Convert IPv6 loopback to IPv4
            if (remoteIp.ToString() == "::1")
            {
                return "127.0.0.1";
            }

            return remoteIp.ToString();
        }

        return "Unknown";
    }

    private string GenerateDeviceId()
    {
        // Generate a unique device ID based on user agent and IP
        string userAgent = Request.Headers["User-Agent"].ToString();
        string ip = GetClientIpAddress();
        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes($"{userAgent}_{ip}_{DateTime.UtcNow.Ticks}"));
        return Convert.ToBase64String(hash)[..32];
    }

    /// <summary>
    /// Get device ID from request headers or cookies
    /// Returns the device_id if present in headers or cookies, otherwise returns empty string
    /// </summary>
    private string GetDeviceId()
    {
        string deviceId = Request.Headers["x-device-id"].ToString();
        if (!string.IsNullOrEmpty(deviceId))
        {
            return deviceId;
        }

        return string.Empty;
    }

    private DateTime? ExtractTokenExpiration(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return null;
        }

        try
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken? jwtToken = handler.ReadJwtToken(accessToken);
            return jwtToken.ValidTo;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private string GetDeviceName()
    {
        string userAgent = Request.Headers["User-Agent"].ToString();

        // Simple device name extraction from User-Agent
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg"))
        {
            return "Chrome Browser";
        }

        if (userAgent.Contains("Firefox"))
        {
            return "Firefox Browser";
        }

        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
        {
            return "Safari Browser";
        }

        if (userAgent.Contains("Edg"))
        {
            return "Edge Browser";
        }

        if (userAgent.Contains("Postman"))
        {
            return "Postman";
        }

        if (userAgent.Contains("curl"))
        {
            return "curl";
        }

        return "Unknown Device";
    }

    private string GetDeviceType()
    {
        string userAgent = Request.Headers["User-Agent"].ToString();

        // Determine device type from User-Agent
        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
        {
            return "mobile";
        }

        if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
        {
            return "tablet";
        }

        return "desktop";
    }

    #endregion

    #region Session & Theme Management

    /// <summary>
    /// Get current user's login sessions
    /// </summary>
    [HttpGet("me/sessions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<IReadOnlyList<UserSessionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMySessions()
    {
        long userId = GetCurrentUserId();
        GetUserSessionsQuery query = new(userId);
        IReadOnlyList<UserSessionDto> result = await _mediator.Send(query);

        return OkResponse(result);
    }

    /// <summary>
    /// Delete a specific login session
    /// </summary>
    [HttpDelete("me/sessions/{sessionId:guid}")]
    [Authorize]
    [RequireDeviceId]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSession(Guid sessionId)
    {
        try
        {
            long userId = GetCurrentUserId();
            string currentDeviceId = GetDeviceId();

            // Extract access token from Authorization header
            string accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            DateTime? expirationTime = ExtractTokenExpiration(accessToken);

            DeleteSessionCommand command = new(userId, sessionId, currentDeviceId, accessToken, expirationTime);
            await _mediator.Send(command);

            return NoContent();
        }
        catch (DomainException ex)
        {
            return NotFoundResponse(ex.Message, ex.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}: {Message}", sessionId, ex.Message);
            return InternalErrorResponse("An error occurred while deleting session", "DELETE_SESSION_ERROR");
        }
    }

    /// <summary>
    /// Delete all login sessions except current device
    /// </summary>
    /// <remarks>
    /// Requires X-Device-Id header to identify the current device that should be kept active.
    /// All other sessions will be terminated and their tokens blacklisted.
    /// </remarks>
    [HttpDelete("me/sessions")]
    [Authorize]
    [RequireDeviceId]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAllSessions()
    {
        long userId = GetCurrentUserId();
        string currentDeviceId = GetDeviceId();

        DeleteAllSessionsCommand command = new(userId, currentDeviceId);
        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Get current user's theme preferences
    /// </summary>
    [HttpGet("me/theme")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserThemeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyTheme()
    {
        long userId = GetCurrentUserId();
        GetUserThemeQuery query = new(userId);
        GetUserThemeResponse? result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFoundResponse("Theme not found. Using default theme.");
        }

        UserThemeResponse response = new(
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

        return OkResponse(response);
    }

    /// <summary>
    /// Update current user's theme preferences
    /// </summary>
    [HttpPut("me/theme")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResponse<UserThemeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMyTheme([FromBody] UpdateUserThemeRequest request)
    {
        long userId = GetCurrentUserId();
        UpdateUserThemeCommand command = new(
            userId,
            request.Theme,
            request.PrimaryColor,
            request.Transparency,
            request.BorderRadius,
            request.DarkTheme,
            request.PinNavbar,
            request.CompactMode
        );

        UpdateUserThemeResponse result = await _mediator.Send(command);

        UserThemeResponse response = new(
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

        return OkResponse(response);
    }

    #endregion
}
