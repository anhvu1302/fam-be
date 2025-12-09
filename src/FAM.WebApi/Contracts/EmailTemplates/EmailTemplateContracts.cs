using System.ComponentModel.DataAnnotations;

namespace FAM.WebApi.Contracts.EmailTemplates;

/// <summary>
/// Request to create an email template
/// </summary>
public sealed record CreateEmailTemplateRequest
{
    /// <summary>
    /// Template code (unique identifier)
    /// </summary>
    [Required(ErrorMessage = "Template code is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Code must be between 3 and 100 characters")]
    [RegularExpression(@"^[A-Z][A-Z0-9_]*$",
        ErrorMessage = "Code must be uppercase letters, numbers, and underscores only, starting with a letter")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Template name
    /// </summary>
    [Required(ErrorMessage = "Template name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Email subject with placeholders
    /// </summary>
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Subject must be between 3 and 500 characters")]
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// HTML body with placeholders
    /// </summary>
    [Required(ErrorMessage = "HTML body is required")]
    [MinLength(10, ErrorMessage = "HTML body must be at least 10 characters")]
    public string HtmlBody { get; init; } = string.Empty;

    /// <summary>
    /// Plain text body (optional fallback)
    /// </summary>
    [StringLength(50000, ErrorMessage = "Plain text body cannot exceed 50000 characters")]
    public string? PlainTextBody { get; init; }

    /// <summary>
    /// Description of template usage
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; init; }

    /// <summary>
    /// Available placeholders (JSON array format)
    /// </summary>
    [StringLength(2000, ErrorMessage = "Available placeholders cannot exceed 2000 characters")]
    public string? AvailablePlaceholders { get; init; }

    /// <summary>
    /// Category (1=Authentication, 2=Notification, 3=Marketing, 4=System)
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    [Range(1, 4, ErrorMessage = "Category must be between 1 and 4")]
    public int Category { get; init; }
}

/// <summary>
/// Request to update an email template
/// </summary>
public sealed record UpdateEmailTemplateRequest
{
    /// <summary>
    /// Template name
    /// </summary>
    [Required(ErrorMessage = "Template name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Email subject with placeholders
    /// </summary>
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Subject must be between 3 and 500 characters")]
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// HTML body with placeholders
    /// </summary>
    [Required(ErrorMessage = "HTML body is required")]
    [MinLength(10, ErrorMessage = "HTML body must be at least 10 characters")]
    public string HtmlBody { get; init; } = string.Empty;

    /// <summary>
    /// Plain text body (optional fallback)
    /// </summary>
    [StringLength(50000, ErrorMessage = "Plain text body cannot exceed 50000 characters")]
    public string? PlainTextBody { get; init; }

    /// <summary>
    /// Description of template usage
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; init; }

    /// <summary>
    /// Available placeholders (JSON array format)
    /// </summary>
    [StringLength(2000, ErrorMessage = "Available placeholders cannot exceed 2000 characters")]
    public string? AvailablePlaceholders { get; init; }

    /// <summary>
    /// Category (1=Authentication, 2=Notification, 3=Marketing, 4=System)
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    [Range(1, 4, ErrorMessage = "Category must be between 1 and 4")]
    public int Category { get; init; }
}