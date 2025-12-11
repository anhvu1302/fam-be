using FAM.Application.Auth.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
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
        var device = await _userDeviceRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (device == null || device.UserId != request.UserId)
            throw new DomainException(ErrorCodes.USER_SESSION_NOT_FOUND, "Session not found or access denied.");

        // Blacklist the active access token using stored JTI before deleting the session
        if (!string.IsNullOrEmpty(device.ActiveAccessTokenJti))
        {
            // Calculate expiration time using config (add extra buffer for clock skew)
            var tokenExpiryTime = DateTime.UtcNow.AddHours(2); // Conservative estimate
            
            await _tokenBlacklistService.BlacklistTokenByJtiAsync(
                device.ActiveAccessTokenJti,
                tokenExpiryTime,
                cancellationToken);
        }
        
        // Also blacklist the current access token if provided (for immediate revocation)
        if (!string.IsNullOrEmpty(request.AccessToken) && request.AccessTokenExpiration.HasValue)
        {
            await _tokenBlacklistService.BlacklistTokenAsync(
                request.AccessToken,
                request.AccessTokenExpiration.Value,
                cancellationToken);
        }

        // Delete the device session entirely
        _userDeviceRepository.Delete(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}