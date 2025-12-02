using System.Linq.Expressions;
using FAM.Application.Authorization.Permissions.Shared;
using FAM.Application.Common.Exceptions;
using FAM.Application.Common.Helpers;
using FAM.Application.Querying;
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
        var fieldMap = PermissionFieldMap.Instance;
        var queryRequest = request.QueryRequest;

        Expression<Func<Permission, bool>>? filterExpression = null;
        if (!string.IsNullOrWhiteSpace(queryRequest.Filter))
            try
            {
                var ast = _filterParser.Parse(queryRequest.Filter);
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

        var includes = fieldMap.ParseIncludes(queryRequest.Include);
        var includeSet = IncludeParser.Parse(queryRequest.Include);

        var (permissions, totalCount) = await _permissionRepository.GetPagedAsync(
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