using System.ComponentModel.DataAnnotations.Schema;

using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF persistence model for EmailTemplate
/// Stores email templates for transactional emails
/// </summary>
[Table("email_templates")]
public class EmailTemplateEf : BaseEntityEf
{
    /// <summary>
    /// Template code (unique identifier)
    /// Examples: OTP_EMAIL, PASSWORD_RESET, PASSWORD_CHANGED
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Template name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email subject with placeholders
    /// Example: "Your OTP Code - {{AppName}}"
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body with placeholders
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Plain text body (optional fallback)
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// Description of template usage
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Available placeholders for this template (JSON array)
    /// Example: ["userName", "otpCode", "expiryMinutes"]
    /// </summary>
    public string? AvailablePlaceholders { get; set; }

    /// <summary>
    /// Is this template active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is this a system template (cannot be deleted)
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Category of template
    /// 1=Authentication, 2=Notification, 3=Marketing, 4=System
    /// </summary>
    public int Category { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedById { get; set; }

    // Navigation properties
    public virtual UserEf? CreatedBy { get; set; }
    public virtual UserEf? UpdatedBy { get; set; }
    public virtual UserEf? DeletedBy { get; set; }
}
