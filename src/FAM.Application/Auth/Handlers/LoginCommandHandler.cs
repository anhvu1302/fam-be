using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Handlers;

/// <summary>
/// Handler for LoginCommand - authenticates user and generates tokens
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by username or email
        var user = await _unitOfWork.Users.FindByIdentityAsync(request.Identity, cancellationToken);

        if (user == null)
            // TODO: Raise UserLoginFailedEvent
            throw new UnauthorizedAccessException("Invalid username/email or password");

        // Check if account is locked
        if (user.IsLockedOut())
        {
            var lockoutEnd = user.LockoutEnd!.Value;
            var minutesRemaining = (int)(lockoutEnd - DateTime.UtcNow).TotalMinutes + 1;
            throw new UnauthorizedAccessException($"Account is locked. Try again in {minutesRemaining} minutes.");
        }

        // Check if account is active
        if (!user.IsActive) throw new UnauthorizedAccessException("Account is inactive");

        // Verify password
        var isPasswordValid = user.Password.Verify(request.Password);

        if (!isPasswordValid)
        {
            // Record failed login attempt
            user.RecordFailedLogin();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // TODO: Raise UserLoginFailedEvent

            throw new UnauthorizedAccessException("Invalid username/email or password");
        }

        // TODO: Check if email is verified (Phase 2)
        // if (!user.IsEmailVerified)
        // {
        //     throw new UnauthorizedAccessException("Email not verified. Please verify your email before logging in.");
        // }

        // Check if Two-Factor Authentication is enabled
        if (user.TwoFactorEnabled)
        {
            // Generate a temporary session token for 2FA verification
            var twoFactorSessionToken = GenerateTwoFactorSessionToken(user.Id);

            return new LoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorSessionToken = twoFactorSessionToken,
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

        // Generate tokens
        var roles = new List<string>(); // TODO: Load user roles from UserNodeRoles
        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username.Value, user.Email.Value, roles);
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

    private string GenerateTwoFactorSessionToken(long userId)
    {
        // Generate a temporary JWT token for 2FA session using access token method
        // We'll use a special username to indicate this is a 2FA session token
        var roles = new List<string> { "2fa_session" };
        return _jwtService.GenerateAccessToken(userId, $"2fa_session_{userId}", "", roles);
    }
}