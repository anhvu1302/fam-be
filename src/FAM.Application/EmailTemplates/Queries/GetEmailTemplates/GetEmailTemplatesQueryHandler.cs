using System.Linq.Expressions;
using FAM.Application.Common.Exceptions;
using FAM.Application.Common.Helpers;
using FAM.Application.EmailTemplates.Shared;
using FAM.Application.Querying;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Parsing;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetEmailTemplates;

public sealed class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, PageResult<EmailTemplateDto>>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IFilterParser _filterParser;

    public GetEmailTemplatesQueryHandler(
        IEmailTemplateRepository emailTemplateRepository,
        IFilterParser filterParser)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _filterParser = filterParser;
    }

    public async Task<PageResult<EmailTemplateDto>> Handle(
        GetEmailTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var fieldMap = EmailTemplateFieldMap.Instance;
        var queryRequest = request.QueryRequest;

        Expression<Func<EmailTemplate, bool>>? filterExpression = null;
        if (!string.IsNullOrWhiteSpace(queryRequest.Filter))
            try
            {
                var ast = _filterParser.Parse(queryRequest.Filter);
                filterExpression = EfFilterBinder<EmailTemplate>.Bind(ast, fieldMap.Fields);
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

        var (templates, totalCount) = await _emailTemplateRepository.GetPagedAsync(
            filterExpression,
            queryRequest.Sort,
            page,
            pageSize,
            includes,
            cancellationToken);

        var items = templates.Select(t => t.ToDto()).ToList();

        return new PageResult<EmailTemplateDto>(
            items,
            page,
            pageSize,
            totalCount
        );
    }
}
