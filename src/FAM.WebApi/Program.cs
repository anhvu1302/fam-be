using FAM.Application.Common.Options;
using FAM.Application.Querying.Parsing;
using FAM.Application.Auth.Services;
using FAM.Application.Settings;
using FAM.Infrastructure;
using FAM.Infrastructure.Auth;
using FAM.WebApi.Configuration;
using FAM.WebApi.Controllers;
using FAM.WebApi.Middleware;
using MediatR;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using Serilog;
using Serilog.Events;
using FluentValidation;
using FluentValidation.AspNetCore;

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

// Add CORS - configured from appsettings.json
var corsSection = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:8001" };
var allowedMethods = corsSection.GetSection("AllowedMethods").Get<string[]>() ??
                     new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" };
var allowedHeaders = corsSection.GetSection("AllowedHeaders").Get<string[]>() ??
                     new[] { "Content-Type", "Authorization" };
var allowCredentials = corsSection.GetValue<bool>("AllowCredentials", true);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .WithMethods(allowedMethods)
            .WithHeaders(allowedHeaders);

        if (allowCredentials)
            policy.AllowCredentials();
    });
});

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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Fixed Asset Management API",
        Version = "v1",
        Description = "API for managing fixed assets, companies, and users"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register MediatR (no validation pipeline - validation is at Web API layer)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FAM.Application.Users.Commands.CreateUser.CreateUserCommand).Assembly);
});

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

// Register Email and OTP Services (Email uses config from .env)
builder.Services.AddScoped<FAM.Application.Common.Services.IEmailService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FAM.Infrastructure.Services.EmailService>>();
    return new FAM.Infrastructure.Services.EmailService(
        logger,
        appConfig.SmtpHost,
        appConfig.SmtpPort,
        appConfig.SmtpUsername,
        appConfig.SmtpPassword,
        appConfig.SmtpEnableSsl,
        appConfig.EmailFrom,
        appConfig.EmailFromName
    );
});
builder.Services.AddScoped<FAM.Application.Common.Services.IOtpService, FAM.Infrastructure.Services.OtpService>();

// Add JWT Authentication
var secretKey = appConfig.JwtSecret;

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
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

// Add global exception handler (must be early in pipeline)
app.UseExceptionHandler();

// IMPORTANT: Use forwarded headers BEFORE any other middleware
// This ensures that HttpContext.Connection.RemoteIpAddress is set correctly
app.UseForwardedHeaders();

// Add custom middleware to extract and validate real client IP
app.UseMiddleware<RealIpMiddleware>();

// Add Correlation ID middleware (for request tracing)
app.UseCorrelationId();

// Add Serilog request logging (structured logging for each request)
app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

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

if (app.Environment.IsDevelopment())
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