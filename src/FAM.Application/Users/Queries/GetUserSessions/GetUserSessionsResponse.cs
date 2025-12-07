namespace FAM.Application.Users.Queries.GetUserSessions;

public record GetUserSessionsResponse(List<UserSessionDto> Sessions);

public record UserSessionDto(
    Guid Id,
    string DeviceId,
    string DeviceName,
    string DeviceType,
    string? IpAddress,
    string? Location,
    string? Browser,
    string? OperatingSystem,
    DateTime LastLoginAt,
    DateTime? LastActivityAt,
    bool IsActive,
    bool IsTrusted,
    bool IsCurrentDevice
);
