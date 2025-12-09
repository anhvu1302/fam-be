using FAM.Infrastructure.PersistenceModels.Mongo.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for User
/// </summary>
[BsonCollection("users")]
public class UserMongo : BasicAuditedEntityMongo
{
    [BsonElement("username")] public string Username { get; set; } = string.Empty;

    [BsonElement("email")] public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")] public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("passwordSalt")] public string PasswordSalt { get; set; } = string.Empty;

    [BsonElement("fullName")] public string? FullName { get; set; }

    public UserMongo() : base()
    {
    }

    public UserMongo(long domainId) : base(domainId)
    {
    }
}