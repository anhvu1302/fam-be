using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Interfaces;
using FAM.Domain.Users.Entities;
using FAM.Domain.ValueObjects;

namespace FAM.Domain.Users;

/// <summary>
/// Người dùng - Uses BasicAuditedAggregateRoot
/// Tracks WHEN things happened (not WHO) since users manage themselves
/// </summary>
public class User : BaseEntity, IHasCreationTime, IHasCreator, IHasModificationTime, IHasDeletionTime, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    // Authentication - Pragmatic DDD: primitives for single-field, VO for complex
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public Password Password { get; private set; } = null!; // Keep as VO (Hash + Salt)

    // Personal Information
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? FullName { get; private set; }
    public string? Avatar { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? PhoneCountryCode { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Bio { get; private set; }

    // Two-Factor Authentication
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public string? TwoFactorBackupCodes { get; private set; } // JSON array of backup codes
    public DateTime? TwoFactorSetupDate { get; private set; }

    // Pending 2FA Secret (for setup phase - before confirmation)
    public string? PendingTwoFactorSecret { get; private set; }
    public DateTime? PendingTwoFactorSecretExpiresAt { get; private set; } // TTL: 10 minutes

    // Password Reset
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }
    public DateTime? PasswordChangedAt { get; private set; }

    // Account Status
    public bool IsActive { get; private set; } = true;
    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }
    public DateTime? PhoneVerifiedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    // Preferences
    public string? PreferredLanguage { get; private set; } = "en";
    public string? TimeZone { get; private set; } = "UTC";
    public bool ReceiveNotifications { get; private set; } = true;
    public bool ReceiveMarketingEmails { get; private set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// User's role assignments within organization nodes (part of User aggregate boundary)
    /// </summary>
    public ICollection<UserNodeRole> UserNodeRoles { get; set; } = new List<UserNodeRole>();

    /// <summary>
    /// User's login devices (part of User aggregate boundary)
    /// </summary>
    public ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();

    private User()
    {
    }

    // ============ Factory Methods ============
    public static User Create(string username, string email, string passwordHash, string passwordSalt,
        string? firstName = null, string? lastName = null, string? phoneNumber = null, string? phoneCountryCode = null)
    {
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            ReceiveNotifications = true,
            PreferredLanguage = "en",
            TimeZone = "UTC"
        };

        user.SetUsername(username);
        user.SetEmail(email);
        user.SetPassword(passwordHash, passwordSalt);
        user.SetPhoneNumber(phoneNumber, phoneCountryCode);
        user.UpdateFullName();

        return user;
    }

    /// <summary>
    /// Tạo User từ plain password (sẽ được hash tự động)
    /// </summary>
    public static User CreateWithPlainPassword(string username, string email, string plainPassword,
        string? firstName = null, string? lastName = null, string? phoneNumber = null, string? phoneCountryCode = null)
    {
        var password = Password.Create(plainPassword);
        return Create(username, email, password.Hash, password.Salt, firstName, lastName, phoneNumber,
            phoneCountryCode);
    }

    /// <summary>
    /// Factory method for loading User from database storage
    /// Hydrates domain entity from persistence model
    /// </summary>
    public static User CreateFromStorage(
        long id,
        string username,
        string email,
        string passwordHash,
        string passwordSalt,
        string? fullName,
        string? firstName,
        string? lastName,
        string? phoneNumber,
        bool twoFactorEnabled,
        string? twoFactorSecret,
        string? twoFactorBackupCodes,
        bool isActive,
        bool isEmailVerified,
        int failedLoginAttempts,
        DateTime? lockoutEnd,
        string? preferredLanguage,
        string? timeZone,
        bool receiveNotifications,
        bool receiveMarketingEmails,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var user = new User
        {
            Id = id,
            FullName = fullName,
            FirstName = firstName,
            LastName = lastName,
            TwoFactorEnabled = twoFactorEnabled,
            TwoFactorSecret = twoFactorSecret,
            TwoFactorBackupCodes = twoFactorBackupCodes,
            IsActive = isActive,
            IsEmailVerified = isEmailVerified,
            FailedLoginAttempts = failedLoginAttempts,
            LockoutEnd = lockoutEnd,
            PreferredLanguage = preferredLanguage ?? "en",
            TimeZone = timeZone ?? "UTC",
            ReceiveNotifications = receiveNotifications,
            ReceiveMarketingEmails = receiveMarketingEmails,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        user.SetUsername(username);
        user.SetEmail(email);
        user.SetPassword(passwordHash, passwordSalt);
        if (!string.IsNullOrWhiteSpace(phoneNumber)) user.SetPhoneNumber(phoneNumber, "VN");

        return user;
    }

    // ============ Getters & Setters ============
    public void SetUsername(string username)
    {
        var usernameVo = Domain.ValueObjects.Username.Create(username);
        Username = usernameVo.Value;
    }

    public void SetEmail(string email)
    {
        var emailVo = Domain.ValueObjects.Email.Create(email);
        Email = emailVo.Value;
        IsEmailVerified = false;
        EmailVerifiedAt = null;
    }

    public void SetPassword(string passwordHash, string passwordSalt)
    {
        Password = Password.FromHash(passwordHash, passwordSalt);
    }

    public void SetPhoneNumber(string? phoneNumber, string? countryCode = "VN")
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var phoneVo = Domain.ValueObjects.PhoneNumber.Create(phoneNumber, countryCode ?? "VN");
            PhoneNumber = phoneVo.Value;
            PhoneCountryCode = phoneVo.CountryCode;
            IsPhoneVerified = false;
            PhoneVerifiedAt = null;
        }
        else
        {
            PhoneNumber = null;
            PhoneCountryCode = null;
        }
    }

    public void SetPasswordResetToken(string token, int expirationMinutes = 60)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    public void SetPendingTwoFactorSecret(string secret, int expirationMinutes = 10)
    {
        PendingTwoFactorSecret = secret;
        PendingTwoFactorSecretExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    public bool IsPasswordResetTokenValid(string token)
    {
        if (string.IsNullOrWhiteSpace(PasswordResetToken))
            return false;

        if (PasswordResetToken != token)
            return false;

        if (!PasswordResetTokenExpiresAt.HasValue)
            return false;

        return PasswordResetTokenExpiresAt.Value > DateTime.UtcNow;
    }

    public bool IsPendingTwoFactorSecretValid(string secret)
    {
        return PendingTwoFactorSecret == secret &&
               PendingTwoFactorSecretExpiresAt > DateTime.UtcNow;
    }

    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    // ============ Business Logic Methods ============
    public void UpdatePersonalInfo(string? firstName, string? lastName, string? avatar, string? bio,
        DateTime? dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        Avatar = avatar;
        Bio = bio;
        DateOfBirth = dateOfBirth;
        UpdateFullName();
    }

    public void UpdateContactInfo(string? phoneNumber, string? countryCode = "VN")
    {
        SetPhoneNumber(phoneNumber, countryCode);
    }

    public void ChangePassword(string currentPassRaw, string newPassRaw)
    {
        if (!Password.Verify(currentPassRaw))
            throw new DomainException(ErrorCodes.AUTH_INVALID_OLD_PASSWORD, "Current password is incorrect");

        var newPassVo = Password.Create(newPassRaw);
        UpdatePassword(newPassVo.Hash, newPassVo.Salt);
    }

    public void UpdatePassword(string newPasswordHash, string newPasswordSalt)
    {
        Password = Password.FromHash(newPasswordHash, newPasswordSalt);
        PasswordChangedAt = DateTime.UtcNow;

        // Clear any pending reset token
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }

    public void EnableTwoFactor(string secret, string? backupCodes = null)
    {
        TwoFactorEnabled = true;
        TwoFactorSecret = secret;
        TwoFactorBackupCodes = backupCodes;
        TwoFactorSetupDate = DateTime.UtcNow;
        // Clear pending secret after confirmation
        PendingTwoFactorSecret = null;
        PendingTwoFactorSecretExpiresAt = null;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        TwoFactorBackupCodes = null;
        TwoFactorSetupDate = null;
    }

    public void UpdateRecoveryCodes(string backupCodes)
    {
        TwoFactorBackupCodes = backupCodes;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
    }

    public void VerifyPhone()
    {
        IsPhoneVerified = true;
        PhoneVerifiedAt = DateTime.UtcNow;
    }

    public void RecordLogin(string? ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5) // Lock account after 5 failed attempts
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
    }

    /// <summary>
    /// Get existing device or create new one for authentication
    /// </summary>
    public UserDevice GetOrCreateDevice(
        string deviceId,
        string deviceName,
        string deviceType,
        string? userAgent = null,
        string? ipAddress = null,
        string? location = null)
    {
        UserDevice? existingDevice = UserDevices.FirstOrDefault(d => d.DeviceId == deviceId);

        if (existingDevice != null)
        {
            existingDevice.UpdateActivity(ipAddress, location);
            return existingDevice;
        }

        var newDevice = UserDevice.Create(
            Id,
            deviceId,
            deviceName,
            deviceType,
            userAgent,
            ipAddress,
            location
        );

        UserDevices.Add(newDevice);
        return newDevice;
    }

    /// <summary>
    /// Reset failed login attempts (used after successful login)
    /// </summary>
    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    /// <summary>
    /// Lock account for security reasons
    /// </summary>
    public void LockAccount(int minutes = 15)
    {
        LockoutEnd = DateTime.UtcNow.AddMinutes(minutes);
    }

    /// <summary>
    /// Unlock account manually (admin action)
    /// </summary>
    public void UnlockAccount()
    {
        LockoutEnd = null;
        FailedLoginAttempts = 0;
    }

    public void UpdatePreferences(string? preferredLanguage, string? timeZone, bool? receiveNotifications,
        bool? receiveMarketingEmails)
    {
        if (!string.IsNullOrWhiteSpace(preferredLanguage))
            PreferredLanguage = preferredLanguage;
        if (!string.IsNullOrWhiteSpace(timeZone))
            TimeZone = timeZone;
        if (receiveNotifications.HasValue)
            ReceiveNotifications = receiveNotifications.Value;
        if (receiveMarketingEmails.HasValue)
            ReceiveMarketingEmails = receiveMarketingEmails.Value;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    // ============ Private Helpers ============
    private void UpdateFullName()
    {
        FullName = $"{FirstName} {LastName}".Trim();
        if (string.IsNullOrWhiteSpace(FullName)) FullName = Username;
    }

    // ============ Domain Events ============
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public virtual void SoftDelete(long? deletedById = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
