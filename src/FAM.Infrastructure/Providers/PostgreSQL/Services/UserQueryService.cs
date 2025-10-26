using AutoMapper;
using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using FAM.Application.Querying.Parsing;
using FAM.Infrastructure.PersistenceModels.Ef;
using FAM.Infrastructure.Providers.PostgreSQL.Querying;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Services;

/// <summary>
/// PostgreSQL implementation of IQueryService for Users
/// </summary>
public class UserQueryService : IQueryService<UserDto>
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFilterParser _parser;

    public UserQueryService(PostgreSqlDbContext context, IMapper mapper, IFilterParser parser)
    {
        _context = context;
        _mapper = mapper;
        _parser = parser;
    }

    public async Task<PageResult<UserDto>> QueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        var fieldMap = UserEfFieldMap.Instance;

        // Start with EF queryable
        var query = _context.Users.AsNoTracking();

        // Apply filter if provided
        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            query = query.ApplyFilter(request.Filter, fieldMap, _parser);
        }

        // Get total count AFTER filter
        var total = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.Sort))
        {
            query = query.ApplySort(request.Sort, fieldMap);
        }
        else
        {
            // Default sort by CreatedAt descending
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        // Apply paging
        const int maxPageSize = 100;
        var pageSize = Math.Min(request.PageSize, maxPageSize);
        query = query.ApplyPaging(request.Page, pageSize, maxPageSize);

        // Execute query on EF entities
        var efUsers = await query.ToListAsync(cancellationToken);

        // Map EF entities to Domain entities, then to DTOs
        // AutoMapper knows: UserEf -> User (domain) and User -> UserDto
        var domainUsers = _mapper.Map<List<FAM.Domain.Users.User>>(efUsers);
        var userDtos = _mapper.Map<List<UserDto>>(domainUsers);

        return new PageResult<UserDto>(userDtos, request.Page, pageSize, total);
    }
}
