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

    /// <summary>Reset token is invalid or expired</summary>
    public const string AUTH_INVALID_RESET_TOKEN = "AUTH_INVALID_RESET_TOKEN";

    /// <summary>Reset token has expired</summary>
    public const string AUTH_RESET_TOKEN_EXPIRED = "AUTH_RESET_TOKEN_EXPIRED";

    /// <summary>Password reset email sent successfully</summary>
    public const string AUTH_RESET_EMAIL_SENT = "AUTH_RESET_EMAIL_SENT";

    /// <summary>Password reset successful</summary>
    public const string AUTH_PASSWORD_RESET_SUCCESS = "AUTH_PASSWORD_RESET_SUCCESS";

    /// <summary>Reset token is valid</summary>
    public const string AUTH_RESET_TOKEN_VALID = "AUTH_RESET_TOKEN_VALID";

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

    // Auth Request Validations
    /// <summary>Identity (username or email) is required</summary>
    public const string VAL_IDENTITY_REQUIRED = "VAL_IDENTITY_REQUIRED";

    /// <summary>Identity too short</summary>
    public const string VAL_IDENTITY_TOO_SHORT = "VAL_IDENTITY_TOO_SHORT";

    /// <summary>Identity too long</summary>
    public const string VAL_IDENTITY_TOO_LONG = "VAL_IDENTITY_TOO_LONG";

    /// <summary>Password is required</summary>
    public const string VAL_PASSWORD_REQUIRED = "VAL_PASSWORD_REQUIRED";

    /// <summary>Password too short</summary>
    public const string VAL_PASSWORD_TOO_SHORT = "VAL_PASSWORD_TOO_SHORT";

    /// <summary>Password too long</summary>
    public const string VAL_PASSWORD_TOO_LONG = "VAL_PASSWORD_TOO_LONG";

    /// <summary>Current password is required</summary>
    public const string VAL_CURRENT_PASSWORD_REQUIRED = "VAL_CURRENT_PASSWORD_REQUIRED";

    /// <summary>New password is required</summary>
    public const string VAL_NEW_PASSWORD_REQUIRED = "VAL_NEW_PASSWORD_REQUIRED";

    /// <summary>New password too short</summary>
    public const string VAL_NEW_PASSWORD_TOO_SHORT = "VAL_NEW_PASSWORD_TOO_SHORT";

    /// <summary>New password too long</summary>
    public const string VAL_NEW_PASSWORD_TOO_LONG = "VAL_NEW_PASSWORD_TOO_LONG";

    /// <summary>New password must contain uppercase letter</summary>
    public const string VAL_NEW_PASSWORD_NO_UPPERCASE = "VAL_NEW_PASSWORD_NO_UPPERCASE";

    /// <summary>New password must contain lowercase letter</summary>
    public const string VAL_NEW_PASSWORD_NO_LOWERCASE = "VAL_NEW_PASSWORD_NO_LOWERCASE";

    /// <summary>New password must contain digit</summary>
    public const string VAL_NEW_PASSWORD_NO_DIGIT = "VAL_NEW_PASSWORD_NO_DIGIT";

    /// <summary>New password must contain special character</summary>
    public const string VAL_NEW_PASSWORD_NO_SPECIAL = "VAL_NEW_PASSWORD_NO_SPECIAL";

    /// <summary>Confirm password is required</summary>
    public const string VAL_CONFIRM_PASSWORD_REQUIRED = "VAL_CONFIRM_PASSWORD_REQUIRED";

    /// <summary>Passwords do not match</summary>
    public const string VAL_PASSWORDS_MISMATCH = "VAL_PASSWORDS_MISMATCH";

    /// <summary>Email is required</summary>
    public const string VAL_EMAIL_REQUIRED = "VAL_EMAIL_REQUIRED";

    /// <summary>Email too long</summary>
    public const string VAL_EMAIL_TOO_LONG = "VAL_EMAIL_TOO_LONG";

    /// <summary>Invalid email format</summary>
    public const string VAL_EMAIL_INVALID = "VAL_EMAIL_INVALID";

    /// <summary>Username is required</summary>
    public const string VAL_USERNAME_REQUIRED = "VAL_USERNAME_REQUIRED";

    /// <summary>Username too short</summary>
    public const string VAL_USERNAME_TOO_SHORT = "VAL_USERNAME_TOO_SHORT";

    /// <summary>Username too long</summary>
    public const string VAL_USERNAME_TOO_LONG = "VAL_USERNAME_TOO_LONG";

    // 2FA Validations
    /// <summary>Two-factor code is required</summary>
    public const string VAL_2FA_CODE_REQUIRED = "VAL_2FA_CODE_REQUIRED";

    /// <summary>Two-factor code invalid length</summary>
    public const string VAL_2FA_CODE_INVALID_LENGTH = "VAL_2FA_CODE_INVALID_LENGTH";

    /// <summary>Two-factor code invalid format</summary>
    public const string VAL_2FA_CODE_INVALID_FORMAT = "VAL_2FA_CODE_INVALID_FORMAT";

    /// <summary>Two-factor session token is required</summary>
    public const string VAL_2FA_SESSION_REQUIRED = "VAL_2FA_SESSION_REQUIRED";

    /// <summary>Authentication method is required</summary>
    public const string VAL_AUTH_METHOD_REQUIRED = "VAL_AUTH_METHOD_REQUIRED";

    /// <summary>Invalid authentication method</summary>
    public const string VAL_AUTH_METHOD_INVALID = "VAL_AUTH_METHOD_INVALID";

    /// <summary>Email OTP is required</summary>
    public const string VAL_EMAIL_OTP_REQUIRED = "VAL_EMAIL_OTP_REQUIRED";

    /// <summary>Email OTP invalid length</summary>
    public const string VAL_EMAIL_OTP_INVALID_LENGTH = "VAL_EMAIL_OTP_INVALID_LENGTH";

    /// <summary>Email OTP invalid format</summary>
    public const string VAL_EMAIL_OTP_INVALID_FORMAT = "VAL_EMAIL_OTP_INVALID_FORMAT";

    /// <summary>Recovery code is required</summary>
    public const string VAL_RECOVERY_CODE_REQUIRED = "VAL_RECOVERY_CODE_REQUIRED";

    /// <summary>Recovery code too short</summary>
    public const string VAL_RECOVERY_CODE_TOO_SHORT = "VAL_RECOVERY_CODE_TOO_SHORT";

    /// <summary>Recovery code too long</summary>
    public const string VAL_RECOVERY_CODE_TOO_LONG = "VAL_RECOVERY_CODE_TOO_LONG";

    /// <summary>Backup code is required</summary>
    public const string VAL_BACKUP_CODE_REQUIRED = "VAL_BACKUP_CODE_REQUIRED";

    /// <summary>Refresh token is required</summary>
    public const string VAL_REFRESH_TOKEN_REQUIRED = "VAL_REFRESH_TOKEN_REQUIRED";

    /// <summary>Reset token is required</summary>
    public const string VAL_RESET_TOKEN_REQUIRED = "VAL_RESET_TOKEN_REQUIRED";

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

    /// <summary>Organization node not found</summary>
    public const string NODE_NOT_FOUND = "NODE_NOT_FOUND";

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

    #region Value Objects (VO_xxx)

    // PostalCode
    /// <summary>Postal code cannot be empty</summary>
    public const string VO_POSTAL_CODE_EMPTY = "VO_POSTAL_CODE_EMPTY";

    /// <summary>Invalid postal code format</summary>
    public const string VO_POSTAL_CODE_INVALID = "VO_POSTAL_CODE_INVALID";

    // Email
    /// <summary>Email cannot be empty</summary>
    public const string VO_EMAIL_EMPTY = "VO_EMAIL_EMPTY";

    /// <summary>Invalid email format</summary>
    public const string VO_EMAIL_INVALID = "VO_EMAIL_INVALID";

    /// <summary>Email too long</summary>
    public const string VO_EMAIL_TOO_LONG = "VO_EMAIL_TOO_LONG";

    // PhoneNumber
    /// <summary>Phone number cannot be empty</summary>
    public const string VO_PHONE_EMPTY = "VO_PHONE_EMPTY";

    /// <summary>Invalid phone number format</summary>
    public const string VO_PHONE_INVALID = "VO_PHONE_INVALID";

    // IPAddress
    /// <summary>IP address cannot be empty</summary>
    public const string VO_IP_EMPTY = "VO_IP_EMPTY";

    /// <summary>Invalid IP address format</summary>
    public const string VO_IP_INVALID = "VO_IP_INVALID";

    // Address
    /// <summary>Street cannot be empty</summary>
    public const string VO_ADDRESS_STREET_EMPTY = "VO_ADDRESS_STREET_EMPTY";

    /// <summary>City cannot be empty</summary>
    public const string VO_ADDRESS_CITY_EMPTY = "VO_ADDRESS_CITY_EMPTY";

    /// <summary>Country cannot be empty</summary>
    public const string VO_ADDRESS_COUNTRY_EMPTY = "VO_ADDRESS_COUNTRY_EMPTY";

    // Password
    /// <summary>Password cannot be empty</summary>
    public const string VO_PASSWORD_EMPTY = "VO_PASSWORD_EMPTY";

    /// <summary>Password too short</summary>
    public const string VO_PASSWORD_TOO_SHORT = "VO_PASSWORD_TOO_SHORT";

    /// <summary>Password too long</summary>
    public const string VO_PASSWORD_TOO_LONG = "VO_PASSWORD_TOO_LONG";

    /// <summary>Password must contain uppercase letter</summary>
    public const string VO_PASSWORD_NO_UPPERCASE = "VO_PASSWORD_NO_UPPERCASE";

    /// <summary>Password must contain lowercase letter</summary>
    public const string VO_PASSWORD_NO_LOWERCASE = "VO_PASSWORD_NO_LOWERCASE";

    /// <summary>Password must contain digit</summary>
    public const string VO_PASSWORD_NO_DIGIT = "VO_PASSWORD_NO_DIGIT";

    /// <summary>Password must contain at least one number</summary>
    public const string VO_PASSWORD_NO_NUMBER = "VO_PASSWORD_NO_NUMBER";

    /// <summary>Password must contain special character</summary>
    public const string VO_PASSWORD_NO_SPECIAL = "VO_PASSWORD_NO_SPECIAL";

    // Money
    /// <summary>Amount cannot be negative</summary>
    public const string VO_MONEY_NEGATIVE = "VO_MONEY_NEGATIVE";

    /// <summary>Currency code cannot be empty</summary>
    public const string VO_MONEY_CURRENCY_EMPTY = "VO_MONEY_CURRENCY_EMPTY";

    /// <summary>Invalid currency code</summary>
    public const string VO_MONEY_CURRENCY_INVALID = "VO_MONEY_CURRENCY_INVALID";

    // Percentage
    /// <summary>Percentage out of range</summary>
    public const string VO_PERCENTAGE_OUT_OF_RANGE = "VO_PERCENTAGE_OUT_OF_RANGE";

    /// <summary>Percentage cannot be negative</summary>
    public const string VO_PERCENTAGE_NEGATIVE = "VO_PERCENTAGE_NEGATIVE";

    /// <summary>Percentage cannot exceed 100%</summary>
    public const string VO_PERCENTAGE_EXCEED = "VO_PERCENTAGE_EXCEED";

    // SerialNumber
    /// <summary>Serial number cannot be empty</summary>
    public const string VO_SERIAL_EMPTY = "VO_SERIAL_EMPTY";

    /// <summary>Invalid serial number format</summary>
    public const string VO_SERIAL_INVALID = "VO_SERIAL_INVALID";

    // TaxCode
    /// <summary>Tax code cannot be empty</summary>
    public const string VO_TAX_CODE_EMPTY = "VO_TAX_CODE_EMPTY";

    /// <summary>Invalid tax code format</summary>
    public const string VO_TAX_CODE_INVALID = "VO_TAX_CODE_INVALID";

    /// <summary>Tax code length is invalid</summary>
    public const string VO_TAX_CODE_TOO_SHORT = "VO_TAX_CODE_TOO_SHORT";

    public const string VO_TAX_CODE_TOO_LONG = "VO_TAX_CODE_TOO_LONG";

    // MACAddress
    /// <summary>MAC address cannot be empty</summary>
    public const string VO_MAC_EMPTY = "VO_MAC_EMPTY";

    /// <summary>Invalid MAC address format</summary>
    public const string VO_MAC_INVALID = "VO_MAC_INVALID";

    // DateRange
    /// <summary>Start date cannot be after end date</summary>
    public const string VO_DATE_RANGE_INVALID = "VO_DATE_RANGE_INVALID";

    // Rating
    /// <summary>Rating out of range</summary>
    public const string VO_RATING_OUT_OF_RANGE = "VO_RATING_OUT_OF_RANGE";

    // Dimensions
    /// <summary>Dimension value must be positive</summary>
    public const string VO_DIMENSION_INVALID = "VO_DIMENSION_INVALID";

    /// <summary>Unit cannot be empty</summary>
    public const string VO_DIMENSION_UNIT_EMPTY = "VO_DIMENSION_UNIT_EMPTY";

    // DomainName
    /// <summary>Domain name cannot be empty</summary>
    public const string VO_DOMAIN_EMPTY = "VO_DOMAIN_EMPTY";

    /// <summary>Domain name too long</summary>
    public const string VO_DOMAIN_TOO_LONG = "VO_DOMAIN_TOO_LONG";

    /// <summary>Invalid domain name format</summary>
    public const string VO_DOMAIN_INVALID = "VO_DOMAIN_INVALID";

    // URL
    /// <summary>URL cannot be empty</summary>
    public const string VO_URL_EMPTY = "VO_URL_EMPTY";

    /// <summary>Invalid URL format</summary>
    public const string VO_URL_INVALID = "VO_URL_INVALID";

    // CountryCode
    /// <summary>Country code cannot be empty</summary>
    public const string VO_COUNTRY_CODE_EMPTY = "VO_COUNTRY_CODE_EMPTY";

    /// <summary>Country code must be 2 characters</summary>
    public const string VO_COUNTRY_CODE_INVALID = "VO_COUNTRY_CODE_INVALID";

    // ResourceType
    /// <summary>Resource type cannot be empty</summary>
    public const string VO_RESOURCE_TYPE_EMPTY = "VO_RESOURCE_TYPE_EMPTY";

    /// <summary>Resource type too long</summary>
    public const string VO_RESOURCE_TYPE_TOO_LONG = "VO_RESOURCE_TYPE_TOO_LONG";

    // ResourceAction
    /// <summary>Resource action cannot be empty</summary>
    public const string VO_RESOURCE_ACTION_EMPTY = "VO_RESOURCE_ACTION_EMPTY";

    /// <summary>Resource action cannot exceed 50 characters</summary>
    public const string VO_RESOURCE_ACTION_TOO_LONG = "VO_RESOURCE_ACTION_TOO_LONG";

    // Warranty, Insurance, Depreciation
    /// <summary>Warranty duration must be positive</summary>
    public const string VO_WARRANTY_DURATION_INVALID = "VO_WARRANTY_DURATION_INVALID";

    /// <summary>End date must be after start date</summary>
    public const string VO_DATE_INVALID = "VO_DATE_INVALID";

    /// <summary>Policy number is required</summary>
    public const string VO_INSURANCE_POLICY_EMPTY = "VO_INSURANCE_POLICY_EMPTY";

    /// <summary>Insured value must be positive</summary>
    public const string VO_INSURANCE_VALUE_INVALID = "VO_INSURANCE_VALUE_INVALID";

    /// <summary>Depreciation method is required</summary>
    public const string VO_DEPRECIATION_METHOD_EMPTY = "VO_DEPRECIATION_METHOD_EMPTY";

    /// <summary>Useful life must be positive</summary>
    public const string VO_DEPRECIATION_LIFE_INVALID = "VO_DEPRECIATION_LIFE_INVALID";

    /// <summary>Residual value cannot be negative</summary>
    public const string VO_DEPRECIATION_RESIDUAL_INVALID = "VO_DEPRECIATION_RESIDUAL_INVALID";

    // CostCenter, AssetTag, Username, RoleCode
    /// <summary>Cost center cannot be empty</summary>
    public const string VO_COST_CENTER_EMPTY = "VO_COST_CENTER_EMPTY";

    /// <summary>Cost center too long</summary>
    public const string VO_COST_CENTER_TOO_LONG = "VO_COST_CENTER_TOO_LONG";

    /// <summary>Asset tag cannot be empty</summary>
    public const string VO_ASSET_TAG_EMPTY = "VO_ASSET_TAG_EMPTY";

    /// <summary>Asset tag too long</summary>
    public const string VO_ASSET_TAG_TOO_LONG = "VO_ASSET_TAG_TOO_LONG";

    /// <summary>Username cannot be empty</summary>
    public const string VO_USERNAME_EMPTY = "VO_USERNAME_EMPTY";

    /// <summary>Username too short</summary>
    public const string VO_USERNAME_TOO_SHORT = "VO_USERNAME_TOO_SHORT";

    /// <summary>Username too long</summary>
    public const string VO_USERNAME_TOO_LONG = "VO_USERNAME_TOO_LONG";

    /// <summary>Invalid username format</summary>
    public const string VO_USERNAME_INVALID = "VO_USERNAME_INVALID";

    /// <summary>Role code cannot be empty</summary>
    public const string VO_ROLE_CODE_EMPTY = "VO_ROLE_CODE_EMPTY";

    /// <summary>Role code too long</summary>
    public const string VO_ROLE_CODE_TOO_LONG = "VO_ROLE_CODE_TOO_LONG";

    /// <summary>Invalid role code format</summary>
    public const string VO_ROLE_CODE_INVALID = "VO_ROLE_CODE_INVALID";

    #endregion

    #region Role & Permission (ROLE_xxx, PERMISSION_xxx)

    /// <summary>Role not found</summary>
    public const string ROLE_NOT_FOUND = "ROLE_NOT_FOUND";

    /// <summary>Role name is required</summary>
    public const string ROLE_NAME_REQUIRED = "ROLE_NAME_REQUIRED";

    /// <summary>Role code already exists</summary>
    public const string ROLE_CODE_EXISTS = "ROLE_CODE_EXISTS";

    /// <summary>Invalid role rank</summary>
    public const string ROLE_INVALID_RANK = "ROLE_INVALID_RANK";

    /// <summary>System role cannot be updated</summary>
    public const string ROLE_SYSTEM_ROLE_CANNOT_UPDATE = "ROLE_SYSTEM_ROLE_CANNOT_UPDATE";

    /// <summary>System role cannot be deleted</summary>
    public const string ROLE_SYSTEM_ROLE_CANNOT_DELETE = "ROLE_SYSTEM_ROLE_CANNOT_DELETE";

    /// <summary>No permissions provided</summary>
    public const string ROLE_NO_PERMISSIONS_PROVIDED = "ROLE_NO_PERMISSIONS_PROVIDED";

    /// <summary>Role is in use and cannot be deleted</summary>
    public const string ROLE_IN_USE = "ROLE_IN_USE";

    /// <summary>Permission not found</summary>
    public const string PERMISSION_NOT_FOUND = "PERMISSION_NOT_FOUND";

    /// <summary>Invalid permission</summary>
    public const string PERMISSION_INVALID = "PERMISSION_INVALID";

    /// <summary>Permission already exists</summary>
    public const string PERMISSION_EXISTS = "PERMISSION_EXISTS";

    /// <summary>Permission already assigned to role</summary>
    public const string PERMISSION_ALREADY_ASSIGNED = "PERMISSION_ALREADY_ASSIGNED";

    /// <summary>Permission not assigned to role</summary>
    public const string PERMISSION_NOT_ASSIGNED = "PERMISSION_NOT_ASSIGNED";

    /// <summary>Role assignment invalid date range</summary>
    public const string ROLE_ASSIGNMENT_INVALID_DATE_RANGE = "ROLE_ASSIGNMENT_INVALID_DATE_RANGE";

    /// <summary>Role already assigned to user at node</summary>
    public const string ROLE_ALREADY_ASSIGNED_TO_USER = "ROLE_ALREADY_ASSIGNED_TO_USER";

    /// <summary>Role assignment not found</summary>
    public const string ROLE_ASSIGNMENT_NOT_FOUND = "ROLE_ASSIGNMENT_NOT_FOUND";

    #endregion

    #region Email Template (EMAIL_TEMPLATE_xxx)

    /// <summary>Email template code is required</summary>
    public const string EMAIL_TEMPLATE_CODE_REQUIRED = "EMAIL_TEMPLATE_CODE_REQUIRED";

    /// <summary>Email template name is required</summary>
    public const string EMAIL_TEMPLATE_NAME_REQUIRED = "EMAIL_TEMPLATE_NAME_REQUIRED";

    /// <summary>Email template subject is required</summary>
    public const string EMAIL_TEMPLATE_SUBJECT_REQUIRED = "EMAIL_TEMPLATE_SUBJECT_REQUIRED";

    /// <summary>Email template body is required</summary>
    public const string EMAIL_TEMPLATE_BODY_REQUIRED = "EMAIL_TEMPLATE_BODY_REQUIRED";

    /// <summary>Email template not found</summary>
    public const string EMAIL_TEMPLATE_NOT_FOUND = "EMAIL_TEMPLATE_NOT_FOUND";

    /// <summary>Email template code already exists</summary>
    public const string EMAIL_TEMPLATE_CODE_EXISTS = "EMAIL_TEMPLATE_CODE_EXISTS";

    /// <summary>Cannot update system email template</summary>
    public const string EMAIL_TEMPLATE_SYSTEM_CANNOT_UPDATE = "EMAIL_TEMPLATE_SYSTEM_CANNOT_UPDATE";

    /// <summary>Cannot delete system email template</summary>
    public const string EMAIL_TEMPLATE_SYSTEM_CANNOT_DELETE = "EMAIL_TEMPLATE_SYSTEM_CANNOT_DELETE";

    #endregion
}