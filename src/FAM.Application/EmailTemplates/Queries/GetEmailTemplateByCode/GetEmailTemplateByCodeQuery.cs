using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetEmailTemplateByCode;

/// <summary>
/// Query to get an email template by code
/// </summary>
public sealed record GetEmailTemplateByCodeQuery(string Code) : IRequest<EmailTemplateDto?>;
