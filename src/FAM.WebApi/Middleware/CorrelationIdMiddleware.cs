using Serilog.Context;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Middleware để tạo và quản lý Correlation ID cho mỗi request.
/// Correlation ID giúp trace toàn bộ log của một request xuyên suốt các tầng.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Lấy Correlation ID từ header hoặc tạo mới
        string correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
                               ?? Guid.NewGuid().ToString("N");

        // Thêm Correlation ID vào Response header để client có thể trace
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Lưu vào HttpContext.Items để các service khác có thể truy cập
        context.Items["CorrelationId"] = correlationId;

        // Push vào Serilog LogContext - tất cả log trong scope này sẽ có CorrelationId
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Extension method để đăng ký middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
