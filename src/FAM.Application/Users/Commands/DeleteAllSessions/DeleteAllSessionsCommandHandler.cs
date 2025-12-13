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

    public DeleteAllSessionsCommandHandler(
        IUserDeviceRepository userDeviceRepository,
        IUnitOfWork unitOfWork,
        ITokenBlacklistService tokenBlacklistService)
    {
        _userDeviceRepository = userDeviceRepository;
        _unitOfWork = unitOfWork;
        _tokenBlacklistService = tokenBlacklistService;
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
                throw new DomainException(
                    ErrorCodes.DEVICE_NOT_FOUND,
                    "Current device not found. Please log in again.");
            }

            if (!currentDevice.IsTrustedForDuration(DomainRules.DeviceTrust.MinimumTrustDaysForSensitiveOperations))
            {
                throw new DomainException(
                    ErrorCodes.DEVICE_NOT_TRUSTED_FOR_OPERATION,
                    $"Device must be trusted for at least {DomainRules.DeviceTrust.MinimumTrustDaysForSensitiveOperations} days to delete other sessions.");
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
            }
        }

        // Deactivate all devices (except current if specified)
        await _userDeviceRepository.DeactivateAllUserDevicesAsync(
            request.UserId,
            request.ExcludeDeviceId,
            cancellationToken);

        // Also blacklist all tokens at user level for double protection
        await _tokenBlacklistService.BlacklistUserTokensAsync(request.UserId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
