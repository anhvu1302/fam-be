using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FAM.Domain.Common;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Global exception handler - handles Domain exceptions and unexpected errors
/// Web API layer validation happens BEFORE this via ModelState
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
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, errors) = exception switch
        {
            DomainException domainException => HandleDomainException(domainException),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", new Dictionary<string, string[]>()),
            KeyNotFoundException keyNotFoundException => HandleNotFoundException(keyNotFoundException),
            InvalidOperationException invalidOpException => HandleInvalidOperationException(invalidOpException),
            _ => HandleGenericException(exception)
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = httpContext.Request.Path
        };

        if (errors.Any())
        {
            problemDetails.Extensions["errors"] = errors;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            cancellationToken);

        return true;
    }

    private (int statusCode, string title, Dictionary<string, string[]> errors) HandleDomainException(
        DomainException exception)
    {
        var errors = new Dictionary<string, string[]>
        {
            ["message"] = new[] { exception.Message }
        };

        return (StatusCodes.Status400BadRequest, "Domain validation error", errors);
    }

    private (int statusCode, string title, Dictionary<string, string[]> errors) HandleNotFoundException(
        KeyNotFoundException exception)
    {
        var errors = new Dictionary<string, string[]>
        {
            ["message"] = new[] { exception.Message }
        };

        return (StatusCodes.Status404NotFound, "Resource not found", errors);
    }

    private (int statusCode, string title, Dictionary<string, string[]> errors) HandleInvalidOperationException(
        InvalidOperationException exception)
    {
        var errors = new Dictionary<string, string[]>
        {
            ["message"] = new[] { exception.Message }
        };

        // InvalidOperationException is used for business rule violations (e.g., duplicate username/email)
        return (StatusCodes.Status409Conflict, "Conflict", errors);
    }

    private (int statusCode, string title, Dictionary<string, string[]> errors) HandleGenericException(
        Exception exception)
    {
        return (
            StatusCodes.Status500InternalServerError,
            "An error occurred while processing your request",
            new Dictionary<string, string[]>()
        );
    }
}
