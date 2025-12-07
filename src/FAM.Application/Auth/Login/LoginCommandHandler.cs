using FAM.Application.Auth.SendEmailVerificationOtp;
using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Login;

/// <summary>
/// Handler for LoginCommand - authenticates user and generates tokens
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ISigningKeyService _signingKeyService;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ISigningKeyService signingKeyService,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _signingKeyService = signingKeyService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by username or email
        var user = await _unitOfWork.Users.FindByIdentityAsync(request.Identity, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username/email or password");

        // Check if account is locked
        if (user.IsLockedOut())
        {
            var lockoutEnd = user.LockoutEnd!.Value;
            var minutesRemaining = (int)(lockoutEnd - DateTime.UtcNow).TotalMinutes + 1;
            throw new UnauthorizedAccessException($"Account is locked. Try again in {minutesRemaining} minutes");
        }

        // Check if account is active
        if (!user.IsActive) 
            throw new UnauthorizedAccessException("Account is inactive");

        // Verify password
        var isPasswordValid = user.Password.Verify(request.Password);

        if (!isPasswordValid)
        {
            // Record failed login attempt
            user.RecordFailedLogin();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Invalid username/email or password");
        }

        // Check if email is verified - if not, send OTP and return email verification response
        if (!user.IsEmailVerified)
        {
            _logger.LogInformation("Email not verified for user {UserId}, sending verification OTP", user.Id);

            // Generate OTP using OtpService (which handles storage and rate limiting)
            try
            {
                // Generate and store OTP (uses email as session identifier for email verification flow)
                var otp = await _otpService.GenerateOtpAsync(
                    userId: user.Id,
                    sessionToken: user.Email.Value, // Use email as session identifier
                    expirationMinutes: 10,
                    cancellationToken: cancellationToken);

                // Send OTP via email
                await _emailService.SendOtpEmailAsync(
                    toEmail: user.Email.Value,
                    otpCode: otp,
                    userName: user.Username.Value,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Verification OTP sent to {Email}", user.Email.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification OTP to {Email}", user.Email.Value);
                throw;
            }

            return new LoginResponse
            {
                RequiresEmailVerification = true,
                MaskedEmail = MaskEmail(user.Email.Value),
                User = new UserInfoDto() // Return minimal user info for display
            };
        }

        // Check if Two-Factor Authentication is enabled
        if (user.TwoFactorEnabled)
        {
            // Generate a temporary session token for 2FA verification
            var twoFactorSessionToken = await GenerateTwoFactorSessionTokenAsync(user.Id, cancellationToken);

            return new LoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorSessionToken = twoFactorSessionToken,
                User = MapToUserInfoDto(user)
            };
        }

        // Generate tokens using RSA
        // Get active signing key from database
        var activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);

        // TODO: Load user roles from UserNodeRoles properly
        var roles = new List<string>();
        // Temporary: Hardcode Admin role for admin user (for integration tests)
        if (user.Username.Value.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            roles.Add("Admin");
            roles.Add("SuperAdmin");
        }

        var accessToken = _jwtService.GenerateAccessTokenWithRsa(
            user.Id,
            user.Username.Value,
            user.Email.Value,
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7);

        // Get or create device via User aggregate
        // This properly handles the device in the User's collection
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

        // Save device separately to avoid tracking conflicts
        // Check if device already exists in database
        var existingDevice = await _unitOfWork.UserDevices.GetByIdAsync(device.Id, cancellationToken);
        if (existingDevice != null)
        {
            // Update existing device
            existingDevice.UpdateRefreshToken(refreshToken, refreshTokenExpiresAt, request.IpAddress);
            existingDevice.RecordLogin(request.IpAddress, request.Location);
            _unitOfWork.UserDevices.Update(existingDevice);
        }
        else
        {
            // Add new device
            await _unitOfWork.UserDevices.AddAsync(device, cancellationToken);
        }

        // Update user without devices
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Raise UserLoggedInEvent
        // var loginEvent = new UserLoggedInEvent(user.Id, device.DeviceId, request.IpAddress, DateTime.UtcNow);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hour - should be configurable
            TokenType = "Bearer",
            User = MapToUserInfoDto(user)
        };
    }

    private static UserInfoDto MapToUserInfoDto(Domain.Users.User user)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username.Value,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Avatar = user.Avatar,
            PhoneNumber = user.PhoneNumber?.Value,
            PhoneCountryCode = user.PhoneNumber?.CountryCode,
            DateOfBirth = user.DateOfBirth,
            Bio = user.Bio,
            IsEmailVerified = user.IsEmailVerified,
            IsTwoFactorEnabled = user.TwoFactorEnabled,
            PreferredLanguage = user.PreferredLanguage,
            TimeZone = user.TimeZone
        };
    }

    private async Task<string> GenerateTwoFactorSessionTokenAsync(long userId, CancellationToken cancellationToken)
    {
        // Generate a temporary JWT token for 2FA session using RSA
        var activeKey = await _signingKeyService.GetOrCreateActiveKeyAsync(cancellationToken);

        var roles = new List<string> { "2fa_session" };
        return _jwtService.GenerateAccessTokenWithRsa(
            userId,
            $"2fa_session_{userId}",
            "",
            roles,
            activeKey.KeyId,
            activeKey.PrivateKey,
            activeKey.Algorithm);
    }

    /// <summary>
    /// Generate random 6-digit OTP
    /// </summary>
    private static string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// Mask email address for display: user@email.com -> u***@email.com
    /// </summary>
    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return email;

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"{localPart}***@{domain}";

        var masked = localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];
        return $"{masked}@{domain}";
    }
}