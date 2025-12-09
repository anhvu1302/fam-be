using FAM.Infrastructure.PersistenceModels.Mongo.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for UserTheme
/// </summary>
[BsonCollection("userThemes")]
public class UserThemeMongo : BaseEntityMongo
{
    [BsonElement("userId")] public long UserId { get; set; }

    [BsonElement("theme")] public string Theme { get; set; } = "System";

    [BsonElement("primaryColor")] public string? PrimaryColor { get; set; }

    [BsonElement("transparency")] public decimal Transparency { get; set; } = 0.5m;

    [BsonElement("borderRadius")] public int BorderRadius { get; set; } = 8;

    [BsonElement("darkTheme")] public bool DarkTheme { get; set; }

    [BsonElement("pinNavbar")] public bool PinNavbar { get; set; }

    [BsonElement("compactMode")] public bool CompactMode { get; set; }
}