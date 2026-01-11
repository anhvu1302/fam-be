using System.Linq.Expressions;

using FAM.Application.Common.Exceptions;
using FAM.Application.Querying;
using FAM.Application.Querying.Ast;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Parsing;
using FAM.Application.Settings.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Settings.Queries.GetSystemSettings;

public sealed class
    GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, PageResult<SystemSettingDto>>
{
    private readonly IFilterParser _filterParser;
    private readonly ISystemSettingRepository _systemSettingRepository;

    public GetSystemSettingsQueryHandler(
        ISystemSettingRepository systemSettingRepository,
        IFilterParser filterParser)
    {
        _systemSettingRepository = systemSettingRepository;
        _filterParser = filterParser;
    }

    public async Task<PageResult<SystemSettingDto>> Handle(GetSystemSettingsQuery request,
        CancellationToken cancellationToken)
    {
        SystemSettingFieldMap fieldMap = SystemSettingFieldMap.Instance;
        QueryRequest queryRequest = request.QueryRequest;

        Expression<Func<SystemSetting, bool>>? filterExpression = null;
        if (!string.IsNullOrWhiteSpace(queryRequest.Filter))
        {
            try
            {
                FilterNode ast = _filterParser.Parse(queryRequest.Filter);
                filterExpression = EfFilterBinder<SystemSetting>.Bind(ast, fieldMap.Fields);
            }
            catch (InvalidOperationException ex)
            {
                throw FilterExceptionHelper.CreateFilterException(ex, queryRequest.Filter, fieldMap.Fields);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid filter syntax: {ex.Message}", ex);
            }
        }

        int page = queryRequest.GetEffectivePage();
        int pageSize = queryRequest.GetEffectivePageSize();

        Expression<Func<SystemSetting, object>>[] includes = fieldMap.ParseIncludes(queryRequest.Include);

        (IEnumerable<SystemSetting> settings, long totalCount) = await _systemSettingRepository.GetPagedAsync(
            filterExpression,
            queryRequest.Sort,
            page,
            pageSize,
            includes,
            cancellationToken);

        List<SystemSettingDto> items = settings.Select(s => s.ToDto()!).ToList();

        return new PageResult<SystemSettingDto>(
            items,
            page,
            pageSize,
            totalCount
        );
    }
}
