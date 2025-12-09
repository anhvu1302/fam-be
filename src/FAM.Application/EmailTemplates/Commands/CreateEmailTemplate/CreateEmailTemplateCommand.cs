using MediatR;

namespace FAM.Application.EmailTemplates.Commands.CreateEmailTemplate;

/// <summary>
/// Command to create a new email template
/// </summary>
public sealed record CreateEmailTemplateCommand : IRequest<long>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public string? Description { get; init; }
    public string? AvailablePlaceholders { get; init; }
    public int Category { get; init; }
    public bool IsSystem { get; init; }
}