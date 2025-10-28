using AutoMapper;
using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using FAM.Application.Querying.Parsing;
using FAM.Application.Users;
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

        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            query = query.ApplyFilter(request.Filter, fieldMap, _parser);
        }

        var total = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Sort))
        {
            query = query.ApplySort(request.Sort, fieldMap);
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        const int maxPageSize = 100;
        var pageSize = Math.Min(request.PageSize, maxPageSize);
        query = query.ApplyPaging(request.Page, pageSize, maxPageSize);

        var efUsers = await query.ToListAsync(cancellationToken);

        var userDtos = _mapper.Map<List<UserDto>>(efUsers);

        return new PageResult<UserDto>(userDtos, request.Page, pageSize, total);
    }
}
