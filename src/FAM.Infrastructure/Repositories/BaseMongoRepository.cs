using AutoMapper;
using FAM.Infrastructure.Providers.MongoDB;
using MongoDB.Driver;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// Base repository for MongoDB with MongoDB Driver specific implementations
/// Inherits common helpers from BasePagedRepository
/// </summary>
public abstract class BaseMongoRepository<TDomain, TMongo> : BasePagedRepository<TDomain, TMongo>
    where TDomain : class
    where TMongo : class
{
    protected readonly MongoDbContext Context;

    protected BaseMongoRepository(MongoDbContext context, IMapper mapper) : base(mapper)
    {
        Context = context;
    }

    /// <summary>
    /// Apply dynamic sorting to MongoDB query
    /// </summary>
    protected SortDefinition<TMongo>? ApplySort(
        string? sort,
        Func<string, SortDefinition<TMongo>> getFieldSort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return null;

        var sortBuilder = Builders<TMongo>.Sort;
        SortDefinition<TMongo>? sortDefinition = null;

        var sortParts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var sortPart in sortParts)
        {
            var trimmed = sortPart.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var descending = trimmed.StartsWith('-');
            var fieldName = descending ? trimmed[1..] : trimmed;

            try
            {
                var fieldSort = getFieldSort(fieldName.ToLowerInvariant());
                sortDefinition = sortDefinition == null
                    ? fieldSort
                    : sortBuilder.Combine(sortDefinition, fieldSort);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Field '{fieldName}' cannot be used for sorting");
            }
        }

        return sortDefinition;
    }
}