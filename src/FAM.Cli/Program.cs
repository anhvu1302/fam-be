using FAM.Infrastructure;
using FAM.Infrastructure.Common.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FAM.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var host = CreateHostBuilder(args).Build();

            if (args.Length == 0)
            {
                ShowHelp();
                return 1;
            }

            var command = args[0].ToLower();
            var flags = args.Skip(1).ToArray();

            return command switch
            {
                "seed" => await RunSeedCommand(host, flags),
                "seed:history" or "history" => await ShowSeedHistory(host),
                "seed:list" or "list" => await ListAvailableSeeders(host),
                "help" or "--help" or "-h" => ShowHelp(),
                _ => InvalidCommand(command)
            };
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
            return 1;
        }
    }

    static int ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("FAM CLI - Database Management Tool");
        Console.WriteLine();
        Console.WriteLine("Usage: FAM.Cli <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  seed              Seed the database with initial data");
        Console.WriteLine("  seed:history      Show seed execution history");
        Console.WriteLine("  seed:list         List all available seeders");
        Console.WriteLine("  help              Show this help message");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --force           Force re-run all seeders (ignore history)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  FAM.Cli seed                # Run pending seeds only");
        Console.WriteLine("  FAM.Cli seed --force        # Re-run all seeds");
        Console.WriteLine("  FAM.Cli seed:history        # View execution history");
        Console.WriteLine();
        return 0;
    }

    static int InvalidCommand(string command)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command: {command}");
        Console.ResetColor();
        Console.WriteLine();
        ShowHelp();
        return 1;
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Use shared appsettings.json from WebApi project
                // Handle both running from CLI folder and from root
                var currentDir = Directory.GetCurrentDirectory();
                string webApiPath;
                
                // If running from FAM.Cli folder (dotnet run)
                if (currentDir.EndsWith("FAM.Cli"))
                {
                    webApiPath = Path.Combine(currentDir, "..", "FAM.WebApi");
                }
                // If running from root folder (make seed)
                else
                {
                    webApiPath = Path.Combine(currentDir, "src", "FAM.WebApi");
                }
                
                // Resolve to absolute path
                webApiPath = Path.GetFullPath(webApiPath);
                
                if (!Directory.Exists(webApiPath))
                {
                    throw new DirectoryNotFoundException(
                        $"Cannot find WebApi directory at: {webApiPath}\n" +
                        $"Current directory: {currentDir}");
                }
                
                // Clear default configuration to have full control
                config.Sources.Clear();
                
                // Load from WebApi's appsettings files (shared configuration)
                config.SetBasePath(webApiPath);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                
                // Environment variables can override
                config.AddEnvironmentVariables();
                
                // Command line arguments have highest priority
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureServices((context, services) =>
            {
                // Add infrastructure (database providers and seeders)
                services.AddInfrastructure(context.Configuration);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });

    static async Task<int> RunSeedCommand(IHost host, string[] flags)
    {
        var forceReseed = flags.Contains("--force");

        using var scope = host.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<DataSeederOrchestrator>();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║   FAM Database Seeder                          ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        if (forceReseed)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ Force mode enabled - all seeders will be re-executed");
            Console.ResetColor();
            Console.WriteLine();
        }

        await orchestrator.SeedAllAsync(forceReseed);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ Seeding completed successfully!");
        Console.ResetColor();
        Console.WriteLine();

        return 0;
    }

    static async Task<int> ShowSeedHistory(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<DataSeederOrchestrator>();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("═══ Seed Execution History ═══");
        Console.ResetColor();
        Console.WriteLine();

        var history = await orchestrator.GetHistoryAsync();
        
        if (!history.Any())
        {
            Console.WriteLine("No seed execution history found.");
            Console.WriteLine();
            return 0;
        }

        var grouped = history
            .OrderBy(h => h.Order)
            .ThenByDescending(h => h.ExecutedAt)
            .GroupBy(h => h.SeederName);

        foreach (var group in grouped)
        {
            var latest = group.First();
            var status = latest.Success ? "✓" : "✗";
            var color = latest.Success ? ConsoleColor.Green : ConsoleColor.Red;

            Console.ForegroundColor = color;
            Console.Write($"{status} ");
            Console.ResetColor();
            Console.Write($"{latest.SeederName,-40} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"Order: {latest.Order,2}  ");
            Console.Write($"Duration: {latest.Duration.TotalMilliseconds,6:F0}ms  ");
            Console.Write($"Last: {latest.ExecutedAt:yyyy-MM-dd HH:mm:ss}");
            
            if (!latest.Success && !string.IsNullOrEmpty(latest.ErrorMessage))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    Error: {latest.ErrorMessage}");
            }
            
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine($"Total executions: {history.Count}");
        Console.WriteLine($"Unique seeders: {grouped.Count()}");
        Console.WriteLine($"Success rate: {history.Count(h => h.Success) * 100.0 / history.Count:F1}%");
        Console.WriteLine();

        return 0;
    }

    static async Task<int> ListAvailableSeeders(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>()
            .OrderBy(s => s.Order)
            .ToList();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("═══ Available Seeders ═══");
        Console.ResetColor();
        Console.WriteLine();

        if (!seeders.Any())
        {
            Console.WriteLine("No seeders registered.");
            Console.WriteLine();
            return 0;
        }

        var orchestrator = scope.ServiceProvider.GetRequiredService<DataSeederOrchestrator>();
        var history = await orchestrator.GetHistoryAsync();
        var executedNames = history.Where(h => h.Success).Select(h => h.SeederName).ToHashSet();

        foreach (var seeder in seeders)
        {
            var executed = executedNames.Contains(seeder.Name);
            var statusIcon = executed ? "✓" : "○";
            var statusColor = executed ? ConsoleColor.Green : ConsoleColor.Yellow;

            Console.ForegroundColor = statusColor;
            Console.Write($"{statusIcon} ");
            Console.ResetColor();
            Console.Write($"{seeder.Name,-40} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"Order: {seeder.Order,2}  ");
            Console.Write(executed ? "Executed" : "Pending");
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine($"Total seeders: {seeders.Count}");
        Console.WriteLine($"Executed: {executedNames.Count}");
        Console.WriteLine($"Pending: {seeders.Count - executedNames.Count}");
        Console.WriteLine();

        return 0;
    }
}
