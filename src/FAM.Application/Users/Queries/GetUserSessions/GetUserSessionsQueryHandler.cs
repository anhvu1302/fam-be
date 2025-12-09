using FAM.Application.Users.Shared;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Users.Queries.GetUserSessions;

public class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, IReadOnlyList<UserSessionDto>>
{
    private readonly IUserDeviceRepository _userDeviceRepository;

    public GetUserSessionsQueryHandler(IUserDeviceRepository userDeviceRepository)
    {
        _userDeviceRepository = userDeviceRepository;
    }

    public async Task<IReadOnlyList<UserSessionDto>> Handle(GetUserSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var devices = await _userDeviceRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var sessions = devices
            .OrderByDescending(d => d.LastLoginAt)
            .Select(d => new UserSessionDto(
                d.Id,
                d.DeviceId,
                d.DeviceName,
                d.DeviceType,
                d.IpAddress?.Value,
                d.Location,
                d.Browser,
                d.OperatingSystem,
                d.LastLoginAt,
                d.LastActivityAt,
                d.IsActive,
                d.IsTrusted,
                false // Will be set by controller based on current device
            ))
            .ToList();

        return sessions;
    }
}