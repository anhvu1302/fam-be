namespace FAM.Domain.Common;

/// <summary>
/// Domain Exception - represents business rule violations.
/// Contains error code for i18n support on frontend.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Error code for frontend internationalization (i18n).
    /// Frontend can use this code to display localized error messages.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Additional data related to the error (e.g., field name, invalid value).
    /// </summary>
    public IDictionary<string, object>? Details { get; }

    /// <summary>
    /// Create a domain exception with error code.
    /// Message is automatically resolved from ErrorMessages.
    /// </summary>
    public DomainException(string errorCode)
        : base(ErrorMessages.GetMessage(errorCode))
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Create a domain exception with error code and custom message.
    /// </summary>
    public DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Create a domain exception with error code and message parameters.
    /// </summary>
    public DomainException(string errorCode, params object[] messageArgs)
        : base(ErrorMessages.GetMessage(errorCode, messageArgs))
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Create a domain exception with error code and details for i18n.
    /// Details will be sent to frontend for parameter substitution.
    /// Example: new DomainException(ErrorCodes.VAL_TOO_SHORT, new { field = "Username", minLength = 3 })
    /// Frontend: i18n.t("VAL_TOO_SHORT", { field: "Username", minLength: 3 })
    /// </summary>
    public DomainException(string errorCode, object details)
        : base(ErrorMessages.GetMessage(errorCode))
    {
        ErrorCode = errorCode;
        Details = ConvertToDictionary(details);
    }

    /// <summary>
    /// Create a domain exception with error code, message, and details.
    /// </summary>
    public DomainException(string errorCode, string message, IDictionary<string, object> details)
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    /// <summary>
    /// Create a domain exception with error code and inner exception.
    /// </summary>
    public DomainException(string errorCode, Exception innerException)
        : base(ErrorMessages.GetMessage(errorCode), innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Create a domain exception with error code, custom message, and inner exception.
    /// </summary>
    public DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Convert anonymous object to dictionary for details.
    /// </summary>
    private static IDictionary<string, object> ConvertToDictionary(object obj)
    {
        if (obj is IDictionary<string, object> dict)
            return dict;

        var dictionary = new Dictionary<string, object>();
        foreach (var prop in obj.GetType().GetProperties())
        {
            var value = prop.GetValue(obj);
            if (value != null)
                dictionary[ToCamelCase(prop.Name)] = value;
        }
        return dictionary;
    }

    /// <summary>
    /// Convert property name to camelCase for JSON compatibility.
    /// </summary>
    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;
        return char.ToLower(str[0]) + str.Substring(1);
    }
}

/// <summary>
/// Not found exception - resource not found.
/// </summary>
public class NotFoundException : DomainException
{
    public string ResourceType { get; }
    public object? ResourceId { get; }

    public NotFoundException(string resourceType, object? resourceId = null)
        : base(ErrorCodes.GEN_NOT_FOUND, $"{resourceType} not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public NotFoundException(string errorCode, string resourceType, object? resourceId = null)
        : base(errorCode)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Conflict exception - resource already exists.
/// </summary>
public class ConflictException : DomainException
{
    public string ResourceType { get; }
    public string ConflictField { get; }

    public ConflictException(string resourceType, string conflictField)
        : base(ErrorCodes.GEN_CONFLICT, $"{resourceType} with this {conflictField} already exists.")
    {
        ResourceType = resourceType;
        ConflictField = conflictField;
    }

    public ConflictException(string errorCode, string resourceType, string conflictField)
        : base(errorCode)
    {
        ResourceType = resourceType;
        ConflictField = conflictField;
    }
}

/// <summary>
/// Validation exception - input validation failed.
/// </summary>
public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string field, string errorCode)
        : base(errorCode)
    {
        Errors = new Dictionary<string, string[]>
        {
            [field] = new[] { ErrorMessages.GetMessage(errorCode) }
        };
    }

    public ValidationException(string field, string errorCode, string message)
        : base(errorCode, message)
    {
        Errors = new Dictionary<string, string[]>
        {
            [field] = new[] { message }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base(ErrorCodes.VAL_INVALID_VALUE, "One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

/// <summary>
/// Unauthorized exception - authentication required or failed.
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException()
        : base(ErrorCodes.AUTH_UNAUTHORIZED)
    {
    }

    public UnauthorizedException(string errorCode)
        : base(errorCode)
    {
    }

    public UnauthorizedException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}

/// <summary>
/// Forbidden exception - access denied due to insufficient permissions.
/// </summary>
public class ForbiddenException : DomainException
{
    public string? RequiredPermission { get; }

    public ForbiddenException()
        : base(ErrorCodes.AUTH_FORBIDDEN)
    {
    }

    public ForbiddenException(string requiredPermission)
        : base(ErrorCodes.AUTH_FORBIDDEN, $"Permission '{requiredPermission}' is required.")
    {
        RequiredPermission = requiredPermission;
    }
}