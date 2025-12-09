using System.ComponentModel.DataAnnotations.Schema;
using FAM.Infrastructure.PersistenceModels.Ef.Base;

namespace FAM.Infrastructure.PersistenceModels.Ef;

/// <summary>
/// EF-specific persistence model for User
/// Separate from domain entity to avoid persistence concerns leaking into domain
/// </summary>
[Table("users")]
public class UserEf : BaseEntityEf
{
    // Authentication
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    // Personal Information
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PhoneCountryCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }

    // Two-Factor Authentication
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public string? TwoFactorBackupCodes { get; set; }
    public DateTime? TwoFactorSetupDate { get; set; }

    // Account Status
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? PhoneVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; } = "en";
    public string? TimeZone { get; set; } = "UTC";
    public bool ReceiveNotifications { get; set; } = true;

    public bool ReceiveMarketingEmails { get; set; }

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


    // Navigation properties for EF relationships
    public ICollection<UserNodeRoleEf> UserNodeRoles { get; set; } = new List<UserNodeRoleEf>();
    public ICollection<UserDeviceEf> UserDevices { get; set; } = new List<UserDeviceEf>();
}