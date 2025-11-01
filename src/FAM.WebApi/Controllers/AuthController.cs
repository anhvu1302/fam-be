using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Application.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Authentication controller - handles login, logout, token refresh, password change
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
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
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var location = await _locationService.GetLocationFromIpAsync(ipAddress);

            var command = new LoginCommand
            {
                Username = request.Username,
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
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed for username: {Username}", request.Username);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Verify 2FA code to complete login
    /// </summary>
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyTwoFactorResponse>> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
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
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "2FA verification failed");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification");
            return StatusCode(500, new { message = "An error occurred during 2FA verification" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken,
                IpAddress = GetClientIpAddress(),
                Location = null // Could parse from IP using GeoIP service
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
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
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
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
            return Ok(new { message = "Logged out from all devices successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout all");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
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
            return Ok(new { message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Password change failed for user: {UserId}", GetCurrentUserId());
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change for user: {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "An error occurred during password change" });
        }
    }

    /// <summary>
    /// Enable 2FA - generates new secret and QR code
    /// </summary>
    [HttpPost("enable-2fa")]
    [Authorize]
    public async Task<ActionResult<Enable2FAResponse>> Enable2FA([FromBody] Enable2FARequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var command = new Enable2FACommand
            {
                UserId = userId,
                Password = request.Password
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Enable 2FA failed for user: {UserId}", GetCurrentUserId());
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enable 2FA for user: {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "An error occurred while enabling 2FA" });
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
    public async Task<ActionResult<Confirm2FAResponse>> Confirm2FA([FromBody] Confirm2FARequest request)
    {
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
            
            // Return structured response with clear format
            return Ok(new
            {
                success = response.Success,
                message = response.Message,
                data = new
                {
                    backupCodes = response.BackupCodes,
                    backupCodesCount = response.BackupCodes.Count,
                    instructions = new
                    {
                        title = "⚠️ Save Your Backup Codes",
                        message = "These codes are shown only once and cannot be recovered.",
                        usage = "Each code can be used once to recover your account if you lose access to your authenticator app.",
                        format = "Example: 881eb-53018"
                    }
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Confirm 2FA failed for user: {UserId}", GetCurrentUserId());
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid 2FA code for user: {UserId}", GetCurrentUserId());
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during confirm 2FA for user: {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "An error occurred while confirming 2FA" });
        }
    }

    /// <summary>
    /// Disable 2FA
    /// </summary>
    [HttpPost("disable-2fa")]
    [Authorize]
    public async Task<ActionResult> Disable2FA([FromBody] Disable2FARequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var command = new Disable2FACommand
            {
                UserId = userId,
                Password = request.Password
            };

            await _mediator.Send(command);
            return Ok(new { message = "2FA disabled successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Disable 2FA failed for user: {UserId}", GetCurrentUserId());
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disable 2FA for user: {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "An error occurred during disable 2FA" });
        }
    }

    /// <summary>
    /// Disable 2FA using backup code (for account recovery when device is lost)
    /// This endpoint does not require authentication - only username, password, and backup code
    /// </summary>
    [HttpPost("disable-2fa-with-backup")]
    [AllowAnonymous]
    public async Task<ActionResult> DisableTwoFactorWithBackup([FromBody] DisableTwoFactorWithBackupRequest request)
    {
        try
        {
            var command = new DisableTwoFactorWithBackupCommand
            {
                Username = request.Username,
                Password = request.Password,
                BackupCode = request.BackupCode
            };

            await _mediator.Send(command);
            return Ok(new { 
                message = "Two-factor authentication has been disabled successfully using backup code. You can now login without 2FA.",
                recommendation = "For security, we recommend re-enabling 2FA after logging in."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Disable 2FA with backup code failed for username: {Username}", request.Username);
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during disable 2FA with backup for username: {Username}", request.Username);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disable 2FA with backup for username: {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred while disabling 2FA" });
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

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }

    private string GetClientIpAddress()
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
        if (!string.IsNullOrEmpty(cfConnectingIp))
        {
            return cfConnectingIp.Trim();
        }

        // Cloudflare Enterprise header
        var trueClientIp = Request.Headers["True-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(trueClientIp))
        {
            return trueClientIp.Trim();
        }

        // X-Real-IP from Nginx or other reverse proxy
        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

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
                if (System.Net.IPAddress.TryParse(clientIp, out _))
                {
                    return clientIp;
                }
            }
        }

        // X-Client-IP header
        var clientIp2 = Request.Headers["X-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clientIp2))
        {
            return clientIp2.Trim();
        }

        // Fallback to RemoteIpAddress (direct connection, no proxy)
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
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
        var userAgent = Request.Headers["User-Agent"].ToString();
        var ip = GetClientIpAddress();
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{userAgent}_{ip}_{DateTime.UtcNow.Ticks}"));
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
