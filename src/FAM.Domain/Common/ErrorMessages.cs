namespace FAM.Domain.Common;

/// <summary>
/// Default English error messages for each error code.
/// These are fallback messages when translation is not available.
/// </summary>
public static class ErrorMessages
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        #region Authentication
        
        [ErrorCodes.AUTH_INVALID_CREDENTIALS] = "Invalid username or password.",
        [ErrorCodes.AUTH_ACCOUNT_LOCKED] = "Your account has been locked. Please contact support.",
        [ErrorCodes.AUTH_ACCOUNT_INACTIVE] = "Your account is not active. Please contact support.",
        [ErrorCodes.AUTH_EMAIL_NOT_VERIFIED] = "Please verify your email address before logging in.",
        [ErrorCodes.AUTH_INVALID_TOKEN] = "The token is invalid or has expired.",
        [ErrorCodes.AUTH_INVALID_REFRESH_TOKEN] = "The refresh token is invalid or has expired. Please log in again.",
        [ErrorCodes.AUTH_2FA_REQUIRED] = "Two-factor authentication is required.",
        [ErrorCodes.AUTH_INVALID_2FA_CODE] = "The two-factor authentication code is invalid.",
        [ErrorCodes.AUTH_SESSION_EXPIRED] = "Your session has expired. Please log in again.",
        [ErrorCodes.AUTH_UNAUTHORIZED] = "You are not authorized to access this resource.",
        [ErrorCodes.AUTH_FORBIDDEN] = "You do not have permission to perform this action.",
        [ErrorCodes.AUTH_WEAK_PASSWORD] = "Password is too weak. Please use a stronger password.",
        [ErrorCodes.AUTH_INVALID_OLD_PASSWORD] = "The current password is incorrect.",

        #endregion

        #region User
        
        [ErrorCodes.USER_NOT_FOUND] = "User not found.",
        [ErrorCodes.USER_USERNAME_EXISTS] = "This username is already taken.",
        [ErrorCodes.USER_EMAIL_EXISTS] = "This email address is already registered.",
        [ErrorCodes.USER_PHONE_EXISTS] = "This phone number is already registered.",
        [ErrorCodes.USER_INVALID_USERNAME] = "Username format is invalid. Use only letters, numbers, dots, underscores, and hyphens.",
        [ErrorCodes.USER_INVALID_EMAIL] = "Email address format is invalid.",
        [ErrorCodes.USER_INVALID_PHONE] = "Phone number format is invalid.",
        [ErrorCodes.USER_ALREADY_ACTIVE] = "User is already active.",
        [ErrorCodes.USER_ALREADY_INACTIVE] = "User is already inactive.",
        [ErrorCodes.USER_CANNOT_DELETE_SELF] = "You cannot delete your own account.",

        #endregion

        #region Validation
        
        [ErrorCodes.VAL_REQUIRED] = "This field is required.",
        [ErrorCodes.VAL_TOO_SHORT] = "The value is too short.",
        [ErrorCodes.VAL_TOO_LONG] = "The value is too long.",
        [ErrorCodes.VAL_OUT_OF_RANGE] = "The value is out of allowed range.",
        [ErrorCodes.VAL_INVALID_FORMAT] = "The format is invalid.",
        [ErrorCodes.VAL_INVALID_VALUE] = "The value is invalid.",
        [ErrorCodes.VAL_DUPLICATE] = "This value already exists.",

        #endregion

        #region Signing Key
        
        [ErrorCodes.KEY_NOT_FOUND] = "Signing key not found.",
        [ErrorCodes.KEY_NO_ACTIVE_KEY] = "No active signing key is available.",
        [ErrorCodes.KEY_ALREADY_ACTIVE] = "The signing key is already active.",
        [ErrorCodes.KEY_ALREADY_INACTIVE] = "The signing key is already inactive.",
        [ErrorCodes.KEY_ALREADY_REVOKED] = "The signing key has already been revoked.",
        [ErrorCodes.KEY_EXPIRED] = "The signing key has expired.",
        [ErrorCodes.KEY_CANNOT_DELETE_ACTIVE] = "Cannot delete an active signing key.",
        [ErrorCodes.KEY_MUST_REVOKE_FIRST] = "You must revoke the key before deleting it.",
        [ErrorCodes.KEY_INVALID_ALGORITHM] = "Invalid algorithm. Must be RS256, RS384, or RS512.",
        [ErrorCodes.KEY_INVALID_SIZE] = "Invalid key size. Must be 2048, 3072, or 4096.",

        #endregion

        #region Organization
        
        [ErrorCodes.ORG_NOT_FOUND] = "Organization not found.",
        [ErrorCodes.ORG_NAME_EXISTS] = "An organization with this name already exists.",
        [ErrorCodes.ORG_CODE_EXISTS] = "An organization with this code already exists.",
        [ErrorCodes.ORG_HAS_CHILDREN] = "Cannot delete organization that has child organizations.",
        [ErrorCodes.ORG_HAS_MEMBERS] = "Cannot delete organization that has members.",
        [ErrorCodes.ORG_INVALID_PARENT] = "The specified parent organization is invalid.",
        [ErrorCodes.ORG_CIRCULAR_REFERENCE] = "This operation would create a circular reference.",

        #endregion

        #region Role & Permission
        
        [ErrorCodes.ROLE_NOT_FOUND] = "Role not found.",
        [ErrorCodes.ROLE_NAME_EXISTS] = "A role with this name already exists.",
        [ErrorCodes.ROLE_SYSTEM_PROTECTED] = "System roles cannot be modified or deleted.",
        [ErrorCodes.ROLE_HAS_USERS] = "Cannot delete role that is assigned to users.",
        [ErrorCodes.PERM_NOT_FOUND] = "Permission not found.",
        [ErrorCodes.PERM_ALREADY_ASSIGNED] = "This permission is already assigned.",

        #endregion

        #region Storage
        
        [ErrorCodes.STOR_FILE_NOT_FOUND] = "File not found.",
        [ErrorCodes.STOR_FILE_TYPE_NOT_ALLOWED] = "This file type is not allowed.",
        [ErrorCodes.STOR_FILE_TOO_LARGE] = "File size exceeds the maximum allowed limit.",
        [ErrorCodes.STOR_SESSION_NOT_FOUND] = "Upload session not found.",
        [ErrorCodes.STOR_SESSION_EXPIRED] = "Upload session has expired.",
        [ErrorCodes.STOR_QUOTA_EXCEEDED] = "Storage quota exceeded.",

        #endregion

        #region Asset
        
        [ErrorCodes.ASSET_NOT_FOUND] = "Asset not found.",
        [ErrorCodes.ASSET_CODE_EXISTS] = "An asset with this code already exists.",
        [ErrorCodes.ASSET_ALREADY_ASSIGNED] = "This asset is already assigned.",
        [ErrorCodes.ASSET_NOT_AVAILABLE] = "This asset is not available.",
        [ErrorCodes.ASSET_INVALID_STATUS] = "Invalid asset status transition.",
        [ErrorCodes.ASSET_CANNOT_DELETE] = "This asset cannot be deleted.",

        #endregion

        #region General
        
        [ErrorCodes.GEN_NOT_FOUND] = "The requested resource was not found.",
        [ErrorCodes.GEN_INVALID_OPERATION] = "This operation is not valid.",
        [ErrorCodes.GEN_CONFLICT] = "A conflict occurred. The resource may already exist.",
        [ErrorCodes.GEN_INTERNAL_ERROR] = "An internal server error occurred. Please try again later.",
        [ErrorCodes.GEN_SERVICE_UNAVAILABLE] = "Service is temporarily unavailable. Please try again later.",
        [ErrorCodes.GEN_RATE_LIMITED] = "Too many requests. Please slow down and try again.",
        [ErrorCodes.GEN_TIMEOUT] = "The request timed out. Please try again.",

        #endregion

        #region Menu
        
        [ErrorCodes.MENU_NOT_FOUND] = "Menu item not found.",
        [ErrorCodes.MENU_CODE_EXISTS] = "A menu item with this code already exists.",
        [ErrorCodes.MENU_HAS_CHILDREN] = "Cannot delete menu item that has children.",
        [ErrorCodes.MENU_INVALID_PARENT] = "The specified parent menu is invalid.",
        [ErrorCodes.MENU_MAX_DEPTH_EXCEEDED] = "Maximum menu nesting level exceeded. Maximum is 3 levels.",
        [ErrorCodes.MENU_CIRCULAR_REFERENCE] = "This operation would create a circular reference.",

        #endregion

        #region System Setting
        
        [ErrorCodes.SETTING_NOT_FOUND] = "Setting not found.",
        [ErrorCodes.SETTING_KEY_EXISTS] = "A setting with this key already exists.",
        [ErrorCodes.SETTING_NOT_EDITABLE] = "This setting is read-only and cannot be modified.",
        [ErrorCodes.SETTING_VALUE_REQUIRED] = "A value is required for this setting.",
        [ErrorCodes.SETTING_INVALID_VALUE] = "The value provided is invalid for this setting type.",

        #endregion
    };

    /// <summary>
    /// Get the default English message for an error code.
    /// </summary>
    public static string GetMessage(string errorCode)
    {
        return Messages.TryGetValue(errorCode, out var message) 
            ? message 
            : "An unexpected error occurred.";
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
}
