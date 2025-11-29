namespace FAM.Domain.Common;

/// <summary>
/// Error codes for internationalization (i18n) support.
/// Frontend can use these codes to display localized error messages.
/// </summary>
public static class ErrorCodes
{
    #region Authentication (AUTH_xxx)
    
    /// <summary>Invalid credentials provided</summary>
    public const string AUTH_INVALID_CREDENTIALS = "AUTH_INVALID_CREDENTIALS";
    
    /// <summary>User account is locked</summary>
    public const string AUTH_ACCOUNT_LOCKED = "AUTH_ACCOUNT_LOCKED";
    
    /// <summary>User account is not active</summary>
    public const string AUTH_ACCOUNT_INACTIVE = "AUTH_ACCOUNT_INACTIVE";
    
    /// <summary>Email is not verified</summary>
    public const string AUTH_EMAIL_NOT_VERIFIED = "AUTH_EMAIL_NOT_VERIFIED";
    
    /// <summary>Token is invalid or expired</summary>
    public const string AUTH_INVALID_TOKEN = "AUTH_INVALID_TOKEN";
    
    /// <summary>Refresh token is invalid or expired</summary>
    public const string AUTH_INVALID_REFRESH_TOKEN = "AUTH_INVALID_REFRESH_TOKEN";
    
    /// <summary>Two-factor authentication is required</summary>
    public const string AUTH_2FA_REQUIRED = "AUTH_2FA_REQUIRED";
    
    /// <summary>Two-factor code is invalid</summary>
    public const string AUTH_INVALID_2FA_CODE = "AUTH_INVALID_2FA_CODE";
    
    /// <summary>Session expired</summary>
    public const string AUTH_SESSION_EXPIRED = "AUTH_SESSION_EXPIRED";
    
    /// <summary>Unauthorized access</summary>
    public const string AUTH_UNAUTHORIZED = "AUTH_UNAUTHORIZED";
    
    /// <summary>Access denied - insufficient permissions</summary>
    public const string AUTH_FORBIDDEN = "AUTH_FORBIDDEN";
    
    /// <summary>Password is too weak</summary>
    public const string AUTH_WEAK_PASSWORD = "AUTH_WEAK_PASSWORD";
    
    /// <summary>Old password is incorrect</summary>
    public const string AUTH_INVALID_OLD_PASSWORD = "AUTH_INVALID_OLD_PASSWORD";

    #endregion

    #region User (USER_xxx)
    
    /// <summary>User not found</summary>
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    
    /// <summary>Username already exists</summary>
    public const string USER_USERNAME_EXISTS = "USER_USERNAME_EXISTS";
    
    /// <summary>Email already exists</summary>
    public const string USER_EMAIL_EXISTS = "USER_EMAIL_EXISTS";
    
    /// <summary>Phone number already exists</summary>
    public const string USER_PHONE_EXISTS = "USER_PHONE_EXISTS";
    
    /// <summary>Invalid username format</summary>
    public const string USER_INVALID_USERNAME = "USER_INVALID_USERNAME";
    
    /// <summary>Invalid email format</summary>
    public const string USER_INVALID_EMAIL = "USER_INVALID_EMAIL";
    
    /// <summary>Invalid phone number format</summary>
    public const string USER_INVALID_PHONE = "USER_INVALID_PHONE";
    
    /// <summary>User is already active</summary>
    public const string USER_ALREADY_ACTIVE = "USER_ALREADY_ACTIVE";
    
    /// <summary>User is already inactive</summary>
    public const string USER_ALREADY_INACTIVE = "USER_ALREADY_INACTIVE";
    
    /// <summary>Cannot delete yourself</summary>
    public const string USER_CANNOT_DELETE_SELF = "USER_CANNOT_DELETE_SELF";

    #endregion

    #region Validation (VAL_xxx)
    
    /// <summary>Required field is missing</summary>
    public const string VAL_REQUIRED = "VAL_REQUIRED";
    
    /// <summary>Field value is too short</summary>
    public const string VAL_TOO_SHORT = "VAL_TOO_SHORT";
    
    /// <summary>Field value is too long</summary>
    public const string VAL_TOO_LONG = "VAL_TOO_LONG";
    
    /// <summary>Field value is out of range</summary>
    public const string VAL_OUT_OF_RANGE = "VAL_OUT_OF_RANGE";
    
    /// <summary>Invalid format</summary>
    public const string VAL_INVALID_FORMAT = "VAL_INVALID_FORMAT";
    
    /// <summary>Invalid value</summary>
    public const string VAL_INVALID_VALUE = "VAL_INVALID_VALUE";
    
    /// <summary>Duplicate value</summary>
    public const string VAL_DUPLICATE = "VAL_DUPLICATE";

    #endregion

    #region Signing Key (KEY_xxx)
    
    /// <summary>Signing key not found</summary>
    public const string KEY_NOT_FOUND = "KEY_NOT_FOUND";
    
    /// <summary>No active signing key available</summary>
    public const string KEY_NO_ACTIVE_KEY = "KEY_NO_ACTIVE_KEY";
    
    /// <summary>Key is already active</summary>
    public const string KEY_ALREADY_ACTIVE = "KEY_ALREADY_ACTIVE";
    
    /// <summary>Key is already inactive</summary>
    public const string KEY_ALREADY_INACTIVE = "KEY_ALREADY_INACTIVE";
    
    /// <summary>Key is already revoked</summary>
    public const string KEY_ALREADY_REVOKED = "KEY_ALREADY_REVOKED";
    
    /// <summary>Key is expired</summary>
    public const string KEY_EXPIRED = "KEY_EXPIRED";
    
    /// <summary>Cannot delete active key</summary>
    public const string KEY_CANNOT_DELETE_ACTIVE = "KEY_CANNOT_DELETE_ACTIVE";
    
    /// <summary>Must revoke key before deleting</summary>
    public const string KEY_MUST_REVOKE_FIRST = "KEY_MUST_REVOKE_FIRST";
    
    /// <summary>Invalid algorithm</summary>
    public const string KEY_INVALID_ALGORITHM = "KEY_INVALID_ALGORITHM";
    
    /// <summary>Invalid key size</summary>
    public const string KEY_INVALID_SIZE = "KEY_INVALID_SIZE";

    #endregion

    #region Organization (ORG_xxx)
    
    /// <summary>Organization not found</summary>
    public const string ORG_NOT_FOUND = "ORG_NOT_FOUND";
    
    /// <summary>Organization name already exists</summary>
    public const string ORG_NAME_EXISTS = "ORG_NAME_EXISTS";
    
    /// <summary>Organization code already exists</summary>
    public const string ORG_CODE_EXISTS = "ORG_CODE_EXISTS";
    
    /// <summary>Cannot delete organization with children</summary>
    public const string ORG_HAS_CHILDREN = "ORG_HAS_CHILDREN";
    
    /// <summary>Cannot delete organization with members</summary>
    public const string ORG_HAS_MEMBERS = "ORG_HAS_MEMBERS";
    
    /// <summary>Invalid parent organization</summary>
    public const string ORG_INVALID_PARENT = "ORG_INVALID_PARENT";
    
    /// <summary>Circular reference detected</summary>
    public const string ORG_CIRCULAR_REFERENCE = "ORG_CIRCULAR_REFERENCE";

    #endregion

    #region Role & Permission (ROLE_xxx / PERM_xxx)
    
    /// <summary>Role not found</summary>
    public const string ROLE_NOT_FOUND = "ROLE_NOT_FOUND";
    
    /// <summary>Role name already exists</summary>
    public const string ROLE_NAME_EXISTS = "ROLE_NAME_EXISTS";
    
    /// <summary>Cannot delete system role</summary>
    public const string ROLE_SYSTEM_PROTECTED = "ROLE_SYSTEM_PROTECTED";
    
    /// <summary>Cannot delete role with users</summary>
    public const string ROLE_HAS_USERS = "ROLE_HAS_USERS";
    
    /// <summary>Permission not found</summary>
    public const string PERM_NOT_FOUND = "PERM_NOT_FOUND";
    
    /// <summary>Permission already assigned</summary>
    public const string PERM_ALREADY_ASSIGNED = "PERM_ALREADY_ASSIGNED";

    #endregion

    #region Storage (STOR_xxx)
    
    /// <summary>File not found</summary>
    public const string STOR_FILE_NOT_FOUND = "STOR_FILE_NOT_FOUND";
    
    /// <summary>File type not allowed</summary>
    public const string STOR_FILE_TYPE_NOT_ALLOWED = "STOR_FILE_TYPE_NOT_ALLOWED";
    
    /// <summary>File size exceeds limit</summary>
    public const string STOR_FILE_TOO_LARGE = "STOR_FILE_TOO_LARGE";
    
    /// <summary>Upload session not found</summary>
    public const string STOR_SESSION_NOT_FOUND = "STOR_SESSION_NOT_FOUND";
    
    /// <summary>Upload session expired</summary>
    public const string STOR_SESSION_EXPIRED = "STOR_SESSION_EXPIRED";
    
    /// <summary>Storage quota exceeded</summary>
    public const string STOR_QUOTA_EXCEEDED = "STOR_QUOTA_EXCEEDED";

    #endregion

    #region Asset (ASSET_xxx)
    
    /// <summary>Asset not found</summary>
    public const string ASSET_NOT_FOUND = "ASSET_NOT_FOUND";
    
    /// <summary>Asset code already exists</summary>
    public const string ASSET_CODE_EXISTS = "ASSET_CODE_EXISTS";
    
    /// <summary>Asset is already assigned</summary>
    public const string ASSET_ALREADY_ASSIGNED = "ASSET_ALREADY_ASSIGNED";
    
    /// <summary>Asset is not available</summary>
    public const string ASSET_NOT_AVAILABLE = "ASSET_NOT_AVAILABLE";
    
    /// <summary>Invalid asset status transition</summary>
    public const string ASSET_INVALID_STATUS = "ASSET_INVALID_STATUS";
    
    /// <summary>Asset cannot be deleted</summary>
    public const string ASSET_CANNOT_DELETE = "ASSET_CANNOT_DELETE";

    #endregion

    #region General (GEN_xxx)
    
    /// <summary>Resource not found</summary>
    public const string GEN_NOT_FOUND = "GEN_NOT_FOUND";
    
    /// <summary>Invalid operation</summary>
    public const string GEN_INVALID_OPERATION = "GEN_INVALID_OPERATION";
    
    /// <summary>Conflict - resource already exists</summary>
    public const string GEN_CONFLICT = "GEN_CONFLICT";
    
    /// <summary>Internal server error</summary>
    public const string GEN_INTERNAL_ERROR = "GEN_INTERNAL_ERROR";
    
    /// <summary>Service unavailable</summary>
    public const string GEN_SERVICE_UNAVAILABLE = "GEN_SERVICE_UNAVAILABLE";
    
    /// <summary>Rate limit exceeded</summary>
    public const string GEN_RATE_LIMITED = "GEN_RATE_LIMITED";
    
    /// <summary>Request timeout</summary>
    public const string GEN_TIMEOUT = "GEN_TIMEOUT";

    #endregion

    #region Menu (MENU_xxx)
    
    /// <summary>Menu item not found</summary>
    public const string MENU_NOT_FOUND = "MENU_NOT_FOUND";
    
    /// <summary>Menu code already exists</summary>
    public const string MENU_CODE_EXISTS = "MENU_CODE_EXISTS";
    
    /// <summary>Cannot delete menu with children</summary>
    public const string MENU_HAS_CHILDREN = "MENU_HAS_CHILDREN";
    
    /// <summary>Invalid parent menu</summary>
    public const string MENU_INVALID_PARENT = "MENU_INVALID_PARENT";
    
    /// <summary>Maximum nesting level exceeded</summary>
    public const string MENU_MAX_DEPTH_EXCEEDED = "MENU_MAX_DEPTH_EXCEEDED";
    
    /// <summary>Menu circular reference detected</summary>
    public const string MENU_CIRCULAR_REFERENCE = "MENU_CIRCULAR_REFERENCE";

    #endregion

    #region System Setting (SETTING_xxx)
    
    /// <summary>Setting not found</summary>
    public const string SETTING_NOT_FOUND = "SETTING_NOT_FOUND";
    
    /// <summary>Setting key already exists</summary>
    public const string SETTING_KEY_EXISTS = "SETTING_KEY_EXISTS";
    
    /// <summary>Setting is not editable</summary>
    public const string SETTING_NOT_EDITABLE = "SETTING_NOT_EDITABLE";
    
    /// <summary>Setting value is required</summary>
    public const string SETTING_VALUE_REQUIRED = "SETTING_VALUE_REQUIRED";
    
    /// <summary>Invalid setting value</summary>
    public const string SETTING_INVALID_VALUE = "SETTING_INVALID_VALUE";

    #endregion
}
