using System.Linq.Expressions;

using FAM.Application.Common.Exceptions;
using FAM.Application.Common.Helpers;
using FAM.Application.EmailTemplates.Shared;
using FAM.Application.Querying;
using FAM.Application.Querying.Ast;
using FAM.Application.Querying.Binding;
using FAM.Application.Querying.Parsing;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;

using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetEmailTemplates;

public sealed class
    GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, PageResult<EmailTemplateDto>>
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
        EmailTemplateFieldMap fieldMap = EmailTemplateFieldMap.Instance;
        QueryRequest queryRequest = request.QueryRequest;

        Expression<Func<EmailTemplate, bool>>? filterExpression = null;
        if (!string.IsNullOrWhiteSpace(queryRequest.Filter))
        {
            try
            {
                FilterNode ast = _filterParser.Parse(queryRequest.Filter);
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
        }

        int page = queryRequest.GetEffectivePage();
        int pageSize = queryRequest.GetEffectivePageSize();

        Expression<Func<EmailTemplate, object>>[] includes = fieldMap.ParseIncludes(queryRequest.Include);
        HashSet<string> includeSet = IncludeParser.Parse(queryRequest.Include);

        (IEnumerable<EmailTemplate> templates, long totalCount) = await _emailTemplateRepository.GetPagedAsync(
            filterExpression,
            queryRequest.Sort,
            page,
            pageSize,
            includes,
            cancellationToken);

        List<EmailTemplateDto> items = templates.Select(t => t.ToDto()).ToList();

        return new PageResult<EmailTemplateDto>(
            items,
            page,
            pageSize,
            totalCount
        );
    }
}
