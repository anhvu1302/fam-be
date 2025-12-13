using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Common.Seeding;

/// <summary>
/// Orchestrates the execution of all data seeders in the correct order.
/// Seeders are sorted alphabetically by Name (use timestamp prefix for ordering).
/// </summary>
public class DataSeederOrchestrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeederOrchestrator> _logger;

    public DataSeederOrchestrator(IServiceProvider serviceProvider, ILogger<DataSeederOrchestrator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Execute all registered data seeders in order (sorted by Name)
    /// </summary>
    public async Task SeedAllAsync(bool forceReseed = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Starting Data Seeding ===");

        var seeders = _serviceProvider.GetServices<IDataSeeder>()
            .OrderBy(s => s.Name, StringComparer.Ordinal)
            .ToList();

        if (!seeders.Any())
        {
            _logger.LogWarning("No data seeders registered");
            return;
        }

        // Get seed history repository
        ISeedHistoryRepository? historyRepo = _serviceProvider.GetService<ISeedHistoryRepository>();

        if (historyRepo != null && !forceReseed)
            _logger.LogInformation("Seed tracking is enabled. Checking execution history...");

        _logger.LogInformation("Found {Count} seeder(s) to execute", seeders.Count);

        foreach (IDataSeeder seeder in seeders)
        {
            // Check if already executed
            if (historyRepo != null && !forceReseed)
            {
                var alreadyExecuted = await historyRepo.HasBeenExecutedAsync(seeder.Name, cancellationToken);
                if (alreadyExecuted)
                {
                    _logger.LogInformation("⊘ Skipping: {SeederName} (already executed)", seeder.Name);
                    continue;
                }
            }

            var stopwatch = Stopwatch.StartNew();
            var success = false;
            string? errorMessage = null;

            try
            {
                _logger.LogInformation("Executing seeder: {SeederName}", seeder.Name);
                await seeder.SeedAsync(cancellationToken);
                stopwatch.Stop();
                success = true;
                _logger.LogInformation("✓ Completed: {SeederName} ({Duration}ms)", seeder.Name,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                errorMessage = ex.Message;
                _logger.LogError(ex, "✗ Failed to execute seeder: {SeederName}", seeder.Name);

                // Record failure
                if (historyRepo != null)
                    await historyRepo.RecordExecutionAsync(new SeedHistory
                    {
                        SeederName = seeder.Name,
                        ExecutedAt = DateTime.UtcNow,
                        Success = false,
                        ErrorMessage = errorMessage,
                        Duration = stopwatch.Elapsed
                    }, cancellationToken);

                throw;
            }

            // Record success
            if (historyRepo != null && success)
                await historyRepo.RecordExecutionAsync(new SeedHistory
                {
                    SeederName = seeder.Name,
                    ExecutedAt = DateTime.UtcNow,
                    Success = true,
                    Duration = stopwatch.Elapsed
                }, cancellationToken);
        }

        _logger.LogInformation("=== Data Seeding Completed Successfully ===");
    }

    /// <summary>
    /// Get seed execution history
    /// </summary>
    public async Task<List<SeedHistory>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        ISeedHistoryRepository? historyRepo = _serviceProvider.GetService<ISeedHistoryRepository>();
        if (historyRepo == null)
        {
            _logger.LogWarning("Seed history tracking is not configured");
            return new List<SeedHistory>();
        }

        return await historyRepo.GetAllHistoryAsync(cancellationToken);
    }
}
