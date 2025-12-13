using System.Linq.Expressions;

using FAM.Application.Common.Helpers;
using FAM.Application.Querying;
using FAM.Application.Querying.Ast;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Parsing;
using FAM.Application.Users.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;

using MediatR;

namespace FAM.Application.Users.Queries.GetUsers;

/// <summary>
/// Handler for GetUsersQuery - Uses Repository with GetPagedAsync
/// </summary>
public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PageResult<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IFilterParser _filterParser;

    public GetUsersQueryHandler(IUserRepository userRepository, IFilterParser filterParser)
    {
        _userRepository = userRepository;
        _filterParser = filterParser;
    }

    public async Task<PageResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        QueryRequest queryRequest = request.QueryRequest;
        UserFieldMap fieldMap = UserFieldMap.Instance;

        // Parse filter expression
        Expression<Func<User, bool>>? filterExpression = null;
        if (!string.IsNullOrWhiteSpace(queryRequest.Filter))
            try
            {
                FilterNode ast = _filterParser.Parse(queryRequest.Filter);
                filterExpression = EfFilterBinder<User>.Bind(ast, fieldMap.Fields);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid filter syntax: {ex.Message}", ex);
            }

        // Get page settings
        var page = queryRequest.Page > 0 ? queryRequest.Page : 1;
        var pageSize = queryRequest.PageSize > 0 ? Math.Min(queryRequest.PageSize, 100) : 10;

        // Parse includes
        Expression<Func<User, object>>[] includes = fieldMap.ParseIncludes(queryRequest.Include);
        HashSet<string> includeSet = IncludeParser.Parse(queryRequest.Include);

        // Call repository
        (IEnumerable<User> items, var total) = await _userRepository.GetPagedAsync(
            filterExpression,
            queryRequest.Sort,
            page,
            pageSize,
            includes,
            cancellationToken);

        // Map to DTOs using extension method
        var dtos = items
            .Select(u => u.ToUserDto(includeSet))
            .Where(dto => dto != null)
            .Cast<UserDto>()
            .ToList();

        return new PageResult<UserDto>(dtos, page, pageSize, total);
    }
}
