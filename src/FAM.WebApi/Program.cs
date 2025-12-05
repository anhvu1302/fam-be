using System.Security.Claims;
using System.Security.Cryptography;
using FAM.Application.Auth.Services;
using FAM.Application.Common.Options;
using FAM.Application.Common.Services;
using FAM.Application.Querying.Parsing;
using FAM.Application.Settings;
using FAM.Application.Users.Commands.CreateUser;
using FAM.Infrastructure;
using FAM.Infrastructure.Auth;
using FAM.Infrastructure.Services;
using FAM.Infrastructure.Services.Email;
using FAM.WebApi.Configuration;
using FAM.WebApi.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

// Load environment variables from .env file
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
DotEnvLoader.LoadForEnvironment(environment);

// Load and validate configuration
var appConfig = new AppConfiguration();

var builder = WebApplication.CreateBuilder(args);

// ===== Configure Serilog =====
builder.Host.UseSerilog((context, services, configuration) => configuration
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "FAM.WebApi")
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

Log.Information("Starting FAM Web API...");

// Register AppConfiguration as singleton
builder.Services.AddSingleton(appConfig);

// Configure forwarded headers for proxy scenarios (Nginx, Load Balancer, Cloudflare, etc.)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Clear the known networks and proxies to allow any proxy
    // In production, you should specify known proxy IPs for security
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    // Add custom headers that might contain the real IP
    options.ForwardedForHeaderName = "X-Forwarded-For";
    options.ForwardedProtoHeaderName = "X-Forwarded-Proto";

    // Limit to 1 proxy hop (adjust based on your infrastructure)
    options.ForwardLimit = 2;
});

// Add CORS - configured from appsettings.json, User Secrets, or Environment Variables
builder.Services.AddOptimizedCors(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation - auto-validate all requests
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fixed Asset Management API",
        Version = "v1",
        Description = "API for managing fixed assets, companies, and users"
    });

    // Enable annotations
    c.EnableAnnotations();

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
    });
});

// Register MediatR (no validation pipeline - validation is at Web API layer)
builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly); });

// Register Filter Parser (singleton - stateless)
builder.Services.AddSingleton<IFilterParser, PrattFilterParser>();

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Redis Distributed Cache for OTP storage (from .env)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = appConfig.RedisConnectionString;
    options.InstanceName = appConfig.RedisInstanceName;
});

// Register Email Services with Queue and Provider (Brevo/SMTP)
builder.Services.AddEmailServices(builder.Configuration);

builder.Services.AddScoped<IOtpService, OtpService>();

// Add JWT Authentication using RSA keys from database
builder.Services.AddAuthentication(options =>
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
            ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
        };

        // Load RSA signing keys dynamically from database on each request
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = async context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                try
                {
                    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    logger.LogInformation("JWT Token received (first 20 chars): {Token}",
                        token.Length > 20 ? token.Substring(0, 20) + "..." : token);

                    // Get signing key service from request scope
                    var signingKeyService = context.HttpContext.RequestServices
                        .GetRequiredService<ISigningKeyService>();

                    // Get JWKS from database
                    var jwks = await signingKeyService.GetJwksAsync(context.HttpContext.RequestAborted);
                    logger.LogInformation("Loaded {Count} keys from database", jwks.Keys.Count);

                    // Convert JWKs to SecurityKeys
                    var keys = new List<SecurityKey>();
                    foreach (var jwk in jwks.Keys)
                        if (jwk.Kty == "RSA")
                        {
                            var rsa = RSA.Create(); // Don't use 'using' - key needs to live longer
                            rsa.ImportParameters(new RSAParameters
                            {
                                Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
                                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E)
                            });
                            keys.Add(new RsaSecurityKey(rsa) { KeyId = jwk.Kid });
                            logger.LogInformation("Added RSA key with Kid: {Kid}", jwk.Kid);
                        }

                    // Set the signing keys for this request
                    context.Options.TokenValidationParameters.IssuerSigningKeys = keys;
                    logger.LogInformation("Set {Count} signing keys for token validation", keys.Count);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error loading signing keys from database");
                }
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "JWT Authentication failed: {Message}", context.Exception.Message);

                if (context.Exception is SecurityTokenExpiredException)
                    logger.LogWarning("Token expired at {Expires}",
                        ((SecurityTokenExpiredException)context.Exception).Expires);
                else if (context.Exception is SecurityTokenInvalidSignatureException)
                    logger.LogError("Invalid token signature - key mismatch");
                else if (context.Exception is SecurityTokenInvalidIssuerException)
                    logger.LogError("Invalid issuer in token");
                else if (context.Exception is SecurityTokenInvalidAudienceException)
                    logger.LogError("Invalid audience in token");

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var userId = context.Principal?.FindFirst("user_id")?.Value ??
                             context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                logger.LogInformation("JWT Token validated successfully for user: {UserId}", userId ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add Rate Limiting
builder.Services.AddRateLimitingPolicies();

// Add infrastructure (database provider) - no longer needs IConfiguration
builder.Services.AddInfrastructure();

// ===== Settings from appsettings.json (non-sensitive) =====

// Pagination settings
builder.Services.Configure<PaginationSettings>(
    builder.Configuration.GetSection(PaginationSettings.SectionName));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<PaginationSettings>>().Value);

// File Upload settings
builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection(FileUploadSettings.SectionName));

// Real IP Detection settings  
builder.Services.Configure<RealIpDetectionSettings>(
    builder.Configuration.GetSection(RealIpDetectionSettings.SectionName));

// Backend settings (API URL configuration)
builder.Services.Configure<BackendOptions>(
    builder.Configuration.GetSection(BackendOptions.SectionName));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<BackendOptions>>().Value);

// Frontend settings (URL configuration) with environment variable overrides
builder.Services.Configure<FrontendOptions>(options =>
{
    // Bind from appsettings first
    builder.Configuration.GetSection(FrontendOptions.SectionName).Bind(options);

    // Override with environment variables if present
    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
    if (!string.IsNullOrEmpty(frontendUrl))
    {
        // If URL contains port, split it
        if (Uri.TryCreate(frontendUrl, UriKind.Absolute, out var uri))
        {
            options.BaseUrl = $"{uri.Scheme}://{uri.Host}";
            if (uri.Port != 80 && uri.Port != 443)
            {
                options.Port = uri.Port;
            }
            else
            {
                options.Port = null; // Default ports
            }
        }
        else
        {
            options.BaseUrl = frontendUrl;
            options.Port = null;
        }
    }

    // Override port separately if specified
    var frontendPort = Environment.GetEnvironmentVariable("FRONTEND_PORT");
    if (!string.IsNullOrEmpty(frontendPort) && int.TryParse(frontendPort, out var port))
    {
        options.Port = port;
    }
});
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<FrontendOptions>>().Value);

// Legacy paging options (backward compatibility)
var paginationSection = builder.Configuration.GetSection(PaginationSettings.SectionName);
builder.Services.Configure<PagingOptions>(options =>
{
    options.MaxPageSize = paginationSection.GetValue<int>("MaxPageSize", 100);
});

// ===== Settings from .env (sensitive - via AppConfiguration) =====

// MinIO settings from environment variables
builder.Services.Configure<MinioSettings>(options =>
{
    options.Endpoint = appConfig.MinioEndpoint;
    options.AccessKey = appConfig.MinioAccessKey;
    options.SecretKey = appConfig.MinioSecretKey;
    options.UseSsl = appConfig.MinioUseSsl;
    options.BucketName = appConfig.MinioBucketName;
});

var app = builder.Build();

// Log configuration on startup
Log.Information("FAM Web API started in {Environment} environment", environment);
appConfig.LogConfiguration(app.Services.GetRequiredService<ILogger<Program>>());

// Configure the HTTP request pipeline.

// IMPORTANT: Use forwarded headers BEFORE any other middleware
// This ensures that HttpContext.Connection.RemoteIpAddress is set correctly
app.UseForwardedHeaders();

// Add custom middleware to extract and validate real client IP
app.UseMiddleware<RealIpMiddleware>();

// Add Correlation ID middleware (for request tracing)
app.UseCorrelationId();

// Add Serilog request logging (structured logging for each request)
// Must be BEFORE UseExceptionHandler so it doesn't log handled exceptions
app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    
    // Don't log exceptions for client errors (4xx) - they are handled by GlobalExceptionHandler
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null || httpContext.Response.StatusCode >= 500)
            return Serilog.Events.LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400)
            return Serilog.Events.LogEventLevel.Warning;
        return Serilog.Events.LogEventLevel.Information;
    };

    // Attach additional properties to the log event
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("ClientIp", httpContext.Items["RealClientIp"]?.ToString()
                                          ?? httpContext.Connection.RemoteIpAddress?.ToString());

        var userId = httpContext.User.FindFirst("sub")?.Value
                     ?? httpContext.User.FindFirst("userId")?.Value;
        if (!string.IsNullOrEmpty(userId))
            diagnosticContext.Set("UserId", userId);
    };
});

// Add global exception handler (after Serilog logging)
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fixed Asset Management API v1"); });
}

app.UseHttpsRedirection();
app.UseCors(); // Enable CORS

// Add Rate Limiting middleware (after CORS, before Authentication)
app.UseRateLimiter();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the Program class accessible to integration tests
public partial class Program
{
}