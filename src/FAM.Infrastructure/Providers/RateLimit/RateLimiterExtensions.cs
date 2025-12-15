using FAM.Application.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.RateLimit;

/// <summary>
/// Extension methods for registering rate limiter providers
/// </summary>
public static class RateLimiterExtensions
{
    /// <summary>
    /// Add rate limiter store that uses the configured cache provider
    /// This ensures rate limiting is consistent across multiple instances
    /// </summary>
    public static IServiceCollection AddRateLimiterStore(this IServiceCollection services)
    {
        services.AddSingleton<IRateLimiterStore>(sp =>
        {
            ICacheProvider cacheProvider = sp.GetRequiredService<ICacheProvider>();
            ILogger<RateLimiterStore> logger = sp.GetRequiredService<ILogger<RateLimiterStore>>();
            return new RateLimiterStore(cacheProvider, logger);
        });

        // Add advanced rate limiter service for custom scenarios
        services.AddScoped<AdvancedRateLimiterService>();

        return services;
    }
}
