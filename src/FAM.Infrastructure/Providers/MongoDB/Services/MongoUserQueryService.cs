using AutoMapper;
using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Extensions;
using FAM.Application.Querying.Parsing;
using FAM.Application.Querying.Validation;
using FAM.Infrastructure.PersistenceModels.Mongo;
using FAM.Infrastructure.Providers.MongoDB.Querying;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB.Services;

/// <summary>
/// MongoDB implementation of IQueryService for Users
/// Uses Filter DSL to generate MongoDB queries
/// </summary>
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

        // Build filter from DSL
        var filterDefinition = BuildMongoFilter(request.Filter, fieldMap);

        // Start find with filter
        var find = collection.Find(filterDefinition);

        // Get total count AFTER filter
        var total = await find.CountDocumentsAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.Sort))
        {
            var sortDefinition = BuildMongoSort(request.Sort, fieldMap);
            find = find.Sort(sortDefinition);
        }
        else
        {
            // Default sort by CreatedAt descending
            find = find.SortByDescending(u => u.CreatedAt);
        }

        // Apply paging
        const int maxPageSize = 100;
        var pageSize = Math.Min(request.PageSize, maxPageSize);
        var skip = (request.Page - 1) * pageSize;
        find = find.Skip(skip).Limit(pageSize);

        // Execute query on MongoDB documents
        var mongoDocs = await find.ToListAsync(cancellationToken);

        // Map MongoDB documents → Domain entities → DTOs
        var domainUsers = _mapper.Map<List<FAM.Domain.Users.User>>(mongoDocs);
        var userDtos = _mapper.Map<List<UserDto>>(domainUsers);

        return new PageResult<UserDto>(userDtos, request.Page, pageSize, total);
    }

    private FilterDefinition<UserMongo> BuildMongoFilter(string? filter, FieldMap<UserMongo> fieldMap)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return Builders<UserMongo>.Filter.Empty;

        var ast = _parser.Parse(filter);
        return MongoFilterBinder<UserMongo>.Bind(ast, fieldMap);
    }

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
