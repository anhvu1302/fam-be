using Swashbuckle.AspNetCore.Annotations;

namespace FAM.WebApi.Contracts.EmailTemplates;

/// <summary>
/// Request to create an email template
/// </summary>
[SwaggerSchema(Required = new[] { "code", "name", "subject", "htmlBody", "category" })]
public sealed record CreateEmailTemplateRequest(
    string Code,
    string Name,
    string Subject,
    string HtmlBody,
    int Category,
    string? PlainTextBody = null,
    string? Description = null,
    string? AvailablePlaceholders = null
);

/// <summary>
/// Request to update an email template
/// </summary>
[SwaggerSchema(Required = new[] { "name", "subject", "htmlBody", "category" })]
public sealed record UpdateEmailTemplateRequest(
    string Name,
    string Subject,
    string HtmlBody,
    int Category,
    string? PlainTextBody = null,
    string? Description = null,
    string? AvailablePlaceholders = null
);
