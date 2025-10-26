using FAM.Application.Common.Mappings;
using FAM.Application.Common.Options;
using FAM.Application.Querying.Parsing;
using FAM.Infrastructure;
using FAM.WebApi.Controllers;
using MediatR;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Fixed Asset Management API",
        Version = "v1",
        Description = "API for managing fixed assets, companies, and users"
    });
});

// Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FAM.Application.Users.Commands.CreateUserCommand).Assembly);
});

// Register Filter Parser (singleton - stateless)
builder.Services.AddSingleton<IFilterParser, PrattFilterParser>();

// Add infrastructure (database provider) - includes AutoMapper registration
builder.Services.AddInfrastructure(builder.Configuration);

// Bind paging options from configuration
builder.Services.Configure<PagingOptions>(builder.Configuration.GetSection("Paging"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fixed Asset Management API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
