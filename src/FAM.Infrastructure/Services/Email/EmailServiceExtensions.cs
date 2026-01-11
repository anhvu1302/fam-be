using FAM.Application.Abstractions;
using FAM.Application.Common.Email;
using FAM.Application.Common.Services;
using FAM.Infrastructure.Providers.Cache;
using FAM.Infrastructure.Services.Email.Providers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services.Email;

/// <summary>
/// Extension methods for registering email services
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Add email services with queue and provider
    /// Configuration is read from appsettings.json "Email" section
    /// Can be overridden by environment variables:
    /// - EMAIL_PROVIDER: "Brevo" or "Smtp"
    /// - EMAIL_FROM: sender email
    /// - EMAIL_FROM_NAME: sender name
    /// - BREVO_API_KEY: Brevo API key
    /// - SMTP_HOST, SMTP_PORT, SMTP_USERNAME, SMTP_PASSWORD, SMTP_ENABLE_SSL
    /// </summary>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration with environment variable overrides
        services.Configure<EmailProviderOptions>(options =>
        {
            // Bind from appsettings first
            configuration.GetSection(EmailProviderOptions.SectionName).Bind(options);

            // Override with environment variables if present
            string? envProvider = Environment.GetEnvironmentVariable("EMAIL_PROVIDER");
            if (!string.IsNullOrEmpty(envProvider))
            {
                options.Provider = envProvider;
            }

            string? envFromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM");
            if (!string.IsNullOrEmpty(envFromEmail))
            {
                options.FromEmail = envFromEmail;
            }

            string? envFromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME");
            if (!string.IsNullOrEmpty(envFromName))
            {
                options.FromName = envFromName;
            }

            // SMTP settings
            string? smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            if (!string.IsNullOrEmpty(smtpHost))
            {
                options.Smtp.Host = smtpHost;
            }

            string? smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
            if (!string.IsNullOrEmpty(smtpPort) && int.TryParse(smtpPort, out int port))
            {
                options.Smtp.Port = port;
            }

            string? smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
            if (!string.IsNullOrEmpty(smtpUsername))
            {
                options.Smtp.Username = smtpUsername;
            }

            string? smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            if (!string.IsNullOrEmpty(smtpPassword))
            {
                options.Smtp.Password = smtpPassword;
            }

            string? smtpEnableSsl = Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL");
            if (!string.IsNullOrEmpty(smtpEnableSsl) && bool.TryParse(smtpEnableSsl, out bool enableSsl))
            {
                options.Smtp.EnableSsl = enableSsl;
            }
        });

        // Register cache provider for email queue
        services.AddCacheProvider(configuration);

        // Register email queue with cache provider (try cache-backed first, fallback to in-memory)
        services.AddSingleton<IEmailQueue>(sp =>
        {
            try
            {
                ICacheProvider cache = sp.GetRequiredService<ICacheProvider>();
                ILogger<CacheEmailQueue> logger = sp.GetRequiredService<ILogger<CacheEmailQueue>>();
                return new CacheEmailQueue(cache, logger);
            }
            catch (Exception ex)
            {
                // Fallback to in-memory queue
                ILogger<InMemoryEmailQueue> memLogger = sp.GetRequiredService<ILogger<InMemoryEmailQueue>>();
                memLogger.LogWarning(ex, "Failed to initialize cache email queue. Using in-memory email queue.");
                return new InMemoryEmailQueue(memLogger);
            }
        });

        // Register template service
        // NOTE: EmailTemplateService depends on IUnitOfWork (scoped), so it must be registered as scoped
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // Get provider from config or env
        string providerName = Environment.GetEnvironmentVariable("EMAIL_PROVIDER")
                              ?? configuration.GetValue<string>($"{EmailProviderOptions.SectionName}:Provider")
                              ?? "Smtp";

        switch (providerName.ToLowerInvariant())
        {
            case "brevo":
            case "sendinblue":
                services.AddBrevoProvider();
                break;

            case "smtp":
            default:
                services.AddSmtpProvider();
                break;
        }

        // Register queued email service (implements IEmailService)
        services.AddScoped<IEmailService, QueuedEmailService>();

        // Register background processor
        services.AddHostedService<EmailQueueProcessor>();

        return services;
    }

    /// <summary>
    /// Add Brevo email provider
    /// </summary>
    private static IServiceCollection AddBrevoProvider(this IServiceCollection services)
    {
        services.AddHttpClient<IEmailProvider, BrevoEmailProvider>()
            .ConfigureHttpClient((sp, client) =>
            {
                // Additional HttpClient configuration if needed
                client.DefaultRequestHeaders.Add("User-Agent", "FAM-System/1.0");
            });

        return services;
    }

    /// <summary>
    /// Add SMTP email provider
    /// </summary>
    private static IServiceCollection AddSmtpProvider(this IServiceCollection services)
    {
        services.AddSingleton<IEmailProvider, SmtpEmailProvider>();
        return services;
    }
}
