using FAM.Application.EmailTemplates.Shared;
using FAM.Application.Querying;
using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetEmailTemplates;

/// <summary>
/// Query to get paginated list of email templates with filtering and sorting
/// </summary>
public sealed record GetEmailTemplatesQuery(QueryRequest QueryRequest)
    : IRequest<PageResult<EmailTemplateDto>>;
