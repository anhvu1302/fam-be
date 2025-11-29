using FAM.Domain.Common;

namespace FAM.Domain.Users.Events;

/// <summary>
/// Domain event raised when user login fails
/// </summary>
public sealed record UserLoginFailedEvent : DomainEvent
{
    public long? UserId { get; init; }
    public string Username { get; init; }
    public string Reason { get; init; }
    public string? IpAddress { get; init; }
    public int FailedAttempts { get; init; }
    public DateTime FailedAt { get; init; }

    public UserLoginFailedEvent(long? userId, string username, string reason, string? ipAddress, int failedAttempts)
    {
        UserId = userId;
        Username = username;
        Reason = reason;
        IpAddress = ipAddress;
        FailedAttempts = failedAttempts;
        FailedAt = DateTime.UtcNow;
    }
}