using FAM.WebApi.Attributes;
using FAM.WebApi.Contracts.Common;
using System.Text.Json;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Middleware to validate that requests marked with [RequireDeviceId] have deviceId
/// This ensures API requests from authenticated users are tied to a specific device
/// </summary>
public class RequireDeviceIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequireDeviceIdMiddleware> _logger;

    public RequireDeviceIdMiddleware(RequestDelegate next, ILogger<RequireDeviceIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for public endpoints (login, refresh, etc.)
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        // Get endpoint metadata to check if [RequireDeviceId] is applied
        var endpoint = context.GetEndpoint();
        var requiresDeviceId = endpoint?.Metadata.GetMetadata<RequireDeviceIdAttribute>() != null;

        // If endpoint doesn't require device_id, skip validation
        if (!requiresDeviceId)
        {
            await _next(context);
            return;
        }

        // Check if request has Authorization header (authenticated request)
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            // Check for deviceId in cookie or header
            var deviceId = context.Request.Cookies["device_id"];
            
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = context.Request.Headers["X-Device-Id"].ToString();
            }

            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogWarning("Authorized request missing device_id. Path: {Path}, User: {User}",
                    path, context.User?.FindFirst("user_id")?.Value ?? "Unknown");

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";

                var errorResponse = new ApiErrorResponse(
                    Success: false,
                    Errors: new List<ApiError>
                    {
                        new ApiError(
                            Message: "Device ID is required. Please include 'device_id' in cookies or 'X-Device-Id' header.",
                            Code: "DEVICE_ID_REQUIRED"
                        )
                    }
                );

                var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(json);
                return;
            }
        }

        await _next(context);
    }

    private static bool IsPublicEndpoint(string path)
    {
        // List of endpoints that don't require any device validation
        var publicPaths = new[]
        {
            "/api/auth/login",
            "/api/auth/verify-2fa",
            "/api/auth/verify-email-otp",
            "/api/auth/refresh",
            "/api/auth/forgot-password",
            "/api/auth/verify-reset-token",
            "/api/auth/reset-password",
            "/api/auth/select-authentication-method",
            "/api/auth/verify-recovery-code",
            "/swagger",
            "/health",
        };

        return publicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }
}
