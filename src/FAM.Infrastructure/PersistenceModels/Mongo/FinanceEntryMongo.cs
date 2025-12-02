using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB persistence model for FinanceEntry
/// </summary>
[BsonIgnoreExtraElements]
public class FinanceEntryMongo : BaseEntityMongo
{
    [BsonElement("assetId")] public long AssetId { get; set; }

    [BsonElement("period")] public DateTime Period { get; set; }

    [BsonElement("entryType")] public string EntryType { get; set; } = string.Empty;

    [BsonElement("amount")] public decimal Amount { get; set; }

    [BsonElement("bookValueAfter")] public decimal? BookValueAfter { get; set; }

    [BsonElement("createdBy")] public long? CreatedBy { get; set; }

    public FinanceEntryMongo()
    {
    }

    public FinanceEntryMongo(long domainId) : base(domainId)
    {
    }
}