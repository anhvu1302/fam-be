using FAM.Domain.Common;

namespace FAM.Domain.Users.Events;

/// <summary>
/// Domain event raised when user successfully logs in
/// </summary>
public sealed record UserLoggedInEvent : DomainEvent
{
    public long UserId { get; init; }
    public string DeviceId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime LoginAt { get; init; }

    public UserLoggedInEvent(long userId, string deviceId, string? ipAddress, string? userAgent)
    {
        UserId = userId;
        DeviceId = deviceId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        LoginAt = DateTime.UtcNow;
    }
}