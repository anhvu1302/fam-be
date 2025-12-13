using FAM.Infrastructure.PersistenceModels.Mongo.Base;

using MongoDB.Bson.Serialization.Attributes;

namespace FAM.Infrastructure.PersistenceModels.Mongo;

/// <summary>
/// MongoDB document model for User
/// </summary>
[BsonCollection("users")]
public class UserMongo : BasicAuditedEntityMongo
{
    [BsonElement("username")] public string Username { get; set; } = string.Empty;

    [BsonElement("email")] public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")] public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("passwordSalt")] public string PasswordSalt { get; set; } = string.Empty;

    [BsonElement("fullName")] public string? FullName { get; set; }

    // Personal Information
    [BsonElement("firstName")] public string? FirstName { get; set; }
    [BsonElement("lastName")] public string? LastName { get; set; }
    [BsonElement("avatar")] public string? Avatar { get; set; }
    [BsonElement("phoneNumber")] public string? PhoneNumber { get; set; }
    [BsonElement("phoneCountryCode")] public string? PhoneCountryCode { get; set; }
    [BsonElement("dateOfBirth")] public DateTime? DateOfBirth { get; set; }
    [BsonElement("bio")] public string? Bio { get; set; }

    // Two-Factor Authentication
    [BsonElement("twoFactorEnabled")] public bool TwoFactorEnabled { get; set; }
    [BsonElement("twoFactorSecret")] public string? TwoFactorSecret { get; set; }
    [BsonElement("twoFactorBackupCodes")] public string? TwoFactorBackupCodes { get; set; }
    [BsonElement("twoFactorSetupDate")] public DateTime? TwoFactorSetupDate { get; set; }
    
    // Pending 2FA Secret (for setup phase - before confirmation)
    [BsonElement("pendingTwoFactorSecret")] public string? PendingTwoFactorSecret { get; set; }
    [BsonElement("pendingTwoFactorSecretExpiresAt")] public DateTime? PendingTwoFactorSecretExpiresAt { get; set; }

    // Password Reset
    [BsonElement("passwordResetToken")] public string? PasswordResetToken { get; set; }
    [BsonElement("passwordResetTokenExpiresAt")] public DateTime? PasswordResetTokenExpiresAt { get; set; }
    [BsonElement("passwordChangedAt")] public DateTime? PasswordChangedAt { get; set; }

    // Account Status
    [BsonElement("isActive")] public bool IsActive { get; set; } = true;
    [BsonElement("isEmailVerified")] public bool IsEmailVerified { get; set; }
    [BsonElement("isPhoneVerified")] public bool IsPhoneVerified { get; set; }
    [BsonElement("emailVerifiedAt")] public DateTime? EmailVerifiedAt { get; set; }
    [BsonElement("phoneVerifiedAt")] public DateTime? PhoneVerifiedAt { get; set; }
    [BsonElement("lastLoginAt")] public DateTime? LastLoginAt { get; set; }
    [BsonElement("lastLoginIp")] public string? LastLoginIp { get; set; }
    [BsonElement("failedLoginAttempts")] public int FailedLoginAttempts { get; set; }
    [BsonElement("lockoutEnd")] public DateTime? LockoutEnd { get; set; }

    // Preferences
    [BsonElement("preferredLanguage")] public string? PreferredLanguage { get; set; } = "en";
    [BsonElement("timeZone")] public string? TimeZone { get; set; } = "UTC";
    [BsonElement("receiveNotifications")] public bool ReceiveNotifications { get; set; } = true;
    [BsonElement("receiveMarketingEmails")] public bool ReceiveMarketingEmails { get; set; }

    public UserMongo() : base()
    {
    }

    public UserMongo(long domainId) : base(domainId)
    {
    }
}
