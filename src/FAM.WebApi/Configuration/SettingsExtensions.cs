using FAM.Application.Common.Options;
using FAM.Application.Settings;

using Microsoft.Extensions.Options;

namespace FAM.WebApi.Configuration;

/// <summary>
/// Extension methods for application settings configuration
/// </summary>
public static class SettingsExtensions
{
    public static IServiceCollection AddApplicationSettings(
        this IServiceCollection services,
        IConfiguration configuration,
        AppConfiguration appConfig)
    {
        // ===== Settings from appsettings.json (non-sensitive) =====

        // Pagination settings
        services.Configure<PaginationSettings>(
            configuration.GetSection(PaginationSettings.SectionName));
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<PaginationSettings>>().Value);

        // File Upload settings
        services.Configure<FileUploadSettings>(
            configuration.GetSection(FileUploadSettings.SectionName));

        // Real IP Detection settings
        services.Configure<RealIpDetectionSettings>(
            configuration.GetSection(RealIpDetectionSettings.SectionName));

        // Backend settings (API URL configuration)
        services.Configure<BackendOptions>(
            configuration.GetSection(BackendOptions.SectionName));
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<BackendOptions>>().Value);

        // Frontend settings (URL configuration) with environment variable overrides
        services.Configure<FrontendOptions>(options =>
        {
            configuration.GetSection(FrontendOptions.SectionName).Bind(options);
            ConfigureFrontendFromEnvironment(options);
        });
        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<FrontendOptions>>().Value);

        // Legacy paging options (backward compatibility)
        IConfigurationSection paginationSection = configuration.GetSection(PaginationSettings.SectionName);
        services.Configure<PagingOptions>(options =>
        {
            options.MaxPageSize = paginationSection.GetValue<int>("MaxPageSize", 100);
        });

        // ===== Settings from .env (sensitive - via AppConfiguration) =====

        // MinIO settings from environment variables
        services.Configure<MinioSettings>(options =>
        {
            options.Endpoint = appConfig.MinioEndpoint;
            options.AccessKey = appConfig.MinioAccessKey;
            options.SecretKey = appConfig.MinioSecretKey;
            options.UseSsl = appConfig.MinioUseSsl;
            options.BucketName = appConfig.MinioBucketName;
        });

        return services;
    }

    private static void ConfigureFrontendFromEnvironment(FrontendOptions options)
    {
        string? frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            if (Uri.TryCreate(frontendUrl, UriKind.Absolute, out Uri? uri))
            {
                options.BaseUrl = $"{uri.Scheme}://{uri.Host}";
                if (uri.Port != 80 && uri.Port != 443)
                {
                    options.Port = uri.Port;
                }
                else
                {
                    options.Port = null;
                }
            }
            else
            {
                options.BaseUrl = frontendUrl;
                options.Port = null;
            }
        }

        string? frontendPort = Environment.GetEnvironmentVariable("FRONTEND_PORT");
        if (!string.IsNullOrEmpty(frontendPort) && int.TryParse(frontendPort, out int port))
        {
            options.Port = port;
        }
    }
}
