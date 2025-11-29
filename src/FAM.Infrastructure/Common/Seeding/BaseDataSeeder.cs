using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Common.Seeding;

/// <summary>
/// Base class for data seeders with common functionality
/// </summary>
public abstract class BaseDataSeeder : IDataSeeder
{
    protected readonly ILogger Logger;

    protected BaseDataSeeder(ILogger logger)
    {
        Logger = logger;
    }

    public abstract Task SeedAsync(CancellationToken cancellationToken = default);

    public abstract int Order { get; }

    public abstract string Name { get; }

    protected void LogInfo(string message)
    {
        Logger.LogInformation("[{SeederName}] {Message}", Name, message);
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning("[{SeederName}] {Message}", Name, message);
    }

    protected void LogError(string message, Exception? ex = null)
    {
        if (ex != null)
            Logger.LogError(ex, "[{SeederName}] {Message}", Name, message);
        else
            Logger.LogError("[{SeederName}] {Message}", Name, message);
    }
}