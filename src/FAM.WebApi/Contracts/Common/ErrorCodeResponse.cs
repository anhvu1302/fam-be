namespace FAM.WebApi.Contracts.Common;

/// <summary>
/// Response model for error codes.
/// Contains error code and its default English message.
/// </summary>
public sealed record ErrorCodeResponse
{
    /// <summary>
    /// The error code identifier (e.g., "AUTH_INVALID_CREDENTIALS")
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The default English error message
    /// </summary>
    public required string Message { get; init; }
}

/// <summary>
/// Response model for all error codes.
/// </summary>
public sealed record ErrorCodesListResponse
{
    /// <summary>
    /// List of all error codes with their default messages
    /// </summary>
    public required IReadOnlyList<ErrorCodeResponse> ErrorCodes { get; init; }

    /// <summary>
    /// Total count of error codes
    /// </summary>
    public required int TotalCount { get; init; }
}
