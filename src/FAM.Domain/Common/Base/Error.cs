namespace FAM.Domain.Common.Base;

/// <summary>
/// Represents an error in the domain with error code for i18n support.
/// Immutable value object that encapsulates error information.
/// </summary>
public sealed class Error
{
    /// <summary>
    /// Error code for frontend i18n translation
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Default English error message (fallback)
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Additional error details (field names, values, etc.)
    /// </summary>
    public IDictionary<string, object>? Details { get; }

    /// <summary>
    /// Creates an error with just an error code.
    /// Message is automatically resolved from ErrorMessages.
    /// </summary>
    public Error(string code)
    {
        Code = code;
        Message = ErrorMessages.GetMessage(code);
        Details = null;
    }

    /// <summary>
    /// Creates an error with error code and additional details.
    /// Message is automatically resolved from ErrorMessages.
    /// </summary>
    public Error(string code, IDictionary<string, object> details)
    {
        Code = code;
        Message = ErrorMessages.GetMessage(code);
        Details = details;
    }

    /// <summary>
    /// Creates an error with error code, custom message, and details.
    /// Use this only when you need to override the default message.
    /// </summary>
    public Error(string code, string message, IDictionary<string, object>? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }

    /// <summary>
    /// Creates an error with error code and message parameters for formatting.
    /// </summary>
    public static Error WithParams(string code, params object[] messageParams)
    {
        return new Error(code, ErrorMessages.GetMessage(code, messageParams));
    }

    /// <summary>
    /// Predefined error: None (used for successful results)
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Implicit conversion from string (error code) to Error
    /// </summary>
    public static implicit operator Error(string code)
    {
        return new Error(code);
    }

    public override string ToString()
    {
        return $"[{Code}] {Message}";
    }
}
