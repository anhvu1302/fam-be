using FAM.Domain.Common;
using FAM.Domain.ValueObjects;
using FAM.Domain.Users.Entities;

namespace FAM.Domain.Users;

/// <summary>
/// Người dùng
/// </summary>
public class User : BaseEntity
{
    // Authentication
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Password Password { get; private set; } = null!;

    // Personal Information
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? FullName { get; private set; }
    public string? Avatar { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Bio { get; private set; }

    // Two-Factor Authentication
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public string? TwoFactorBackupCodes { get; private set; } // JSON array of backup codes
    public DateTime? TwoFactorSetupDate { get; private set; }

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

    // Navigation properties
    public ICollection<Assets.Asset> OwnedAssets { get; set; } = new List<Assets.Asset>();
    public ICollection<Assets.Assignment> Assignments { get; set; } = new List<Assets.Assignment>();
    public ICollection<Assets.AssetEvent> AssetEvents { get; set; } = new List<Assets.AssetEvent>();
    public ICollection<Finance.FinanceEntry> FinanceEntries { get; set; } = new List<Finance.FinanceEntry>();
    public ICollection<Assets.Attachment> Attachments { get; set; } = new List<Assets.Attachment>();
    public ICollection<Authorization.UserNodeRole> UserNodeRoles { get; set; } = new List<Authorization.UserNodeRole>();
    public ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();

    private User() { }

    public static User Create(string username, string email, string passwordHash, string passwordSalt,
        string? firstName = null, string? lastName = null, string? phoneNumber = null)
    {
        var user = new User
        {
            Username = Username.Create(username),
            Email = Email.Create(email),
            Password = Password.FromHash(passwordHash, passwordSalt),
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            ReceiveNotifications = true,
            PreferredLanguage = "en",
            TimeZone = "UTC"
        };

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            user.PhoneNumber = PhoneNumber.Create(phoneNumber, "VN"); // Default to Vietnam
        }

        user.UpdateFullName();
        return user;
    }

    /// <summary>
    /// Tạo User từ plain password (sẽ được hash tự động)
    /// </summary>
    public static User Create(string username, string email, string plainPassword,
        string? firstName = null, string? lastName = null, string? phoneNumber = null)
    {
        var password = Password.Create(plainPassword);
        return Create(username, email, password.Hash, password.Salt, firstName, lastName, phoneNumber);
    }

    public void UpdatePersonalInfo(string? firstName, string? lastName, string? avatar, string? bio, DateTime? dateOfBirth)
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
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            PhoneNumber = PhoneNumber.Create(phoneNumber, countryCode);
        }
        else
        {
            PhoneNumber = null;
        }
    }

    public void UpdatePassword(string newPasswordHash, string newPasswordSalt)
    {
        Password = Password.FromHash(newPasswordHash, newPasswordSalt);
    }

    public void EnableTwoFactor(string secret, string? backupCodes = null)
    {
        TwoFactorEnabled = true;
        TwoFactorSecret = secret;
        TwoFactorBackupCodes = backupCodes;
        TwoFactorSetupDate = DateTime.UtcNow;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        TwoFactorBackupCodes = null;
        TwoFactorSetupDate = null;
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
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        }
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
        var existingDevice = UserDevices.FirstOrDefault(d => d.DeviceId == deviceId);
        
        if (existingDevice != null)
        {
            existingDevice.UpdateActivity(ipAddress, location);
            return existingDevice;
        }

        var newDevice = UserDevice.Create(
            this.Id,
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

    public void UpdatePreferences(string? preferredLanguage, string? timeZone, bool? receiveNotifications, bool? receiveMarketingEmails)
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

    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    private void UpdateFullName()
    {
        FullName = $"{FirstName} {LastName}".Trim();
        if (string.IsNullOrWhiteSpace(FullName))
        {
            FullName = Username.Value;
        }
    }
}
