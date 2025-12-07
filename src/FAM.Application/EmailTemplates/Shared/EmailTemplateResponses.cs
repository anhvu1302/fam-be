namespace FAM.Application.EmailTemplates.Shared;

/// <summary>
/// Response for email template operations
/// </summary>
public sealed record EmailTemplateResponse(
    long Id,
    string Code,
    string Name,
    string Subject,
    string HtmlBody,
    string? PlainTextBody,
    string? Description,
    string? AvailablePlaceholders,
    bool IsActive,
    bool IsSystem,
    int Category,
    string CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
