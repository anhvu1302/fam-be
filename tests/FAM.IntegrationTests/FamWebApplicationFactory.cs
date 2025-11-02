using FAM.Domain.Users;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace FAM.IntegrationTests;

/// <summary>
/// Web Application Factory for Integration Tests
/// Uses Testcontainers to spin up isolated PostgreSQL database for each test run
/// </summary>
public class FamWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("fam_integration_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true) // Automatically cleanup container after tests
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PostgreSqlDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using the test container connection string
            services.AddDbContext<PostgreSqlDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString())
                    .EnableSensitiveDataLogging() // Helpful for debugging test failures
                    .EnableDetailedErrors();
            });
        });

        // Use test environment
        builder.UseEnvironment("IntegrationTest");
    }

    public async Task InitializeAsync()
    {
        // Start the test database container
        await _dbContainer.StartAsync();

        // Initialize database schema and seed test data
        await InitializeDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        // Cleanup: stop and remove the container
        await _dbContainer.DisposeAsync();
    }

    /// <summary>
    /// Initialize database schema and seed initial test data
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<PostgreSqlDbContext>();

        // Create database schema
        await db.Database.EnsureCreatedAsync();

        // Seed test data
        await SeedTestDataAsync(db);
    }

    /// <summary>
    /// Seed initial test data for integration tests
    /// Uses raw SQL to avoid domain validation complexity during test setup
    /// </summary>
    private static async Task SeedTestDataAsync(PostgreSqlDbContext context)
    {
        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // Generate password hashes for test users
        var adminPassword = FAM.Domain.ValueObjects.Password.Create("Admin@123");
        var testPassword = FAM.Domain.ValueObjects.Password.Create("Test@123");
        
        var now = DateTime.UtcNow;

        // Seed admin user via raw SQL (matching UserEf schema)
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO users (
                id, username, email, password_hash, password_salt, 
                first_name, last_name, full_name, phone_number,
                is_active, is_email_verified, two_factor_enabled,
                preferred_language, time_zone, receive_notifications, receive_marketing_emails,
                failed_login_attempts, lockout_end, created_at, updated_at, is_deleted
            ) VALUES (
                1, {0}, {1}, {2}, {3},
                {4}, {5}, {6}, {7},
                {8}, {9}, {10},
                {11}, {12}, {13}, {14},
                {15}, {16}, {17}, {18}, {19}
            )", 
            "admin",                    // username
            "admin@fam-test.com",      // email
            adminPassword.Hash,         // password_hash
            adminPassword.Salt,         // password_salt
            "Admin",                    // first_name
            "",                         // last_name
            "Administrator",            // full_name
            "",                         // phone_number
            true,                       // is_active
            true,                       // is_email_verified
            false,                      // two_factor_enabled
            "en",                       // preferred_language
            "UTC",                      // time_zone
            true,                       // receive_notifications
            false,                      // receive_marketing_emails
            0,                          // failed_login_attempts
            DBNull.Value,               // lockout_end
            now,                        // created_at
            now,                        // updated_at
            false                       // is_deleted
        );

        // Seed test user via raw SQL (matching UserEf schema)
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO users (
                id, username, email, password_hash, password_salt, 
                first_name, last_name, full_name, phone_number,
                is_active, is_email_verified, two_factor_enabled,
                preferred_language, time_zone, receive_notifications, receive_marketing_emails,
                failed_login_attempts, lockout_end, created_at, updated_at, is_deleted
            ) VALUES (
                2, {0}, {1}, {2}, {3},
                {4}, {5}, {6}, {7},
                {8}, {9}, {10},
                {11}, {12}, {13}, {14},
                {15}, {16}, {17}, {18}, {19}
            )",
            "testuser",                 // username
            "testuser@fam-test.com",   // email
            testPassword.Hash,          // password_hash
            testPassword.Salt,          // password_salt
            "Test",                     // first_name
            "User",                     // last_name
            "Test User",                // full_name
            "",                         // phone_number
            true,                       // is_active
            true,                       // is_email_verified
            false,                      // two_factor_enabled
            "en",                       // preferred_language
            "UTC",                      // time_zone
            true,                       // receive_notifications
            false,                      // receive_marketing_emails
            0,                          // failed_login_attempts
            DBNull.Value,               // lockout_end
            now,                        // created_at
            now,                        // updated_at
            false                       // is_deleted
        );

        // Update sequence to start from 3
        await context.Database.ExecuteSqlRawAsync("SELECT setval('users_id_seq', 2, true);");
    }

    /// <summary>
    /// Reset database to clean state between test classes if needed
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PostgreSqlDbContext>();

        // Clear all data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Re-seed
        await SeedTestDataAsync(context);
    }
}
