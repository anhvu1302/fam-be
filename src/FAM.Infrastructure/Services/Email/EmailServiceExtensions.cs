using FAM.Application.Common.Email;
using FAM.Application.Common.Services;
using FAM.Infrastructure.Services.Email.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

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
            var envProvider = Environment.GetEnvironmentVariable("EMAIL_PROVIDER");
            if (!string.IsNullOrEmpty(envProvider))
                options.Provider = envProvider;

            var envFromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM");
            if (!string.IsNullOrEmpty(envFromEmail))
                options.FromEmail = envFromEmail;

            var envFromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME");
            if (!string.IsNullOrEmpty(envFromName))
                options.FromName = envFromName;

            // Brevo settings
            var brevoApiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");
            if (!string.IsNullOrEmpty(brevoApiKey))
                options.Brevo.ApiKey = brevoApiKey;

            // SMTP settings
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            if (!string.IsNullOrEmpty(smtpHost))
                options.Smtp.Host = smtpHost;

            var smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
            if (!string.IsNullOrEmpty(smtpPort) && int.TryParse(smtpPort, out var port))
                options.Smtp.Port = port;

            var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
            if (!string.IsNullOrEmpty(smtpUsername))
                options.Smtp.Username = smtpUsername;

            var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            if (!string.IsNullOrEmpty(smtpPassword))
                options.Smtp.Password = smtpPassword;

            var smtpEnableSsl = Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL");
            if (!string.IsNullOrEmpty(smtpEnableSsl) && bool.TryParse(smtpEnableSsl, out var enableSsl))
                options.Smtp.EnableSsl = enableSsl;
        });

        // Register Redis connection for email queue
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
            var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

            var configOptions = new ConfigurationOptions
            {
                EndPoints = { $"{redisHost}:{redisPort}" },
                AbortOnConnectFail = false,
                ConnectRetry = 3,
                ConnectTimeout = 5000
            };

            if (!string.IsNullOrEmpty(redisPassword)) configOptions.Password = redisPassword;

            var logger = sp.GetRequiredService<ILogger<RedisEmailQueue>>();

            try
            {
                var connection = ConnectionMultiplexer.Connect(configOptions);
                logger.LogInformation("Connected to Redis for email queue at {Host}:{Port}", redisHost, redisPort);
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to connect to Redis. Falling back to in-memory queue.");
                throw;
            }
        });

        // Register Redis queue as primary, with fallback to in-memory
        services.AddSingleton<IEmailQueue>(sp =>
        {
            try
            {
                var redis = sp.GetService<IConnectionMultiplexer>();
                if (redis != null && redis.IsConnected)
                {
                    var logger = sp.GetRequiredService<ILogger<RedisEmailQueue>>();
                    return new RedisEmailQueue(redis, logger);
                }
            }
            catch
            {
                // Fall through to in-memory
            }

            // Fallback to in-memory queue
            var memLogger = sp.GetRequiredService<ILogger<InMemoryEmailQueue>>();
            memLogger.LogWarning("Using in-memory email queue. Emails will be lost on restart!");
            return new InMemoryEmailQueue(memLogger);
        });

        // Register template service
        // NOTE: EmailTemplateService depends on IUnitOfWork (scoped), so it must be registered as scoped
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // Get provider from config or env
        var providerName = Environment.GetEnvironmentVariable("EMAIL_PROVIDER")
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