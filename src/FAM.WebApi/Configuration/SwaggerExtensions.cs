using Microsoft.OpenApi;

namespace FAM.WebApi.Configuration;

/// <summary>
/// Extension methods for Swagger/OpenAPI configuration
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fixed Asset Management API",
                Version = "v1",
                Description = "API for managing fixed assets, companies, and users"
            });

            c.EnableAnnotations();
            c.SupportNonNullableReferenceTypes();
            c.UseAllOfForInheritance();
            c.UseAllOfToExtendReferenceSchemas();

            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter only your JWT token (the Bearer prefix will be added automatically)",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // Add Device ID header requirement
            c.AddSecurityDefinition("DeviceId", new OpenApiSecurityScheme
            {
                Description =
                    "Device ID from login response or stored in cookie. Required for all authenticated requests.",
                Name = "X-Device-Id",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>(),
                [new OpenApiSecuritySchemeReference("DeviceId", doc)] = new List<string>()
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerInDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fixed Asset Management API v1"); });
        }

        return app;
    }
}
