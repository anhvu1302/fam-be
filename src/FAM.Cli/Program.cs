using FAM.Infrastructure;
using FAM.Infrastructure.Common.Seeding;
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
            // Load .env file before anything else
            LoadDotEnv();
            
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
            .ConfigureServices((context, services) =>
            {
                // Add infrastructure (uses environment variables loaded from .env)
                services.AddInfrastructure();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });

    static void LoadDotEnv()
    {
        // Find the solution root directory (where .env is located)
        var currentDir = Directory.GetCurrentDirectory();
        var solutionRoot = FindSolutionRoot(currentDir);
        
        if (solutionRoot == null)
        {
            Console.WriteLine("Warning: Could not find solution root. Skipping .env file loading.");
            return;
        }

        var envFile = Path.Combine(solutionRoot, ".env");
        
        if (!File.Exists(envFile))
        {
            Console.WriteLine($"Warning: .env file not found at '{envFile}'.");
            return;
        }

        foreach (var line in File.ReadAllLines(envFile))
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            var parts = trimmedLine.Split('=', 2, StringSplitOptions.None);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Remove quotes if present
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value[1..^1];
            }

            // Only set if not already set (system env vars take precedence)
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    static string? FindSolutionRoot(string startDirectory)
    {
        var current = new DirectoryInfo(startDirectory);
        
        while (current != null)
        {
            // Look for .sln file or docker-compose.yml as indicators of solution root
            if (current.GetFiles("*.sln").Length > 0 || 
                current.GetFiles("docker-compose.yml").Length > 0)
            {
                return current.FullName;
            }
            
            current = current.Parent;
        }
        
        return null;
    }

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
