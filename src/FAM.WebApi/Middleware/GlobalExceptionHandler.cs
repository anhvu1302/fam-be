using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FAM.Domain.Common;
using Serilog.Context;

namespace FAM.WebApi.Middleware;

/// <summary>
/// RFC 7807 Problem Details - Standardized error response
/// https://datatracker.ietf.org/doc/html/rfc7807
/// </summary>
public class ProblemDetailsResponse
{
    /// <summary>URI reference identifying the problem type</summary>
    public string Type { get; set; } = "about:blank";
    
    /// <summary>Short, human-readable summary of the problem type</summary>
    public string Title { get; set; } = default!;
    
    /// <summary>HTTP status code</summary>
    public int Status { get; set; }
    
    /// <summary>Human-readable explanation specific to this occurrence</summary>
    public string Detail { get; set; } = default!;
    
    /// <summary>URI reference identifying the specific occurrence</summary>
    public string? Instance { get; set; }
    
    /// <summary>Array of validation errors (for 400/422 responses)</summary>
    public List<ErrorDetail>? Errors { get; set; }
}

/// <summary>
/// Individual error detail with error code for i18n support
/// </summary>
public class ErrorDetail
{
    /// <summary>Error code for frontend i18n translation</summary>
    public string Code { get; set; } = default!;
    
    /// <summary>Field name that caused the error (if applicable)</summary>
    public string? Field { get; set; }
    
    /// <summary>Human-readable error message (fallback)</summary>
    public string Detail { get; set; } = default!;
    
    /// <summary>Additional contextual information (minLength, maxLength, etc.)</summary>
    public IDictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Global exception handler - handles Domain exceptions and unexpected errors.
/// Returns RFC 7807 Problem Details with error codes for frontend i18n support.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private const string ErrorTypeBaseUrl = "https://api.fam.com/errors";

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => HandleValidationException(validationEx),
            NotFoundException notFoundEx => HandleNotFoundException(notFoundEx),
            ConflictException conflictEx => HandleConflictException(conflictEx),
            UnauthorizedException unauthorizedEx => HandleUnauthorizedException(unauthorizedEx),
            ForbiddenException forbiddenEx => HandleForbiddenException(forbiddenEx),
            DomainException domainEx => HandleDomainException(domainEx, httpContext),
            UnauthorizedAccessException => HandleUnauthorizedAccessException(),
            KeyNotFoundException keyNotFoundEx => HandleKeyNotFoundException(keyNotFoundEx),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),
            _ => HandleGenericException(exception)
        };

        // Set instance to current request path
        problemDetails.Instance = httpContext.Request.Path;

        // Log with appropriate context
        using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
        using (LogContext.PushProperty("StatusCode", statusCode))
        using (LogContext.PushProperty("ErrorType", problemDetails.Type))
        {
            if (statusCode >= 500)
                _logger.LogError(exception,
                    "Unhandled exception occurred: {ErrorType} - {ExceptionMessage}",
                    problemDetails.Type, exception.Message);
            else if (statusCode >= 400)
                _logger.LogWarning(
                    "Client error: {StatusCode} {ErrorType} - {ExceptionMessage}",
                    statusCode, problemDetails.Type, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }),
            cancellationToken);

        return true;
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleValidationException(ValidationException ex)
    {
        var errors = new List<ErrorDetail>();
        
        if (ex.Errors != null)
        {
            foreach (var (field, messages) in ex.Errors)
            {
                foreach (var message in messages)
                {
                    errors.Add(new ErrorDetail
                    {
                        Code = ex.ErrorCode,
                        Field = field,
                        Detail = message
                    });
                }
            }
        }

        return (StatusCodes.Status400BadRequest, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/validation",
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Errors = errors.Any() ? errors : null
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleNotFoundException(NotFoundException ex)
    {
        return (StatusCodes.Status404NotFound, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/not-found",
            Title = "Resource Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = ex.Message,
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ex.ErrorCode,
                    Detail = ex.Message,
                    Details = new Dictionary<string, object>
                    {
                        ["resourceType"] = ex.ResourceType,
                        ["resourceId"] = ex.ResourceId ?? ""
                    }
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleConflictException(ConflictException ex)
    {
        return (StatusCodes.Status409Conflict, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/conflict",
            Title = "Resource Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ex.ErrorCode,
                    Field = ex.ConflictField,
                    Detail = ex.Message,
                    Details = new Dictionary<string, object>
                    {
                        ["resourceType"] = ex.ResourceType,
                        ["conflictField"] = ex.ConflictField
                    }
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleUnauthorizedException(UnauthorizedException ex)
    {
        return (StatusCodes.Status401Unauthorized, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/unauthorized",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = ex.Message,
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ex.ErrorCode,
                    Detail = ex.Message
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleForbiddenException(ForbiddenException ex)
    {
        return (StatusCodes.Status403Forbidden, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/forbidden",
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = ex.Message,
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ex.ErrorCode,
                    Detail = ex.Message,
                    Details = ex.RequiredPermission != null 
                        ? new Dictionary<string, object> { ["requiredPermission"] = ex.RequiredPermission }
                        : null
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleDomainException(
        DomainException ex, 
        HttpContext httpContext)
    {
        // Extract field name from details if available
        string? fieldName = null;
        if (ex.Details?.TryGetValue("field", out var field) == true)
        {
            fieldName = field?.ToString();
        }

        return (StatusCodes.Status422UnprocessableEntity, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/domain-validation",
            Title = "Domain Validation Error",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = ex.Message,
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ex.ErrorCode,
                    Field = fieldName,
                    Detail = ex.Message,
                    Details = ex.Details
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleUnauthorizedAccessException()
    {
        return (StatusCodes.Status401Unauthorized, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/unauthorized",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = ErrorMessages.GetMessage(ErrorCodes.AUTH_UNAUTHORIZED),
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ErrorCodes.AUTH_UNAUTHORIZED,
                    Detail = ErrorMessages.GetMessage(ErrorCodes.AUTH_UNAUTHORIZED)
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleKeyNotFoundException(KeyNotFoundException ex)
    {
        return (StatusCodes.Status404NotFound, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/not-found",
            Title = "Resource Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = ex.Message,
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ErrorCodes.GEN_NOT_FOUND,
                    Detail = ex.Message
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleInvalidOperationException(InvalidOperationException ex)
    {
        return (StatusCodes.Status409Conflict, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/invalid-operation",
            Title = "Invalid Operation",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ErrorCodes.GEN_INVALID_OPERATION,
                    Detail = ex.Message
                }
            }
        });
    }

    private static (int statusCode, ProblemDetailsResponse response) HandleGenericException(Exception ex)
    {
        return (StatusCodes.Status500InternalServerError, new ProblemDetailsResponse
        {
            Type = $"{ErrorTypeBaseUrl}/internal-error",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = ErrorMessages.GetMessage(ErrorCodes.GEN_INTERNAL_ERROR),
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Code = ErrorCodes.GEN_INTERNAL_ERROR,
                    Detail = ErrorMessages.GetMessage(ErrorCodes.GEN_INTERNAL_ERROR)
                }
            }
        });
    }
}