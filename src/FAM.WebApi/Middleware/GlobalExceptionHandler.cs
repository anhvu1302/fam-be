using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FAM.Domain.Common;
using Serilog.Context;

namespace FAM.WebApi.Middleware;

/// <summary>
/// API Error Response with error code for i18n support.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>Error code for frontend i18n translation</summary>
    public string Code { get; set; } = default!;
    
    /// <summary>Default English error message</summary>
    public string Message { get; set; } = default!;
    
    /// <summary>Additional error details (optional)</summary>
    public IDictionary<string, object>? Details { get; set; }
    
    /// <summary>Validation errors by field (optional)</summary>
    public IDictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Global exception handler - handles Domain exceptions and unexpected errors.
/// Returns error codes for frontend i18n support.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, errorResponse) = exception switch
        {
            ValidationException validationEx => HandleValidationException(validationEx),
            NotFoundException notFoundEx => HandleNotFoundException(notFoundEx),
            ConflictException conflictEx => HandleConflictException(conflictEx),
            UnauthorizedException unauthorizedEx => HandleUnauthorizedException(unauthorizedEx),
            ForbiddenException forbiddenEx => HandleForbiddenException(forbiddenEx),
            DomainException domainEx => HandleDomainException(domainEx),
            UnauthorizedAccessException => HandleUnauthorizedAccessException(),
            KeyNotFoundException keyNotFoundEx => HandleKeyNotFoundException(keyNotFoundEx),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),
            _ => HandleGenericException(exception)
        };

        // Log with appropriate context
        using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
        using (LogContext.PushProperty("StatusCode", statusCode))
        using (LogContext.PushProperty("ErrorCode", errorResponse.Code))
        {
            if (statusCode >= 500)
                _logger.LogError(exception,
                    "Unhandled exception occurred: {ErrorCode} - {ExceptionMessage}",
                    errorResponse.Code, exception.Message);
            else if (statusCode >= 400)
                _logger.LogWarning(
                    "Client error: {StatusCode} {ErrorCode} - {ExceptionMessage}",
                    statusCode, errorResponse.Code, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }),
            cancellationToken);

        return true;
    }

    private static (int statusCode, ApiErrorResponse response) HandleValidationException(ValidationException ex)
    {
        return (StatusCodes.Status400BadRequest, new ApiErrorResponse
        {
            Code = ex.ErrorCode,
            Message = ex.Message,
            Errors = ex.Errors
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleNotFoundException(NotFoundException ex)
    {
        return (StatusCodes.Status404NotFound, new ApiErrorResponse
        {
            Code = ex.ErrorCode,
            Message = ex.Message,
            Details = new Dictionary<string, object>
            {
                ["resourceType"] = ex.ResourceType,
                ["resourceId"] = ex.ResourceId ?? ""
            }
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleConflictException(ConflictException ex)
    {
        return (StatusCodes.Status409Conflict, new ApiErrorResponse
        {
            Code = ex.ErrorCode,
            Message = ex.Message,
            Details = new Dictionary<string, object>
            {
                ["resourceType"] = ex.ResourceType,
                ["conflictField"] = ex.ConflictField
            }
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleUnauthorizedException(UnauthorizedException ex)
    {
        return (StatusCodes.Status401Unauthorized, new ApiErrorResponse
        {
            Code = ex.ErrorCode,
            Message = ex.Message
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleForbiddenException(ForbiddenException ex)
    {
        return (StatusCodes.Status403Forbidden, new ApiErrorResponse
        {
            Code = ex.ErrorCode,
            Message = ex.Message,
            Details = ex.RequiredPermission != null 
                ? new Dictionary<string, object> { ["requiredPermission"] = ex.RequiredPermission }
                : null
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleDomainException(DomainException ex)
    {
        return (StatusCodes.Status422UnprocessableEntity, new ApiErrorResponse
        {
            Code = ex.ErrorCode,
            Message = ex.Message,
            Details = ex.Details
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleUnauthorizedAccessException()
    {
        return (StatusCodes.Status401Unauthorized, new ApiErrorResponse
        {
            Code = ErrorCodes.AUTH_UNAUTHORIZED,
            Message = ErrorMessages.GetMessage(ErrorCodes.AUTH_UNAUTHORIZED)
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleKeyNotFoundException(KeyNotFoundException ex)
    {
        return (StatusCodes.Status404NotFound, new ApiErrorResponse
        {
            Code = ErrorCodes.GEN_NOT_FOUND,
            Message = ex.Message
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleInvalidOperationException(InvalidOperationException ex)
    {
        return (StatusCodes.Status409Conflict, new ApiErrorResponse
        {
            Code = ErrorCodes.GEN_INVALID_OPERATION,
            Message = ex.Message
        });
    }

    private static (int statusCode, ApiErrorResponse response) HandleGenericException(Exception ex)
    {
        return (StatusCodes.Status500InternalServerError, new ApiErrorResponse
        {
            Code = ErrorCodes.GEN_INTERNAL_ERROR,
            Message = ErrorMessages.GetMessage(ErrorCodes.GEN_INTERNAL_ERROR)
        });
    }
}