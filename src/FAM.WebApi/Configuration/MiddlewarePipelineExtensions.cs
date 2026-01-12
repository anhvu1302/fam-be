using FAM.WebApi.Middleware;

using Serilog;
using Serilog.Events;

namespace FAM.WebApi.Configuration;

/// <summary>
/// Extension methods for middleware pipeline configuration
/// </summary>
public static class MiddlewarePipelineExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        // IMPORTANT: Use forwarded headers BEFORE any other middleware
        app.UseForwardedHeaders();

        // Add custom middleware to extract and validate real client IP
        app.UseMiddleware<RealIpMiddleware>();

        // Add Correlation ID middleware (for request tracing)
        app.UseCorrelationId();

        // Add Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null || httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Debug;
                }

                return LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIp", httpContext.Items["RealClientIp"]?.ToString()
                                                  ?? httpContext.Connection.RemoteIpAddress?.ToString());

                string? userId = httpContext.User.FindFirst("sub")?.Value
                                 ?? httpContext.User.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    diagnosticContext.Set("UserId", userId);
                }
            };
        });

        // Add global exception handler
        app.UseExceptionHandler();

        // Swagger in development
        app.UseSwaggerInDevelopment();

        app.UseHttpsRedirection();
        app.UseCors();

        // Add Rate Limiting middleware (after CORS, before Authentication)
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        // Add middleware to validate deviceId for authorized requests
        app.UseMiddleware<RequireDeviceIdMiddleware>();

        app.MapControllers();

        return app;
    }
}
