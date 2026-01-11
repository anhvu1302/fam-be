using System.Text.Json;
using System.Text.Json.Serialization;

using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Diagnostics;

using Serilog.Context;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Global exception handler - handles Domain exceptions and unexpected errors.
/// Returns standardized {success, errors} or {success, result} format.
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
        (int statusCode, ApiErrorResponse errorResponse) = exception switch
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

        // Only log server errors (5xx), not client errors (4xx)
        if (statusCode >= 500)
        {
            using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
            using (LogContext.PushProperty("StatusCode", statusCode))
            {
                // Log full exception details for server errors
                _logger.LogError(exception,
                    "Server error: {ExceptionType} - {ExceptionMessage}\nStack Trace: {StackTrace}",
                    exception.GetType().Name, exception.Message, exception.StackTrace);
            }
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }),
            cancellationToken);

        return true;
    }

    private static (int statusCode, ApiErrorResponse response) HandleValidationException(ValidationException ex)
    {
        List<ApiError> errors = new();

        if (ex.Errors != null)
        {
            foreach ((string field, string[] messages) in ex.Errors)
            {
                foreach (string message in messages)
                {
                    string errorMessage = string.IsNullOrEmpty(field) ? message : $"{field}: {message}";
                    errors.Add(new ApiError(errorMessage, ex.ErrorCode));
                }
            }
        }
        else
        {
            errors.Add(new ApiError(ex.Message, ex.ErrorCode));
        }

        return (StatusCodes.Status400BadRequest, new ApiErrorResponse(false, errors));
    }

    private static (int statusCode, ApiErrorResponse response) HandleNotFoundException(NotFoundException ex)
    {
        return (StatusCodes.Status404NotFound,
            ApiErrorResponse.NotFound(ex.Message, ex.ErrorCode));
    }

    private static (int statusCode, ApiErrorResponse response) HandleConflictException(ConflictException ex)
    {
        return (StatusCodes.Status409Conflict,
            ApiErrorResponse.BadRequest(ex.Message, ex.ErrorCode));
    }

    private static (int statusCode, ApiErrorResponse response) HandleUnauthorizedException(UnauthorizedException ex)
    {
        return (StatusCodes.Status401Unauthorized,
            ApiErrorResponse.Unauthorized(ex.Message, ex.ErrorCode));
    }

    private static (int statusCode, ApiErrorResponse response) HandleForbiddenException(ForbiddenException ex)
    {
        return (StatusCodes.Status403Forbidden,
            ApiErrorResponse.Forbidden(ex.Message, ex.ErrorCode));
    }

    private static (int statusCode, ApiErrorResponse response) HandleDomainException(DomainException ex)
    {
        return (StatusCodes.Status422UnprocessableEntity,
            ApiErrorResponse.BadRequest(ex.Message, ex.ErrorCode));
    }

    private static (int statusCode, ApiErrorResponse response) HandleUnauthorizedAccessException()
    {
        return (StatusCodes.Status401Unauthorized,
            ApiErrorResponse.Unauthorized(
                ErrorMessages.GetMessage(ErrorCodes.AUTH_UNAUTHORIZED),
                ErrorCodes.AUTH_UNAUTHORIZED));
    }

    private static (int statusCode, ApiErrorResponse response) HandleKeyNotFoundException(KeyNotFoundException ex)
    {
        return (StatusCodes.Status404NotFound,
            ApiErrorResponse.NotFound(ex.Message, ErrorCodes.GEN_NOT_FOUND));
    }

    private static (int statusCode, ApiErrorResponse response) HandleInvalidOperationException(
        InvalidOperationException ex)
    {
        return (StatusCodes.Status409Conflict,
            ApiErrorResponse.BadRequest(ex.Message, ErrorCodes.GEN_INVALID_OPERATION));
    }

    private static (int statusCode, ApiErrorResponse response) HandleGenericException(Exception ex)
    {
        return (StatusCodes.Status500InternalServerError,
            ApiErrorResponse.InternalServerError(
                ErrorMessages.GetMessage(ErrorCodes.GEN_INTERNAL_ERROR),
                ErrorCodes.GEN_INTERNAL_ERROR));
    }
}
