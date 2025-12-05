using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetEmailTemplateById;

/// <summary>
/// Query to get an email template by ID
/// </summary>
public sealed record GetEmailTemplateByIdQuery(long Id) : IRequest<EmailTemplateDto?>;
