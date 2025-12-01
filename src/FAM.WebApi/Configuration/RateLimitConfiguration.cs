using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace FAM.WebApi.Configuration;

/// <summary>
/// Rate limiting configuration for API endpoints
/// </summary>
public static class RateLimitConfiguration
{
    // Policy names
    public const string GlobalPolicy = "GlobalRateLimit";
    public const string AuthenticationPolicy = "AuthRateLimit";
    public const string SensitivePolicy = "SensitiveRateLimit";

    /// <summary>
    /// Add rate limiting policies
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limit: 100 requests per minute per IP
            options.AddFixedWindowLimiter(GlobalPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });

            // Authentication endpoints: 10 attempts per 15 minutes per IP
            // For login, register, forgot password, etc.
            options.AddSlidingWindowLimiter(AuthenticationPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(15);
                limiterOptions.SegmentsPerWindow = 3;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 2;
            });

            // Sensitive operations: 5 attempts per 15 minutes per IP
            // For OTP verify, password reset, recovery code verification
            options.AddSlidingWindowLimiter(SensitivePolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(15);
                limiterOptions.SegmentsPerWindow = 3;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Default rejection response
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                var retryAfter = TimeSpan.Zero;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue))
                {
                    retryAfter = retryAfterValue;
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again later.",
                    retryAfterSeconds = (int)retryAfter.TotalSeconds
                }, cancellationToken: cancellationToken);
            };

            // Use IP address as partition key
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1000, // Global limit: 1000 requests per minute
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 50
                    });
            });
        });

        return services;
    }
}
