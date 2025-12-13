using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users.Entities;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Users.Commands.DeleteAllSessions;

public class DeleteAllSessionsCommandHandler : IRequestHandler<DeleteAllSessionsCommand, Unit>
{
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly ILogger<DeleteAllSessionsCommandHandler> _logger;
    private const int MinimumTrustDaysForDeletion = 3;

    public DeleteAllSessionsCommandHandler(
        IUserDeviceRepository userDeviceRepository,
        IUnitOfWork unitOfWork,
        ITokenBlacklistService tokenBlacklistService,
        ILogger<DeleteAllSessionsCommandHandler> logger)
    {
        _userDeviceRepository = userDeviceRepository;
        _unitOfWork = unitOfWork;
        _tokenBlacklistService = tokenBlacklistService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteAllSessionsCommand request, CancellationToken cancellationToken)
    {
        // SECURITY: Verify current device is trusted for at least 3 days before allowing this operation
        if (!string.IsNullOrEmpty(request.ExcludeDeviceId))
        {
            UserDevice? currentDevice =
                await _userDeviceRepository.GetByDeviceIdAsync(request.ExcludeDeviceId, cancellationToken);

            if (currentDevice == null)
            {
                _logger.LogWarning("Device {DeviceId} not found for user {UserId}", request.ExcludeDeviceId,
                    request.UserId);
                throw new DomainException(
                    ErrorCodes.DEVICE_NOT_FOUND,
                    "Current device not found. Please log in again.");
            }

            if (!currentDevice.IsTrustedForDuration(MinimumTrustDaysForDeletion))
            {
                _logger.LogWarning(
                    "User {UserId} attempted to delete all sessions from untrusted device {DeviceId} (created: {Created}, trusted: {IsTrusted})",
                    request.UserId, request.ExcludeDeviceId, currentDevice.CreatedAt, currentDevice.IsTrusted);

                throw new DomainException(
                    ErrorCodes.DEVICE_NOT_TRUSTED_FOR_OPERATION,
                    $"For security reasons, this device must be trusted for at least {MinimumTrustDaysForDeletion} days before you can delete other sessions. " +
                    "Please use a trusted device or contact support if you've lost access to your account.");
            }
        }

        // Get all active devices to blacklist their access tokens
        IEnumerable<UserDevice> allDevices = await _userDeviceRepository.GetActiveDevicesByUserIdAsync(
            request.UserId,
            cancellationToken);

        // Blacklist access tokens for each device (except current if specified)
        // Use conservative estimate for token expiry (add buffer for clock skew)
        DateTime tokenExpiryTime = DateTime.UtcNow.AddHours(2);
        foreach (UserDevice device in allDevices)
        {
            // Skip current device if specified
            if (!string.IsNullOrEmpty(request.ExcludeDeviceId) && device.DeviceId == request.ExcludeDeviceId)
                continue;

            // Blacklist the stored active access token JTI
            if (!string.IsNullOrEmpty(device.ActiveAccessTokenJti))
            {
                await _tokenBlacklistService.BlacklistTokenByJtiAsync(
                    device.ActiveAccessTokenJti,
                    tokenExpiryTime,
                    cancellationToken);

                _logger.LogInformation(
                    "Blacklisted access token with JTI {JTI} for device {DeviceId} of user {UserId}",
                    device.ActiveAccessTokenJti, device.DeviceId, request.UserId);
            }
        }

        // Deactivate all devices (except current if specified)
        await _userDeviceRepository.DeactivateAllUserDevicesAsync(
            request.UserId,
            request.ExcludeDeviceId,
            cancellationToken);

        // Also blacklist all tokens at user level for double protection
        await _tokenBlacklistService.BlacklistUserTokensAsync(request.UserId, cancellationToken);
        _logger.LogInformation("All tokens blacklisted for user {UserId}", request.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
