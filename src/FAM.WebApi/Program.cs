using FAM.Infrastructure;
using FAM.Infrastructure.Providers.Cache;
using FAM.Infrastructure.Providers.PostgreSQL;
using FAM.Infrastructure.Providers.RateLimit;
using FAM.Infrastructure.Services.Email;
using FAM.WebApi.Configuration;
using FAM.WebApi.Middleware;
using FAM.WebApi.Services;

using FluentValidation;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Events;

// Load environment variables from .env file
string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
DotEnvLoader.LoadForEnvironment(environment);

// Load and validate configuration
AppConfiguration appConfig = new();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

// Configure forwarded headers for proxy scenarios
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardedForHeaderName = "X-Forwarded-For";
    options.ForwardedProtoHeaderName = "X-Forwarded-Proto";
    options.ForwardLimit = 2;
});

// Add CORS
builder.Services.AddOptimizedCors(builder.Configuration);

// Add controllers with validation filter
builder.Services.AddControllers(options => { options.Filters.Add<ValidationFilter>(); })
    .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context => { context.ProblemDetails.Extensions.Clear(); };
});

// Add Swagger configuration
builder.Services.AddSwaggerConfiguration();

// Add application services (MediatR, JWT, OTP, FileValidator, etc.)
builder.Services.AddApplicationServices();

// Add Cache Provider (Redis for production, In-Memory for development)
builder.Services.AddCacheProvider(builder.Configuration);

// Add Email Services with Queue and Provider (Brevo/SMTP)
builder.Services.AddEmailServices(builder.Configuration);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(appConfig);

// Add Rate Limiting with cache-based store
builder.Services.AddRateLimitingPolicies();
builder.Services.AddRateLimiterStore();

// Add infrastructure (database provider)
builder.Services.AddInfrastructure();

// Add application settings
builder.Services.AddApplicationSettings(builder.Configuration, appConfig);

WebApplication app = builder.Build();

Log.Information("FAM Web API started in {Environment} environment", environment);

// Validate all external connections before starting
try
{
    Log.Information("Validating all connections (PostgreSQL, Redis, MinIO)...");
    using IServiceScope validatorScope = app.Services.CreateScope();
    IConnectionValidator validator = validatorScope.ServiceProvider.GetRequiredService<IConnectionValidator>();
    await validator.ValidateAllAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Connection validation failed. Application will not start.");
    throw;
}

// Configure the HTTP request pipeline
app.UseApplicationMiddleware();

// Apply migrations before running the app
try
{
    Log.Information("Applying database migrations...");
    using (IServiceScope scope = app.Services.CreateScope())
    {
        PostgreSqlDbContext dbContext = scope.ServiceProvider.GetRequiredService<PostgreSqlDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("✅ Database migrations applied successfully");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "❌ Error applying migrations");
    throw;
}

app.Run();

// Make the Program class accessible to integration tests
public partial class Program
{
}
