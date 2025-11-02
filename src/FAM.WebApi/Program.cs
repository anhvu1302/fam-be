using FAM.Application.Common.Mappings;
using FAM.Application.Common.Options;
using FAM.Application.Querying.Parsing;
using FAM.Application.Auth.Services;
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
using System.Text;

// Load environment variables from .env file
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
DotEnvLoader.LoadForEnvironment(environment);

// Load and validate configuration
var appConfig = new AppConfiguration();

var builder = WebApplication.CreateBuilder(args);

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

// Add services to the container.
builder.Services.AddControllers();

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
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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
    cfg.RegisterServicesFromAssembly(typeof(FAM.Application.Users.Commands.CreateUserCommand).Assembly);
});

// Register Filter Parser (singleton - stateless)
builder.Services.AddSingleton<IFilterParser, PrattFilterParser>();

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

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

// Add infrastructure (database provider) - no longer needs IConfiguration
builder.Services.AddInfrastructure();

// Bind paging options from AppConfiguration
builder.Services.Configure<PagingOptions>(options =>
{
    options.MaxPageSize = appConfig.MaxPageSize;
});

var app = builder.Build();

// Log configuration on startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
appConfig.LogConfiguration(logger);

// Configure the HTTP request pipeline.

// Add global exception handler (must be early in pipeline)
app.UseExceptionHandler();

// IMPORTANT: Use forwarded headers BEFORE any other middleware
// This ensures that HttpContext.Connection.RemoteIpAddress is set correctly
app.UseForwardedHeaders();

// Add custom middleware to extract and validate real client IP
app.UseMiddleware<RealIpMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fixed Asset Management API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the Program class accessible to integration tests
public partial class Program { }
