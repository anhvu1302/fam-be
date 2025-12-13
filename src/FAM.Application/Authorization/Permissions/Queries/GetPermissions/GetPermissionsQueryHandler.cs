using System.Linq.Expressions;

using FAM.Application.Authorization.Permissions.Shared;
using FAM.Application.Common.Exceptions;
using FAM.Application.Common.Helpers;
using FAM.Application.Querying;
using FAM.Application.Querying.Ast;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Parsing;
using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;

using MediatR;

namespace FAM.Application.Authorization.Permissions.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, PageResult<PermissionDto>>
{
    private readonly IFilterParser _filterParser;
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionsQueryHandler(
        IPermissionRepository permissionRepository,
        IFilterParser filterParser)
    {
        _permissionRepository = permissionRepository;
        _filterParser = filterParser;
    }

    public async Task<PageResult<PermissionDto>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        PermissionFieldMap fieldMap = PermissionFieldMap.Instance;
        QueryRequest queryRequest = request.QueryRequest;

        Expression<Func<Permission, bool>>? filterExpression = null;
        if (!string.IsNullOrWhiteSpace(queryRequest.Filter))
            try
            {
                FilterNode ast = _filterParser.Parse(queryRequest.Filter);
                filterExpression = EfFilterBinder<Permission>.Bind(ast, fieldMap.Fields);
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

        Expression<Func<Permission, object>>[] includes = fieldMap.ParseIncludes(queryRequest.Include);
        HashSet<string> includeSet = IncludeParser.Parse(queryRequest.Include);

        (IEnumerable<Permission> permissions, var totalCount) = await _permissionRepository.GetPagedAsync(
            filterExpression,
            queryRequest.Sort,
            page,
            pageSize,
            includes,
            cancellationToken);

        var items = permissions.Select(p => p.ToPermissionDto(includeSet)).ToList();

        return new PageResult<PermissionDto>(
            items!,
            page,
            pageSize,
            totalCount
        );
    }
}
