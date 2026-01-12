using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FAM.WebApi.Configuration;

/// <summary>
/// Extension methods for JWT Authentication configuration
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        AppConfiguration appConfig)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = appConfig.Environment == "Production";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = appConfig.JwtIssuer,
                    ValidAudience = appConfig.JwtAudience,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = OnMessageReceivedAsync,
                    OnAuthenticationFailed = OnAuthenticationFailedAsync,
                    OnTokenValidated = OnTokenValidatedAsync
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static async Task OnMessageReceivedAsync(MessageReceivedContext context)
    {
        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

        try
        {
            string token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            ISigningKeyRepository repository = context.HttpContext.RequestServices
                .GetRequiredService<ISigningKeyRepository>();

            IEnumerable<SigningKey> signingKeys =
                await repository.GetAllAsync(context.HttpContext.RequestAborted);
            List<SigningKey> activeKeys = signingKeys.Where(k => k.IsActive && !k.IsRevoked && !k.IsExpired())
                .ToList();

            List<SecurityKey> keys = new();
            foreach (SigningKey key in activeKeys)
            {
                if (key.Algorithm == "RSA")
                {
                    RSA rsa = RSA.Create();
                    rsa.ImportParameters(new RSAParameters
                    {
                        Modulus = Base64UrlEncoder.DecodeBytes(key.KeyId),
                        Exponent = Base64UrlEncoder.DecodeBytes(key.KeyId)
                    });
                    keys.Add(new RsaSecurityKey(rsa) { KeyId = key.KeyId });
                }
            }

            context.Options.TokenValidationParameters.IssuerSigningKeys = keys;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading signing keys from database");
        }
    }

    private static Task OnAuthenticationFailedAsync(AuthenticationFailedContext context)
    {
        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(context.Exception, "JWT Authentication failed: {Message}", context.Exception.Message);

        if (context.Exception is SecurityTokenExpiredException expiredException)
        {
            logger.LogWarning("Token expired at {Expires}", expiredException.Expires);
        }
        else if (context.Exception is SecurityTokenInvalidSignatureException)
        {
            logger.LogError("Invalid token signature - key mismatch");
        }
        else if (context.Exception is SecurityTokenInvalidIssuerException)
        {
            logger.LogError("Invalid issuer in token");
        }
        else if (context.Exception is SecurityTokenInvalidAudienceException)
        {
            logger.LogError("Invalid audience in token");
        }

        return Task.CompletedTask;
    }

    private static async Task OnTokenValidatedAsync(TokenValidatedContext context)
    {
        ILogger<Program> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        string? userId = context.Principal?.FindFirst("user_id")?.Value ??
                         context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        string token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            ITokenBlacklistService blacklistService = context.HttpContext.RequestServices
                .GetRequiredService<ITokenBlacklistService>();

            bool isBlacklisted = await blacklistService.IsTokenBlacklistedAsync(token);
            if (isBlacklisted)
            {
                logger.LogWarning("Blacklisted token attempted for user: {UserId}", userId ?? "Unknown");
                context.Fail("Token has been revoked");
                return;
            }

            if (!string.IsNullOrEmpty(jti))
            {
                bool isJtiBlacklisted = await blacklistService.IsTokenBlacklistedByJtiAsync(jti);
                if (isJtiBlacklisted)
                {
                    logger.LogWarning("Token with JTI {JTI} is blacklisted for user: {UserId}", jti,
                        userId ?? "Unknown");
                    context.Fail("Token has been revoked");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(userId) && long.TryParse(userId, out long userIdLong))
            {
                bool userBlacklisted = await blacklistService.AreUserTokensBlacklistedAsync(userIdLong);
                if (userBlacklisted)
                {
                    logger.LogWarning("User {UserId} tokens are blacklisted (logged out from all devices)",
                        userId);
                    context.Fail("All sessions have been terminated");
                    return;
                }
            }
        }

        logger.LogInformation("JWT Token validated successfully for user: {UserId}", userId ?? "Unknown");
    }
}
