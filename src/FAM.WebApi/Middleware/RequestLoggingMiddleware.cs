using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Serilog.Context;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Middleware để log chi tiết Request/Response.
/// Bao gồm: URL, Method, Headers, Body (với sensitive data masking), Response time.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Các field nhạy cảm cần mask
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "newPassword", "currentPassword", "confirmPassword",
        "token", "accessToken", "refreshToken",
        "secret", "apiKey", "apiSecret",
        "creditCard", "cardNumber", "cvv", "pin",
        "ssn", "socialSecurityNumber",
        "authorization"
    };

    // Các endpoint không log body (ví dụ: upload file)
    private static readonly HashSet<string> ExcludeBodyPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/storage/upload",
        "/api/storage/multipart"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        HttpRequest request = context.Request;

        // Lấy thông tin cơ bản
        var method = request.Method;
        var path = request.Path.Value ?? "/";
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : null;
        var userAgent = request.Headers.UserAgent.ToString();
        var clientIp = context.Items["RealClientIp"]?.ToString() ?? context.Connection.RemoteIpAddress?.ToString();
        var userId = context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("userId")?.Value;

        // Push thêm context vào Serilog
        using (LogContext.PushProperty("ClientIp", clientIp))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserAgent", userAgent))
        using (LogContext.PushProperty("RequestMethod", method))
        using (LogContext.PushProperty("RequestPath", path))
        {
            // Log request body (nếu không phải upload và là POST/PUT/PATCH)
            string? requestBody = null;
            if (ShouldLogRequestBody(request)) requestBody = await ReadAndMaskRequestBodyAsync(request);

            _logger.LogInformation(
                "HTTP {Method} {Path}{QueryString} started - IP: {ClientIp}, User: {UserId}",
                method, path, queryString, clientIp, userId ?? "anonymous");

            if (!string.IsNullOrEmpty(requestBody)) _logger.LogDebug("Request Body: {RequestBody}", requestBody);

            // Capture response
            Stream originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // Log response
                using (LogContext.PushProperty("StatusCode", statusCode))
                using (LogContext.PushProperty("ElapsedMs", elapsedMs))
                {
                    LogLevel logLevel = statusCode >= 500 ? LogLevel.Error
                        : statusCode >= 400 ? LogLevel.Warning
                        : LogLevel.Information;

                    _logger.Log(
                        logLevel,
                        "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                        method, path, statusCode, elapsedMs);

                    // Log response body cho errors (debug purpose)
                    if (statusCode >= 400 && responseBody.Length > 0 && responseBody.Length < 4096)
                    {
                        responseBody.Seek(0, SeekOrigin.Begin);
                        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                        _logger.LogDebug("Response Body: {ResponseBody}", responseText);
                    }
                }

                // Copy response body back
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        // Không log body cho các request không có body
        if (request.Method is "GET" or "DELETE" or "HEAD" or "OPTIONS")
            return false;

        // Không log body cho upload files
        if (ExcludeBodyPaths.Any(p => request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Không log body quá lớn (> 10KB)
        if (request.ContentLength > 10 * 1024)
            return false;

        return true;
    }

    private async Task<string?> ReadAndMaskRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
                return null;

            return MaskSensitiveData(body);
        }
        catch
        {
            return null;
        }
    }

    private static string MaskSensitiveData(string json)
    {
        // Simple regex-based masking cho các field nhạy cảm
        foreach (var field in SensitiveFields)
        {
            // Match: "fieldName": "value" hoặc "fieldName":"value"
            var pattern = $@"(""{field}""\s*:\s*"")[^""]*("")";
            json = Regex.Replace(
                json,
                pattern,
                "$1***MASKED***$2",
                RegexOptions.IgnoreCase);
        }

        return json;
    }
}

/// <summary>
/// Extension method để đăng ký middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
