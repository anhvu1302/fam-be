using System.Linq.Expressions;
using FAM.Application.Authorization.Roles.Shared;
using FAM.Application.Common.Exceptions;
using FAM.Application.Common.Helpers;
using FAM.Application.Querying;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Parsing;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using MediatR;

namespace FAM.Application.Authorization.Roles.Queries.GetRoles;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PageResult<RoleDto>>
{
    private readonly IFilterParser _filterParser;
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(
        IRoleRepository roleRepository,
        IFilterParser filterParser)
    {
        _roleRepository = roleRepository;
        _filterParser = filterParser;
    }

    public async Task<PageResult<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var fieldMap = RoleFieldMap.Instance;
        var queryRequest = request.QueryRequest;

        Expression<Func<Role, bool>>? filterExpression = null;
        if (!string.IsNullOrWhiteSpace(queryRequest.Filter))
            try
            {
                var ast = _filterParser.Parse(queryRequest.Filter);
                filterExpression = EfFilterBinder<Role>.Bind(ast, fieldMap.Fields);
            }
            catch (InvalidOperationException ex)
            {
                throw FilterExceptionHelper.CreateFilterException(ex, queryRequest.Filter, fieldMap.Fields);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid filter syntax: {ex.Message}", ex);
            }

        var page = queryRequest.GetEffectivePage();
        var pageSize = queryRequest.GetEffectivePageSize();

        var includes = fieldMap.ParseIncludes(queryRequest.Include);
        var includeSet = IncludeParser.Parse(queryRequest.Include);

        var (roles, totalCount) = await _roleRepository.GetPagedAsync(
            filterExpression,
            queryRequest.Sort,
            page,
            pageSize,
            includes,
            cancellationToken);

        var items = roles.Select(r => r.ToRoleDto(includeSet)).ToList();

        return new PageResult<RoleDto>(
            items!,
            page,
            pageSize,
            totalCount
        );
    }
}