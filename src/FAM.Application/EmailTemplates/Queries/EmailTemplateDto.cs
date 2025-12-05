namespace FAM.Application.EmailTemplates.Queries;

/// <summary>
/// Email template DTO
/// </summary>
public sealed record EmailTemplateDto
{
    public long Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public string? Description { get; init; }
    public string? AvailablePlaceholders { get; init; }
    public bool IsActive { get; init; }
    public bool IsSystem { get; init; }
    public int Category { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
