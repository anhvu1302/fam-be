using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users;

namespace FAM.Domain.EmailTemplates;

/// <summary>
/// Email template for transactional emails
/// Uses FullAuditedAggregateRoot for complete audit trail
/// </summary>
public class EmailTemplate : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasModifier,
    IHasDeletionTime, IHasDeleter, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

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

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Domain events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Navigation properties
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
    public User? DeletedBy { get; set; }

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
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_CODE_REQUIRED);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_NAME_REQUIRED);
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SUBJECT_REQUIRED);
        }

        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_BODY_REQUIRED);
        }

        EmailTemplate template = new()
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
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_UPDATE);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_NAME_REQUIRED);
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SUBJECT_REQUIRED);
        }

        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_BODY_REQUIRED);
        }

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

    // Domain event methods
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // Audit methods
    public void SoftDelete(long? deletedById = null)
    {
        if (IsSystem)
        {
            throw new DomainException(ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_DELETE);
        }

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = deletedById;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = deletedById;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdatedAt = DateTime.UtcNow;
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
