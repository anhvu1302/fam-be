using AutoMapper;
using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Extensions;
using FAM.Application.Querying.Parsing;
using FAM.Application.Querying.Validation;
using FAM.Application.Users;
using FAM.Infrastructure.PersistenceModels.Mongo;
using FAM.Infrastructure.Providers.MongoDB.Querying;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Services;

public class MongoUserQueryService : IQueryService<UserDto>
{
    private readonly MongoDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFilterParser _parser;

    public MongoUserQueryService(MongoDbContext context, IMapper mapper, IFilterParser parser)
    {
        _context = context;
        _mapper = mapper;
        _parser = parser;
    }

    public async Task<PageResult<UserDto>> QueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        var fieldMap = MongoUserFieldMap.Instance;
        var collection = _context.GetCollection<UserMongo>("users");

        var filterDefinition = BuildMongoFilter(request.Filter, fieldMap);

        var find = collection.Find(filterDefinition);

        var total = await find.CountDocumentsAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Sort))
        {
            var sortDefinition = BuildMongoSort(request.Sort, fieldMap);
            find = find.Sort(sortDefinition);
        }
        else
        {
            find = find.SortByDescending(u => u.CreatedAt);
        }

        const int maxPageSize = 100;
        var pageSize = Math.Min(request.PageSize, maxPageSize);
        var skip = (request.Page - 1) * pageSize;
        find = find.Skip(skip).Limit(pageSize);

        var mongoDocs = await find.ToListAsync(cancellationToken);

        var userDtos = _mapper.Map<List<UserDto>>(mongoDocs);

        return new PageResult<UserDto>(userDtos, request.Page, pageSize, total);
    }

    /// <summary>
    /// Build MongoDB filter from DSL filter string
    /// </summary>
    private FilterDefinition<UserMongo> BuildMongoFilter(string? filter, FieldMap<UserMongo> fieldMap)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return Builders<UserMongo>.Filter.Empty;

        var ast = _parser.Parse(filter);
        return MongoFilterBinder<UserMongo>.Bind(ast, fieldMap);
    }

    /// <summary>
    /// Build MongoDB sort definition from sort string
    /// </summary>
    private SortDefinition<UserMongo> BuildMongoSort(string? sort, FieldMap<UserMongo> fieldMap)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return Builders<UserMongo>.Sort.Descending(u => u.CreatedAt);

        var builder = Builders<UserMongo>.Sort;
        var sortParts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var sortDefinitions = new List<SortDefinition<UserMongo>>();

        foreach (var sortPart in sortParts)
        {
            var trimmed = sortPart.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var descending = trimmed.StartsWith('-');
            var fieldName = descending ? trimmed[1..] : trimmed;

            if (!fieldMap.ContainsField(fieldName))
                continue;

            sortDefinitions.Add(descending
                ? builder.Descending(fieldName)
                : builder.Ascending(fieldName));
        }

        return sortDefinitions.Count > 0
            ? builder.Combine(sortDefinitions)
            : builder.Descending(u => u.CreatedAt);
    }
}
