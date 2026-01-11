namespace FAM.WebApi.Configuration;

/// <summary>
/// Extension method để configure CORS từ nhiều nguồn
/// Thứ tự ưu tiên: Environment Variables > User Secrets > appsettings.json
/// </summary>
public static class CorsConfigurationExtensions
{
    public static IServiceCollection AddOptimizedCors(this IServiceCollection services, IConfiguration configuration)
    {
        CorsSettings corsSettings = LoadCorsSettings(configuration);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(corsSettings.AllowedOrigins)
                    .WithMethods(corsSettings.AllowedMethods)
                    .WithHeaders(corsSettings.AllowedHeaders)
                    .WithExposedHeaders("Content-Disposition");

                if (corsSettings.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Load CORS settings từ:
    /// 1. Environment Variables (CORS_ALLOWED_ORIGINS, CORS_ALLOW_CREDENTIALS, v.v)
    /// 2. User Secrets (development)
    /// 3. appsettings.json (default)
    /// </summary>
    private static CorsSettings LoadCorsSettings(IConfiguration configuration)
    {
        CorsSettings settings = new();

        // Từ appsettings.json
        IConfigurationSection corsSection = configuration.GetSection("Cors");
        if (corsSection.Exists())
        {
            settings.AllowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            settings.AllowedMethods = corsSection.GetSection("AllowedMethods").Get<string[]>() ?? Array.Empty<string>();
            settings.AllowedHeaders = corsSection.GetSection("AllowedHeaders").Get<string[]>() ?? Array.Empty<string>();
            settings.AllowCredentials = corsSection.GetValue("AllowCredentials", false);
        }

        // Override từ Environment Variables (Production)
        string? envOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        if (!string.IsNullOrEmpty(envOrigins))
        {
            settings.AllowedOrigins = envOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        string? envMethods = Environment.GetEnvironmentVariable("CORS_ALLOWED_METHODS");
        if (!string.IsNullOrEmpty(envMethods))
        {
            settings.AllowedMethods = envMethods.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        string? envHeaders = Environment.GetEnvironmentVariable("CORS_ALLOWED_HEADERS");
        if (!string.IsNullOrEmpty(envHeaders))
        {
            settings.AllowedHeaders = envHeaders.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        if (bool.TryParse(Environment.GetEnvironmentVariable("CORS_ALLOW_CREDENTIALS"), out bool allowCreds))
        {
            settings.AllowCredentials = allowCreds;
        }

        // Validate
        if (!settings.AllowedOrigins.Any())
        {
            settings.AllowedOrigins = new[] { "http://localhost:8001" };
        }

        if (!settings.AllowedMethods.Any())
        {
            settings.AllowedMethods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" };
        }

        if (!settings.AllowedHeaders.Any())
        {
            settings.AllowedHeaders = new[] { "Content-Type", "Authorization", "X-Requested-With" };
        }

        return settings;
    }
}

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; }
}
