using System.Threading.RateLimiting;

using FAM.Application.Abstractions;
using FAM.Infrastructure.Providers.RateLimit;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace FAM.WebApi.Configuration;

/// <summary>
/// Rate limiting configuration for API endpoints using Redis for distributed rate limiting
/// </summary>
public static class RateLimitConfiguration
{
    // Policy names
    public const string GlobalPolicy = "GlobalRateLimit";
    public const string AuthenticationPolicy = "AuthRateLimit";
    public const string SensitivePolicy = "SensitiveRateLimit";

    /// <summary>
    /// Add Redis-backed distributed rate limiting policies
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limit: 100 requests per minute per IP (Redis-backed)
            options.AddPolicy<string>(GlobalPolicy, context =>
            {
                var serviceProvider = context.RequestServices;
                var store = serviceProvider.GetRequiredService<IRateLimiterStore>();
                var logger = serviceProvider.GetRequiredService<ILogger<RedisRateLimiter>>();
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var partitionKey = $"ratelimit:global:{ipAddress}";

                return RateLimitPartition.Get(partitionKey, _ => 
                    new RedisRateLimiter(store, partitionKey, 100, TimeSpan.FromMinutes(1), 10, logger));
            });

            // Authentication endpoints: 10 attempts per 15 minutes per IP (Redis-backed)
            // For login, register, forgot password, etc.
            options.AddPolicy<string>(AuthenticationPolicy, context =>
            {
                var serviceProvider = context.RequestServices;
                var store = serviceProvider.GetRequiredService<IRateLimiterStore>();
                var logger = serviceProvider.GetRequiredService<ILogger<RedisRateLimiter>>();
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var partitionKey = $"ratelimit:auth:{ipAddress}";

                return RateLimitPartition.Get(partitionKey, _ => 
                    new RedisRateLimiter(store, partitionKey, 10, TimeSpan.FromMinutes(15), 2, logger));
            });

            // Sensitive operations: 5 attempts per 15 minutes per IP (Redis-backed)
            // For OTP verify, password reset, recovery code verification
            options.AddPolicy<string>(SensitivePolicy, context =>
            {
                var serviceProvider = context.RequestServices;
                var store = serviceProvider.GetRequiredService<IRateLimiterStore>();
                var logger = serviceProvider.GetRequiredService<ILogger<RedisRateLimiter>>();
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var partitionKey = $"ratelimit:sensitive:{ipAddress}";

                return RateLimitPartition.Get(partitionKey, _ => 
                    new RedisRateLimiter(store, partitionKey, 5, TimeSpan.FromMinutes(15), 0, logger));
            });

            // Default rejection response
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                TimeSpan retryAfter = TimeSpan.Zero;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue))
                {
                    retryAfter = retryAfterValue;
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Too many requests. Please try again later.",
                    errors = new[]
                    {
                        new
                        {
                            message = "Rate limit exceeded",
                            code = "RATE_LIMIT_EXCEEDED",
                            retryAfterSeconds = (int)retryAfter.TotalSeconds
                        }
                    }
                }, cancellationToken);
            };

            // Global limiter using Redis - applies to all endpoints without specific policy
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var serviceProvider = context.RequestServices;
                var store = serviceProvider.GetRequiredService<IRateLimiterStore>();
                var logger = serviceProvider.GetRequiredService<ILogger<RedisRateLimiter>>();
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var partitionKey = $"ratelimit:global-limiter:{ipAddress}";

                return RateLimitPartition.Get(partitionKey, _ => 
                    new RedisRateLimiter(store, partitionKey, 1000, TimeSpan.FromMinutes(1), 50, logger));
            });
        });

        return services;
    }
}
