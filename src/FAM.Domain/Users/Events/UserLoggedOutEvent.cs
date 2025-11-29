using FAM.Domain.Common;

namespace FAM.Domain.Users.Events;

/// <summary>
/// Domain event raised when user logs out
/// </summary>
public sealed record UserLoggedOutEvent : DomainEvent
{
    public long UserId { get; init; }
    public string DeviceId { get; init; }
    public bool AllDevices { get; init; }
    public DateTime LogoutAt { get; init; }

    public UserLoggedOutEvent(long userId, string deviceId, bool allDevices = false)
    {
        UserId = userId;
        DeviceId = deviceId;
        AllDevices = allDevices;
        LogoutAt = DateTime.UtcNow;
    }
}