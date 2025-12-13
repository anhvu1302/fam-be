namespace FAM.Application.Users.Shared;

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
