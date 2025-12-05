using FAM.Domain.Common;

namespace FAM.Domain.EmailTemplates;

/// <summary>
/// Email template for transactional emails
/// </summary>
public class EmailTemplate : AggregateRoot
{
    /// <summary>
    /// Template code (unique identifier for template)
    /// Examples: OTP_EMAIL, PASSWORD_RESET, PASSWORD_CHANGED, WELCOME_EMAIL, ACCOUNT_LOCKED
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Template name
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Email subject with placeholders
    /// Example: "Your OTP Code - {{AppName}}"
    /// </summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>
    /// HTML body with placeholders
    /// Placeholders: {{userName}}, {{otpCode}}, {{resetLink}}, etc.
    /// </summary>
    public string HtmlBody { get; private set; } = string.Empty;

    /// <summary>
    /// Plain text body (optional fallback)
    /// </summary>
    public string? PlainTextBody { get; private set; }

    /// <summary>
    /// Description of template usage
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Available placeholders for this template (JSON array)
    /// Example: ["userName", "otpCode", "expiryMinutes"]
    /// </summary>
    public string? AvailablePlaceholders { get; private set; }

    /// <summary>
    /// Is this template active?
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Is this a system template (cannot be deleted)
    /// </summary>
    public bool IsSystem { get; private set; }

    /// <summary>
    /// Category of template
    /// </summary>
    public EmailTemplateCategory Category { get; private set; }

    private EmailTemplate()
    {
    }

    public static EmailTemplate Create(
        string code,
        string name,
        string subject,
        string htmlBody,
        EmailTemplateCategory category,
        string? description = null,
        string? plainTextBody = null,
        string? availablePlaceholders = null,
        bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_CODE_REQUIRED);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_NAME_REQUIRED);

        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SUBJECT_REQUIRED);

        if (string.IsNullOrWhiteSpace(htmlBody))
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_BODY_REQUIRED);

        var template = new EmailTemplate
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainTextBody,
            Description = description,
            AvailablePlaceholders = availablePlaceholders,
            Category = category,
            IsSystem = isSystem,
            IsActive = true
        };

        return template;
    }

    public void Update(
        string name,
        string subject,
        string htmlBody,
        EmailTemplateCategory category,
        string? description = null,
        string? plainTextBody = null,
        string? availablePlaceholders = null)
    {
        if (IsSystem)
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_UPDATE);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_NAME_REQUIRED);

        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SUBJECT_REQUIRED);

        if (string.IsNullOrWhiteSpace(htmlBody))
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_BODY_REQUIRED);

        Name = name;
        Subject = subject;
        HtmlBody = htmlBody;
        PlainTextBody = plainTextBody;
        Description = description;
        AvailablePlaceholders = availablePlaceholders;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public override void SoftDelete(long? deletedById = null)
    {
        if (IsSystem)
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_DELETE);

        base.SoftDelete(deletedById);
    }
}

/// <summary>
/// Email template category
/// </summary>
public enum EmailTemplateCategory
{
    Authentication = 1,
    Notification = 2,
    Marketing = 3,
    System = 4
}
