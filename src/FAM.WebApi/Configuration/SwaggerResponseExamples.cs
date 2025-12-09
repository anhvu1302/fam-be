namespace FAM.WebApi.Configuration;

/// <summary>
/// Swagger examples for standardized API responses
/// </summary>
public static class SwaggerResponseExamples
{
    /// <summary>
    /// Example for successful response with data
    /// </summary>
    public const string SuccessWithDataExample = @"{
  ""success"": true,
  ""message"": ""Operation completed successfully"",
  ""result"": {
    ""id"": 1,
    ""name"": ""Example""
  }
}";

    /// <summary>
    /// Example for successful response without data
    /// </summary>
    public const string SuccessNoDataExample = @"{
  ""success"": true,
  ""message"": ""Operation completed successfully""
}";

    /// <summary>
    /// Example for error response
    /// </summary>
    public const string ErrorResponseExample = @"{
  ""success"": false,
  ""errors"": [
    {
      ""message"": ""Validation failed"",
      ""code"": ""VALIDATION_ERROR""
    }
  ]
}";

    /// <summary>
    /// Example for validation error response with multiple errors
    /// </summary>
    public const string ValidationErrorExample = @"{
  ""success"": false,
  ""errors"": [
    {
      ""message"": ""Email is required"",
      ""code"": ""REQUIRED_FIELD""
    },
    {
      ""message"": ""Password must be at least 8 characters"",
      ""code"": ""INVALID_LENGTH""
    }
  ]
}";

    // ============ Standard Response Documentation Snippets ============

    /// <summary>
    /// 200 OK - Success response with data
    /// </summary>
    public const string Response200Success =
        @"/// <response code=""200"">Success - Returns {success: true, message?: string, result: T}</response>";

    /// <summary>
    /// 201 Created - Success response for resource creation
    /// </summary>
    public const string Response201Created =
        @"/// <response code=""201"">Created - Returns {success: true, message?: string, result: T}</response>";

    /// <summary>
    /// 204 No Content - Success with no response body
    /// </summary>
    public const string Response204NoContent =
        @"/// <response code=""204"">No Content - Returns empty response</response>";

    /// <summary>
    /// 400 Bad Request - Standard validation error
    /// </summary>
    public const string Response400BadRequest =
        @"/// <response code=""400"">Bad Request - Returns {success: false, errors: [{message: string, code: string}]}</response>";

    /// <summary>
    /// 400 File Required
    /// </summary>
    public const string Response400FileRequired =
        @"/// <response code=""400"">Bad Request - Returns {success: false, errors: [{message: ""File is required"", code: ""FILE_REQUIRED""}]}</response>";

    /// <summary>
    /// 400 Validation Failed
    /// </summary>
    public const string Response400ValidationFailed =
        @"/// <response code=""400"">Bad Request - Returns {success: false, errors: [{message: ""Validation failed"", code: ""VALIDATION_FAILED""}]}</response>";

    /// <summary>
    /// 400 Invalid Parameters
    /// </summary>
    public const string Response400InvalidParameters =
        @"/// <response code=""400"">Bad Request - Returns {success: false, errors: [{message: ""Invalid query parameters"", code: ""INVALID_PARAMETERS""}]}</response>";

    /// <summary>
    /// 401 Unauthorized - User not authenticated
    /// </summary>
    public const string Response401Unauthorized =
        @"/// <response code=""401"">Unauthorized - Returns {success: false, errors: [{message: ""User not authenticated"", code: ""UNAUTHORIZED""}]}</response>";

    /// <summary>
    /// 403 Forbidden - User not authorized
    /// </summary>
    public const string Response403Forbidden =
        @"/// <response code=""403"">Forbidden - Returns {success: false, errors: [{message: ""User not authorized"", code: ""FORBIDDEN""}]}</response>";

    /// <summary>
    /// 404 Not Found - Resource not found
    /// </summary>
    public const string Response404NotFound =
        @"/// <response code=""404"">Not Found - Returns {success: false, errors: [{message: ""Resource not found"", code: ""NOT_FOUND""}]}</response>";

    /// <summary>
    /// 409 Conflict - Resource already exists
    /// </summary>
    public const string Response409Conflict =
        @"/// <response code=""409"">Conflict - Returns {success: false, errors: [{message: ""Resource already exists"", code: ""ALREADY_EXISTS""}]}</response>";

    /// <summary>
    /// 500 Internal Server Error
    /// </summary>
    public const string Response500InternalError =
        @"/// <response code=""500"">Internal Server Error - Returns {success: false, errors: [{message: ""Internal server error"", code: ""INTERNAL_ERROR""}]}</response>";
}