using FAM.Domain.Abstractions;
using FAM.Domain.Users;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.Logout;

/// <summary>
/// Handler for LogoutAllDevicesCommand - logs out user from all devices
/// </summary>
public class LogoutAllDevicesCommandHandler : IRequestHandler<LogoutAllDevicesCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly ILogger<LogoutAllDevicesCommandHandler> _logger;

    public LogoutAllDevicesCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenBlacklistService tokenBlacklistService,
        ILogger<LogoutAllDevicesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenBlacklistService = tokenBlacklistService;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogoutAllDevicesCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        User? user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null) throw new KeyNotFoundException($"User with ID {request.UserId} not found");

        // Deactivate all user devices (except current device if specified)
        await _unitOfWork.UserDevices.DeactivateAllUserDevicesAsync(
            request.UserId,
            request.ExceptDeviceId,
            cancellationToken);

        // Blacklist all tokens for this user to invalidate them immediately
        await _tokenBlacklistService.BlacklistUserTokensAsync(request.UserId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
