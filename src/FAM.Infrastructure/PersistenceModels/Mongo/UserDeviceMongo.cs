using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for UserDevice
/// </summary>
[BsonCollection("userDevices")]
public class UserDeviceMongo : BaseEntityMongo
{
    [BsonElement("userId")]
    public long UserId { get; set; }

    [BsonElement("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [BsonElement("deviceName")]
    public string DeviceName { get; set; } = string.Empty;

    [BsonElement("deviceType")]
    public string DeviceType { get; set; } = string.Empty;

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("browser")]
    public string? Browser { get; set; }

    [BsonElement("operatingSystem")]
    public string? OperatingSystem { get; set; }

    [BsonElement("lastLoginAt")]
    public DateTime LastLoginAt { get; set; }

    [BsonElement("lastActivityAt")]
    public DateTime? LastActivityAt { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("isTrusted")]
    public bool IsTrusted { get; set; }

    [BsonElement("refreshToken")]
    public string? RefreshToken { get; set; }

    [BsonElement("refreshTokenExpiresAt")]
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public UserDeviceMongo() : base() { }

    public UserDeviceMongo(long domainId) : base(domainId) { }
}