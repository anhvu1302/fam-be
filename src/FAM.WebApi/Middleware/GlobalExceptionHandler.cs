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

        // Log all errors with appropriate level
        using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
        using (LogContext.PushProperty("StatusCode", statusCode))
        {
            if (statusCode >= 500)
            {
                // Log full exception details for server errors
                _logger.LogError(exception,
                    "Server error: {ExceptionType} - {ExceptionMessage}\nStack Trace: {StackTrace}",
                    exception.GetType().Name, exception.Message, exception.StackTrace);
            }
            else
            {
                _logger.LogWarning(exception,
                    "Client error: {ExceptionType} - {ExceptionMessage}",
                    exception.GetType().Name, exception.Message);
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
        var errors = new List<ApiError>();

        if (ex.Errors != null)
            foreach (var (field, messages) in ex.Errors)
            foreach (var message in messages)
            {
                var errorMessage = string.IsNullOrEmpty(field) ? message : $"{field}: {message}";
                errors.Add(new ApiError(errorMessage, ex.ErrorCode));
            }
        else
            errors.Add(new ApiError(ex.Message, ex.ErrorCode));

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