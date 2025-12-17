namespace FAM.Domain.Common.Base;

/// <summary>
/// Default English error messages for each error code.
/// These are fallback messages when translation is not available.
/// </summary>
public static class ErrorMessages
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        #region Authentication

        [ErrorCodes.AUTH_INVALID_CREDENTIALS] = "Invalid username or password",
        [ErrorCodes.AUTH_ACCOUNT_LOCKED] = "Your account has been locked. Please contact support",
        [ErrorCodes.AUTH_ACCOUNT_INACTIVE] = "Your account is not active. Please contact support",
        [ErrorCodes.AUTH_EMAIL_NOT_VERIFIED] = "Please verify your email address before logging in",
        [ErrorCodes.AUTH_INVALID_TOKEN] = "The token is invalid or has expired",
        [ErrorCodes.AUTH_INVALID_REFRESH_TOKEN] = "The refresh token is invalid or has expired. Please log in again",
        [ErrorCodes.AUTH_2FA_REQUIRED] = "Two-factor authentication is required",
        [ErrorCodes.AUTH_INVALID_2FA_CODE] = "The two-factor authentication code is invalid",
        [ErrorCodes.AUTH_SESSION_EXPIRED] = "Your session has expired. Please log in again",
        [ErrorCodes.AUTH_UNAUTHORIZED] = "You are not authorized to access this resource",
        [ErrorCodes.AUTH_FORBIDDEN] = "You do not have permission to perform this action",
        [ErrorCodes.AUTH_WEAK_PASSWORD] = "Password is too weak. Please use a stronger password",
        [ErrorCodes.AUTH_INVALID_OLD_PASSWORD] = "The current password is incorrect",
        [ErrorCodes.AUTH_INVALID_RESET_TOKEN] = "The reset token is invalid or expired",
        [ErrorCodes.AUTH_RESET_EMAIL_SENT] = "Password reset email has been sent to your email address",
        [ErrorCodes.AUTH_RESET_TOKEN_EXPIRED] = "The reset token has expired",
        [ErrorCodes.AUTH_PASSWORD_RESET_SUCCESS] = "Your password has been reset successfully",
        [ErrorCodes.AUTH_RESET_TOKEN_VALID] = "The reset token is valid",

        #endregion

        #region User

        [ErrorCodes.USER_NOT_FOUND] = "User not found",
        [ErrorCodes.USER_USERNAME_EXISTS] = "This username is already taken",
        [ErrorCodes.USER_EMAIL_EXISTS] = "This email address is already registered",
        [ErrorCodes.USER_PHONE_EXISTS] = "This phone number is already registered",
        [ErrorCodes.USER_INVALID_USERNAME] =
            "Username format is invalid. Use only letters, numbers, dots, underscores, and hyphens",
        [ErrorCodes.USER_INVALID_EMAIL] = "Email address format is invalid",
        [ErrorCodes.USER_INVALID_PHONE] = "Phone number format is invalid",
        [ErrorCodes.USER_ALREADY_ACTIVE] = "User is already active",
        [ErrorCodes.USER_ALREADY_INACTIVE] = "User is already inactive",
        [ErrorCodes.USER_CANNOT_DELETE_SELF] = "You cannot delete your own account",
        [ErrorCodes.USER_SESSION_NOT_FOUND] = "Session not found or access denied",
        [ErrorCodes.DEVICE_NOT_FOUND] = "Device not found",
        [ErrorCodes.DEVICE_NOT_TRUSTED_FOR_OPERATION] =
            "This device is not trusted enough for this operation. For security reasons, devices must be trusted for at least 3 days before they can delete other sessions.",

        #endregion

        #region Validation

        [ErrorCodes.VAL_REQUIRED] = "This field is required",
        [ErrorCodes.VAL_TOO_SHORT] = "The value is too short",
        [ErrorCodes.VAL_TOO_LONG] = "The value is too long",
        [ErrorCodes.VAL_OUT_OF_RANGE] = "The value is out of allowed range",
        [ErrorCodes.VAL_INVALID_FORMAT] = "The format is invalid",
        [ErrorCodes.VAL_INVALID_VALUE] = "The value is invalid",
        [ErrorCodes.VAL_DUPLICATE] = "This value already exists",

        // Auth Request Validations
        [ErrorCodes.VAL_IDENTITY_REQUIRED] = "Username or email is required",
        [ErrorCodes.VAL_IDENTITY_TOO_SHORT] = "Username or email must be at least {0} characters",
        [ErrorCodes.VAL_IDENTITY_TOO_LONG] = "Username or email must not exceed {0} characters",
        [ErrorCodes.VAL_PASSWORD_REQUIRED] = "Password is required",
        [ErrorCodes.VAL_PASSWORD_TOO_SHORT] = "Password must be at least {0} characters",
        [ErrorCodes.VAL_PASSWORD_TOO_LONG] = "Password must not exceed {0} characters",
        [ErrorCodes.VAL_CURRENT_PASSWORD_REQUIRED] = "Current password is required",
        [ErrorCodes.VAL_NEW_PASSWORD_REQUIRED] = "New password is required",
        [ErrorCodes.VAL_NEW_PASSWORD_TOO_SHORT] = "New password must be at least {0} characters",
        [ErrorCodes.VAL_NEW_PASSWORD_TOO_LONG] = "New password must not exceed {0} characters",
        [ErrorCodes.VAL_NEW_PASSWORD_NO_UPPERCASE] = "New password must contain at least one uppercase letter",
        [ErrorCodes.VAL_NEW_PASSWORD_NO_LOWERCASE] = "New password must contain at least one lowercase letter",
        [ErrorCodes.VAL_NEW_PASSWORD_NO_DIGIT] = "New password must contain at least one digit",
        [ErrorCodes.VAL_NEW_PASSWORD_NO_SPECIAL] = "New password must contain at least one special character",
        [ErrorCodes.VAL_CONFIRM_PASSWORD_REQUIRED] = "Password confirmation is required",
        [ErrorCodes.VAL_PASSWORDS_MISMATCH] = "Passwords do not match",
        [ErrorCodes.VAL_EMAIL_REQUIRED] = "Email is required",
        [ErrorCodes.VAL_EMAIL_TOO_LONG] = "Email must not exceed {0} characters",
        [ErrorCodes.VAL_EMAIL_INVALID] = "Invalid email format",
        [ErrorCodes.VAL_USERNAME_REQUIRED] = "Username is required",
        [ErrorCodes.VAL_USERNAME_TOO_SHORT] = "Username must be at least {0} characters",
        [ErrorCodes.VAL_USERNAME_TOO_LONG] = "Username must not exceed {0} characters",

        // 2FA Validations
        [ErrorCodes.VAL_2FA_CODE_REQUIRED] = "Two-factor code is required",
        [ErrorCodes.VAL_2FA_CODE_INVALID_LENGTH] = "Two-factor code must be exactly 6 digits",
        [ErrorCodes.VAL_2FA_CODE_INVALID_FORMAT] = "Two-factor code must contain only digits",
        [ErrorCodes.VAL_2FA_SESSION_REQUIRED] = "Two-factor session token is required",
        [ErrorCodes.VAL_AUTH_METHOD_REQUIRED] = "Authentication method is required",
        [ErrorCodes.VAL_AUTH_METHOD_INVALID] = "Method must be: email_otp, authenticator_app, or recovery_code",
        [ErrorCodes.VAL_EMAIL_OTP_REQUIRED] = "Email OTP is required",
        [ErrorCodes.VAL_EMAIL_OTP_INVALID_LENGTH] = "OTP must be exactly 6 digits",
        [ErrorCodes.VAL_EMAIL_OTP_INVALID_FORMAT] = "OTP must contain only digits",
        [ErrorCodes.VAL_RECOVERY_CODE_REQUIRED] = "Recovery code is required",
        [ErrorCodes.VAL_RECOVERY_CODE_TOO_SHORT] = "Recovery code must be at least 8 characters",
        [ErrorCodes.VAL_RECOVERY_CODE_TOO_LONG] = "Recovery code must not exceed 50 characters",
        [ErrorCodes.VAL_BACKUP_CODE_REQUIRED] = "Backup code is required",
        [ErrorCodes.VAL_REFRESH_TOKEN_REQUIRED] = "Refresh token is required",
        [ErrorCodes.VAL_RESET_TOKEN_REQUIRED] = "Reset token is required",

        #endregion

        #region Signing Key

        [ErrorCodes.KEY_NOT_FOUND] = "Signing key not found",
        [ErrorCodes.KEY_NO_ACTIVE_KEY] = "No active signing key is available",
        [ErrorCodes.KEY_ALREADY_ACTIVE] = "The signing key is already active",
        [ErrorCodes.KEY_ALREADY_INACTIVE] = "The signing key is already inactive",
        [ErrorCodes.KEY_ALREADY_REVOKED] = "Key already revoked",
        [ErrorCodes.KEY_EXPIRED] = "The signing key has expired",
        [ErrorCodes.KEY_CANNOT_DELETE_ACTIVE] = "Cannot delete an active signing key",
        [ErrorCodes.KEY_MUST_REVOKE_FIRST] = "You must revoke the key before deleting it",
        [ErrorCodes.KEY_INVALID_ALGORITHM] = "Invalid algorithm. Must be RS256, RS384, or RS512",
        [ErrorCodes.KEY_INVALID_SIZE] = "Invalid key size. Must be 2048, 3072, or 4096",

        #endregion

        #region Organization

        [ErrorCodes.ORG_NOT_FOUND] = "Organization not found",
        [ErrorCodes.ORG_NAME_EXISTS] = "An organization with this name already exists",
        [ErrorCodes.ORG_CODE_EXISTS] = "An organization with this code already exists",
        [ErrorCodes.ORG_HAS_CHILDREN] = "Cannot delete organization that has child organizations",
        [ErrorCodes.ORG_HAS_MEMBERS] = "Cannot delete organization that has members",
        [ErrorCodes.ORG_INVALID_PARENT] = "The specified parent organization is invalid",
        [ErrorCodes.ORG_CIRCULAR_REFERENCE] = "This operation would create a circular reference",

        #endregion

        #region Role & Permission

        [ErrorCodes.ROLE_NOT_FOUND] = "Role not found",
        [ErrorCodes.ROLE_NAME_REQUIRED] = "Role name is required",
        [ErrorCodes.ROLE_CODE_EXISTS] = "A role with this code already exists",
        [ErrorCodes.ROLE_INVALID_RANK] = "Invalid role rank",
        [ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_UPDATE] = "System roles cannot be modified",
        [ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_DELETE] = "System roles cannot be deleted",
        [ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED] = "No permissions provided",
        [ErrorCodes.ROLE_IN_USE] = "Cannot delete role that is assigned to users",
        [ErrorCodes.PERMISSION_NOT_FOUND] = "Permission not found",
        [ErrorCodes.PERMISSION_INVALID] = "Invalid permission",
        [ErrorCodes.PERMISSION_EXISTS] = "Permission already exists",
        [ErrorCodes.PERMISSION_ALREADY_ASSIGNED] = "This permission is already assigned",
        [ErrorCodes.PERMISSION_NOT_ASSIGNED] = "Permission not assigned to role",
        [ErrorCodes.ROLE_ASSIGNMENT_INVALID_DATE_RANGE] = "End date must be after start date",
        [ErrorCodes.ROLE_ALREADY_ASSIGNED_TO_USER] = "Role already assigned to user at this node",
        [ErrorCodes.ROLE_ASSIGNMENT_NOT_FOUND] = "Role assignment not found",

        #endregion

        #region Storage

        [ErrorCodes.STOR_FILE_NOT_FOUND] = "File not found",
        [ErrorCodes.STOR_FILE_TYPE_NOT_ALLOWED] = "This file type is not allowed",
        [ErrorCodes.STOR_FILE_TOO_LARGE] = "File size exceeds the maximum allowed limit",
        [ErrorCodes.STOR_SESSION_NOT_FOUND] = "Upload session not found",
        [ErrorCodes.STOR_SESSION_EXPIRED] = "Upload session has expired",
        [ErrorCodes.STOR_QUOTA_EXCEEDED] = "Storage quota exceeded",

        #endregion

        #region Asset

        [ErrorCodes.ASSET_NOT_FOUND] = "Asset not found",
        [ErrorCodes.ASSET_CODE_EXISTS] = "An asset with this code already exists",
        [ErrorCodes.ASSET_ALREADY_ASSIGNED] = "This asset is already assigned",
        [ErrorCodes.ASSET_NOT_AVAILABLE] = "This asset is not available",
        [ErrorCodes.ASSET_INVALID_STATUS] = "Invalid asset status transition",
        [ErrorCodes.ASSET_CANNOT_DELETE] = "This asset cannot be deleted",

        #endregion

        #region General

        [ErrorCodes.GEN_NOT_FOUND] = "The requested resource was not found",
        [ErrorCodes.NODE_NOT_FOUND] = "The requested node/location was not found in the hierarchy",
        [ErrorCodes.GEN_INVALID_OPERATION] = "This operation is not valid",
        [ErrorCodes.GEN_CONFLICT] = "A conflict occurred. The resource may already exist",
        [ErrorCodes.GEN_INTERNAL_ERROR] = "An internal server error occurred. Please try again later",
        [ErrorCodes.GEN_SERVICE_UNAVAILABLE] = "Service is temporarily unavailable. Please try again later",
        [ErrorCodes.GEN_RATE_LIMITED] = "Too many requests. Please slow down and try again",
        [ErrorCodes.GEN_TIMEOUT] = "The request timed out. Please try again",

        #endregion

        #region Menu

        [ErrorCodes.MENU_NOT_FOUND] = "Menu item not found",
        [ErrorCodes.MENU_CODE_EXISTS] = "A menu item with this code already exists",
        [ErrorCodes.MENU_HAS_CHILDREN] = "Cannot delete menu item that has children",
        [ErrorCodes.MENU_INVALID_PARENT] = "The specified parent menu is invalid",
        [ErrorCodes.MENU_MAX_DEPTH_EXCEEDED] = "Maximum menu nesting level exceeded. Maximum is 3 levels",
        [ErrorCodes.MENU_CIRCULAR_REFERENCE] = "This operation would create a circular reference",

        #endregion

        #region System Setting

        [ErrorCodes.SETTING_NOT_FOUND] = "Setting not found",
        [ErrorCodes.SETTING_KEY_EXISTS] = "A setting with this key already exists",
        [ErrorCodes.SETTING_NOT_EDITABLE] = "This setting is read-only and cannot be modified",
        [ErrorCodes.SETTING_VALUE_REQUIRED] = "A value is required for this setting",
        [ErrorCodes.SETTING_INVALID_VALUE] = "The value provided is invalid for this setting type",

        #endregion

        #region Value Objects

        // PostalCode
        [ErrorCodes.VO_POSTAL_CODE_EMPTY] = "Postal code cannot be empty",
        [ErrorCodes.VO_POSTAL_CODE_INVALID] = "Invalid postal code format",

        // Email
        [ErrorCodes.VO_EMAIL_EMPTY] = "Email is required",
        [ErrorCodes.VO_EMAIL_INVALID] = "Invalid email format",
        [ErrorCodes.VO_EMAIL_TOO_LONG] = "Email is too long (maximum 256 characters)",

        // PhoneNumber
        [ErrorCodes.VO_PHONE_EMPTY] = "Phone number is required",
        [ErrorCodes.VO_PHONE_INVALID] = "Invalid phone number length",

        // IPAddress
        [ErrorCodes.VO_IP_EMPTY] = "IP address is required",
        [ErrorCodes.VO_IP_INVALID] = "Invalid IP address format",

        // Address
        [ErrorCodes.VO_ADDRESS_STREET_EMPTY] = "Street is required",
        [ErrorCodes.VO_ADDRESS_CITY_EMPTY] = "City is required",
        [ErrorCodes.VO_ADDRESS_COUNTRY_EMPTY] = "Country code is required",

        // Password
        [ErrorCodes.VO_PASSWORD_EMPTY] = "Password cannot be empty",
        [ErrorCodes.VO_PASSWORD_TOO_SHORT] = "Password must be at least 8 characters long",
        [ErrorCodes.VO_PASSWORD_TOO_LONG] = "Password cannot exceed {0} characters",
        [ErrorCodes.VO_PASSWORD_NO_UPPERCASE] = "Password must contain at least one uppercase letter",
        [ErrorCodes.VO_PASSWORD_NO_LOWERCASE] = "Password must contain at least one lowercase letter",
        [ErrorCodes.VO_PASSWORD_NO_DIGIT] = "Password must contain at least one digit",
        [ErrorCodes.VO_PASSWORD_NO_NUMBER] = "Password must contain at least one number",
        [ErrorCodes.VO_PASSWORD_NO_SPECIAL] = "Password must contain at least one special character",

        // Money
        [ErrorCodes.VO_MONEY_NEGATIVE] = "Amount cannot be negative",
        [ErrorCodes.VO_MONEY_CURRENCY_EMPTY] = "Currency is required",
        [ErrorCodes.VO_MONEY_CURRENCY_INVALID] = "Invalid currency code. Must be a valid ISO 4217 code",

        // Percentage
        [ErrorCodes.VO_PERCENTAGE_OUT_OF_RANGE] = "Percentage must be between 0 and 100",
        [ErrorCodes.VO_PERCENTAGE_NEGATIVE] = "Percentage cannot be negative",
        [ErrorCodes.VO_PERCENTAGE_EXCEED] = "Percentage cannot exceed 100%",

        // SerialNumber
        [ErrorCodes.VO_SERIAL_EMPTY] = "Serial number cannot be empty",
        [ErrorCodes.VO_SERIAL_INVALID] = "Serial number cannot exceed 100 characters",

        // TaxCode
        [ErrorCodes.VO_TAX_CODE_EMPTY] = "Tax code cannot be empty",
        [ErrorCodes.VO_TAX_CODE_INVALID] = "Tax code can only contain numbers and hyphens",
        [ErrorCodes.VO_TAX_CODE_TOO_SHORT] = "Tax code must be between 8 and 15 characters",
        [ErrorCodes.VO_TAX_CODE_TOO_LONG] = "Tax code must be between 8 and 15 characters",

        // MACAddress
        [ErrorCodes.VO_MAC_EMPTY] = "MAC address is required",
        [ErrorCodes.VO_MAC_INVALID] = "Invalid MAC address format",

        // DateRange
        [ErrorCodes.VO_DATE_RANGE_INVALID] = "Start date cannot be after end date",

        // Rating
        [ErrorCodes.VO_RATING_OUT_OF_RANGE] = "Rating must be between 1 and 5",

        // Dimensions
        [ErrorCodes.VO_DIMENSION_INVALID] = "Dimension value must be positive",
        [ErrorCodes.VO_DIMENSION_UNIT_EMPTY] = "Measurement unit cannot be empty",

        // DomainName
        [ErrorCodes.VO_DOMAIN_EMPTY] = "Domain cannot be empty",
        [ErrorCodes.VO_DOMAIN_TOO_LONG] = "Domain cannot exceed 253 characters",
        [ErrorCodes.VO_DOMAIN_INVALID] = "Invalid domain format",

        // URL
        [ErrorCodes.VO_URL_EMPTY] = "URL cannot be empty",
        [ErrorCodes.VO_URL_INVALID] = "Invalid URL format",

        // CountryCode
        [ErrorCodes.VO_COUNTRY_CODE_EMPTY] = "Country code cannot be empty",
        [ErrorCodes.VO_COUNTRY_CODE_INVALID] = "Country code must be exactly 2 characters (ISO 3166-1 alpha-2)",

        // ResourceType
        [ErrorCodes.VO_RESOURCE_TYPE_EMPTY] = "Resource type cannot be empty",
        [ErrorCodes.VO_RESOURCE_TYPE_TOO_LONG] = "Resource type cannot exceed 50 characters",

        // ResourceAction
        [ErrorCodes.VO_RESOURCE_ACTION_EMPTY] = "Resource action cannot be empty",
        [ErrorCodes.VO_RESOURCE_ACTION_TOO_LONG] = "Resource action cannot exceed 50 characters",

        // Warranty, Insurance, Depreciation
        [ErrorCodes.VO_WARRANTY_DURATION_INVALID] = "Warranty duration must be positive",
        [ErrorCodes.VO_DATE_INVALID] = "End date must be after start date",
        [ErrorCodes.VO_INSURANCE_POLICY_EMPTY] = "Policy number is required",
        [ErrorCodes.VO_INSURANCE_VALUE_INVALID] = "Insured value must be positive",
        [ErrorCodes.VO_DEPRECIATION_METHOD_EMPTY] = "Depreciation method is required",
        [ErrorCodes.VO_DEPRECIATION_LIFE_INVALID] = "Useful life must be positive",
        [ErrorCodes.VO_DEPRECIATION_RESIDUAL_INVALID] = "Residual value cannot be negative",

        // Rating, CostCenter, AssetTag, Username, RoleCode
        [ErrorCodes.VO_RATING_OUT_OF_RANGE] = "Rating must be between 1 and 5",
        [ErrorCodes.VO_COST_CENTER_EMPTY] = "Cost center cannot be empty",
        [ErrorCodes.VO_COST_CENTER_TOO_LONG] = "Cost center cannot exceed 30 characters",
        [ErrorCodes.VO_ASSET_TAG_EMPTY] = "Asset tag cannot be empty",
        [ErrorCodes.VO_ASSET_TAG_TOO_LONG] = "Asset tag cannot exceed 50 characters",
        [ErrorCodes.VO_USERNAME_EMPTY] = "Username cannot be empty",
        [ErrorCodes.VO_USERNAME_TOO_SHORT] = "Username must be at least 3 characters long",
        [ErrorCodes.VO_USERNAME_TOO_LONG] = "Username cannot exceed 50 characters",
        [ErrorCodes.VO_USERNAME_INVALID] = "Username can only contain letters, numbers, underscores, and hyphens",
        [ErrorCodes.VO_ROLE_CODE_EMPTY] = "Role code cannot be empty",
        [ErrorCodes.VO_ROLE_CODE_TOO_LONG] = "Role code cannot exceed 20 characters",
        [ErrorCodes.VO_ROLE_CODE_INVALID] = "Role code can only contain uppercase letters, numbers, and underscores",

        #endregion

        #region Email Template

        [ErrorCodes.EMAIL_TEMPLATE_CODE_REQUIRED] = "Template code is required",
        [ErrorCodes.EMAIL_TEMPLATE_NAME_REQUIRED] = "Template name is required",
        [ErrorCodes.EMAIL_TEMPLATE_SUBJECT_REQUIRED] = "Email subject is required",
        [ErrorCodes.EMAIL_TEMPLATE_BODY_REQUIRED] = "Email body is required",
        [ErrorCodes.EMAIL_TEMPLATE_NOT_FOUND] = "Email template not found",
        [ErrorCodes.EMAIL_TEMPLATE_CODE_EXISTS] = "An email template with this code already exists",
        [ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_UPDATE] = "System email templates cannot be modified",
        [ErrorCodes.EMAIL_TEMPLATE_SYSTEM_CANNOT_DELETE] = "System email templates cannot be deleted",

        #endregion
    };

    /// <summary>
    /// Get the default English message for an error code.
    /// If error code not found, return the code itself (for backwards compatibility with string messages).
    /// </summary>
    public static string GetMessage(string errorCode)
    {
        return Messages.TryGetValue(errorCode, out var message)
            ? message
            : errorCode; // Return the input as message (backwards compatible)
    }

    /// <summary>
    /// Get message with parameter substitution.
    /// Use {0}, {1}, etc. for placeholders.
    /// </summary>
    public static string GetMessage(string errorCode, params object[] args)
    {
        var template = GetMessage(errorCode);
        return args.Length > 0 ? string.Format(template, args) : template;
    }

    /// <summary>
    /// Get all error codes with their default English messages.
    /// Returns a dictionary where key is the error code and value is the message.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetAllErrorCodes()
    {
        return Messages;
    }
}
