using FAM.Application.Users.Shared;
using FAM.Domain.EmailTemplates;

namespace FAM.Application.EmailTemplates.Shared;

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

    public DateTime? DeletedAt { get; init; }

    public UserDto? CreatedBy { get; init; }
    public UserDto? UpdatedBy { get; init; }
    public UserDto? DeletedBy { get; init; }
}

public static class EmailTemplateExtensions
{
    public static EmailTemplateDto ToDto(this EmailTemplate template)
    {
        return new EmailTemplateDto
        {
            Id = template.Id,
            Code = template.Code,
            Name = template.Name,
            Subject = template.Subject,
            HtmlBody = template.HtmlBody,
            PlainTextBody = template.PlainTextBody,
            Description = template.Description,
            AvailablePlaceholders = template.AvailablePlaceholders,
            IsActive = template.IsActive,
            IsSystem = template.IsSystem,
            Category = (int)template.Category,
            CategoryName = template.Category.ToString(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            DeletedAt = template.DeletedAt,
            CreatedBy = template.CreatedBy?.ToUserDto(),
            UpdatedBy = template.UpdatedBy?.ToUserDto(),
            DeletedBy = template.DeletedBy?.ToUserDto()
        };
    }
}