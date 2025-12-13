using MediatR;

namespace FAM.Application.EmailTemplates.Commands.UpdateEmailTemplate;

/// <summary>
/// Command to update an existing email template
/// </summary>
public sealed record UpdateEmailTemplateCommand : IRequest<bool>
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public string? Description { get; init; }
    public string? AvailablePlaceholders { get; init; }
    public int Category { get; init; }
}
