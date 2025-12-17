using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users.Entities;

using MediatR;

namespace FAM.Application.Users.Commands.DeleteSession;

public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, Unit>
{
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    public DeleteSessionCommandHandler(
        IUserDeviceRepository userDeviceRepository,
        IUnitOfWork unitOfWork,
        ITokenBlacklistService tokenBlacklistService)
    {
        _userDeviceRepository = userDeviceRepository;
        _unitOfWork = unitOfWork;
        _tokenBlacklistService = tokenBlacklistService;
    }

    public async Task<Unit> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        // SECURITY: Verify current device is trusted for at least 3 days before allowing deletion
        UserDevice? currentDevice =
            await _userDeviceRepository.GetByDeviceIdAsync(request.CurrentDeviceId, cancellationToken);

        if (currentDevice == null)
            throw new DomainException(
                ErrorCodes.DEVICE_NOT_FOUND,
                "Current device not found. Please log in again.");

        if (!currentDevice.IsTrustedForDuration(DomainRules.DeviceTrust.MinimumTrustDaysForSensitiveOperations))
            throw new DomainException(
                ErrorCodes.DEVICE_NOT_TRUSTED_FOR_OPERATION,
                $"Device must be trusted for at least {DomainRules.DeviceTrust.MinimumTrustDaysForSensitiveOperations} days to delete other sessions.");

        UserDevice? device = await _userDeviceRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (device == null || device.UserId != request.UserId)
            throw new DomainException(ErrorCodes.USER_SESSION_NOT_FOUND, "Session not found or access denied.");

        // Blacklist the active access token using stored JTI before deleting the session
        if (!string.IsNullOrEmpty(device.ActiveAccessTokenJti))
        {
            // Calculate expiration time using config (add extra buffer for clock skew)
            DateTime tokenExpiryTime = DateTime.UtcNow.AddHours(2); // Conservative estimate

            await _tokenBlacklistService.BlacklistTokenByJtiAsync(
                device.ActiveAccessTokenJti,
                tokenExpiryTime,
                cancellationToken);
        }

        // Also blacklist the current access token if provided (for immediate revocation)
        if (!string.IsNullOrEmpty(request.AccessToken) && request.AccessTokenExpiration.HasValue)
            await _tokenBlacklistService.BlacklistTokenAsync(
                request.AccessToken,
                request.AccessTokenExpiration.Value,
                cancellationToken);

        // Delete the device session entirely
        _userDeviceRepository.Delete(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
