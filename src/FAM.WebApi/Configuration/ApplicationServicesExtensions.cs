using FAM.Application.Querying.Parsing;
using FAM.Application.Storage;
using FAM.Application.Users.Commands.CreateUser;
using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Services;
using FAM.Domain.Abstractions.Storage;
using FAM.Infrastructure.Auth;
using FAM.Infrastructure.Services;
using FAM.WebApi.Services;

namespace FAM.WebApi.Configuration;

/// <summary>
/// Extension methods for application services registration
/// </summary>
public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR (no validation pipeline - validation is at Web API layer)
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly); });

        // Register Filter Parser (singleton - stateless)
        services.AddSingleton<IFilterParser, PrattFilterParser>();

        // Register JWT Service
        services.AddScoped<IJwtService, JwtService>();

        // Register 2FA Session Service (uses configurable cache provider - Redis or In-Memory)
        services.AddScoped<ITwoFactorSessionService, TwoFactorSessionService>();

        // Register OTP and Token services
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

        // Register File Validator for storage operations
        services.AddScoped<IFileValidator, FileValidator>();

        // Register Connection Validator (for startup validation)
        services.AddSingleton<IConnectionValidator, ConnectionValidator>();

        return services;
    }
}
