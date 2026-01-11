using FAM.Infrastructure;
using FAM.Infrastructure.Common.Seeding;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FAM.Cli;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            // Load .env file before anything else
            LoadDotEnv();

            IHost host = CreateHostBuilder(args).Build();

            if (args.Length == 0)
            {
                ShowHelp();
                return 1;
            }

            string command = args[0].ToLower();
            string[] flags = args.Skip(1).ToArray();

            return command switch
            {
                "seed" => await RunSeedCommand(host, flags),
                "seed:history" or "history" => await ShowSeedHistory(host),
                "seed:list" or "list" => await ListAvailableSeeders(host),
                "create-seeder" => CreateNewSeeder(args.Skip(1).FirstOrDefault()),
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

    private static int ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("FAM CLI - Database Management Tool");
        Console.WriteLine();
        Console.WriteLine("Usage: FAM.Cli <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  seed                Seed the database with initial data");
        Console.WriteLine("  seed:history        Show seed execution history");
        Console.WriteLine("  seed:list           List all available seeders");
        Console.WriteLine("  create-seeder       Create a new seeder template file");
        Console.WriteLine("  help                Show this help message");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --force             Force re-run all seeders (ignore history)");
        Console.WriteLine("  --resync            Delete all data and resync (IDs restart from 1)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  FAM.Cli seed                          # Run pending seeds only");
        Console.WriteLine("  FAM.Cli seed --force                  # Re-run all seeds");
        Console.WriteLine("  FAM.Cli seed --resync                 # Delete all data and resync");
        Console.WriteLine("  FAM.Cli seed:history                  # View execution history");
        Console.WriteLine("  FAM.Cli create-seeder MyNewSeeder     # Create new seeder template");
        Console.WriteLine();
        return 0;
    }

    private static int InvalidCommand(string command)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command: {command}");
        Console.ResetColor();
        Console.WriteLine();
        ShowHelp();
        return 1;
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
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
    }

    private static void LoadDotEnv()
    {
        // Find the solution root directory (where .env is located)
        string currentDir = Directory.GetCurrentDirectory();
        string? solutionRoot = FindSolutionRoot(currentDir);

        if (solutionRoot == null)
        {
            Console.WriteLine("Warning: Could not find solution root. Skipping .env file loading.");
            return;
        }

        string envFile = Path.Combine(solutionRoot, ".env");

        if (!File.Exists(envFile))
        {
            Console.WriteLine($"Warning: .env file not found at '{envFile}'.");
            return;
        }

        foreach (string line in File.ReadAllLines(envFile))
        {
            string trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            string[] parts = trimmedLine.Split('=', 2, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

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

    private static string? FindSolutionRoot(string startDirectory)
    {
        DirectoryInfo? current = new(startDirectory);

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

    private static async Task<int> RunSeedCommand(IHost host, string[] flags)
    {
        bool forceReseed = flags.Contains("--force");
        bool resync = flags.Contains("--resync");

        using IServiceScope scope = host.Services.CreateScope();
        DataSeederOrchestrator orchestrator = scope.ServiceProvider.GetRequiredService<DataSeederOrchestrator>();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║   FAM Database Seeder                          ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        if (resync)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("⚠ WARNING: RESYNC mode - deleting all data and resetting seed history!");
            Console.WriteLine("           All IDs will restart from 1");
            Console.ResetColor();
            Console.WriteLine();

            await orchestrator.ResyncDatabaseAsync();
        }
        else if (forceReseed)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ Force mode enabled - all seeders will be re-executed");
            Console.ResetColor();
            Console.WriteLine();
        }

        await orchestrator.SeedAllAsync(forceReseed || resync);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ Seeding completed successfully!");
        Console.ResetColor();
        Console.WriteLine();

        return 0;
    }

    private static async Task<int> ShowSeedHistory(IHost host)
    {
        using IServiceScope scope = host.Services.CreateScope();
        DataSeederOrchestrator orchestrator = scope.ServiceProvider.GetRequiredService<DataSeederOrchestrator>();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("═══ Seed Execution History ═══");
        Console.ResetColor();
        Console.WriteLine();

        List<SeedHistory> history = await orchestrator.GetHistoryAsync();

        if (!history.Any())
        {
            Console.WriteLine("No seed execution history found.");
            Console.WriteLine();
            return 0;
        }

        IEnumerable<IGrouping<string, SeedHistory>> grouped = history
            .OrderBy(h => h.SeederName, StringComparer.Ordinal)
            .ThenByDescending(h => h.ExecutedAt)
            .GroupBy(h => h.SeederName);

        foreach (IGrouping<string, SeedHistory> group in grouped)
        {
            SeedHistory latest = group.First();
            string status = latest.Success ? "✓" : "✗";
            ConsoleColor color = latest.Success ? ConsoleColor.Green : ConsoleColor.Red;

            Console.ForegroundColor = color;
            Console.Write($"{status} ");
            Console.ResetColor();
            Console.Write($"{latest.SeederName,-50} ");
            Console.ForegroundColor = ConsoleColor.Gray;
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

    private static async Task<int> ListAvailableSeeders(IHost host)
    {
        using IServiceScope scope = host.Services.CreateScope();
        List<IDataSeeder> seeders = scope.ServiceProvider.GetServices<IDataSeeder>()
            .OrderBy(s => s.Name, StringComparer.Ordinal)
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

        DataSeederOrchestrator orchestrator = scope.ServiceProvider.GetRequiredService<DataSeederOrchestrator>();
        List<SeedHistory> history = await orchestrator.GetHistoryAsync();
        HashSet<string> executedNames = history.Where(h => h.Success).Select(h => h.SeederName).ToHashSet();

        int index = 1;
        foreach (IDataSeeder seeder in seeders)
        {
            bool executed = executedNames.Contains(seeder.Name);
            string statusIcon = executed ? "✓" : "○";
            ConsoleColor statusColor = executed ? ConsoleColor.Green : ConsoleColor.Yellow;

            Console.ForegroundColor = statusColor;
            Console.Write($"{statusIcon} ");
            Console.ResetColor();
            Console.Write($"{index,2}. ");
            Console.Write($"{seeder.Name,-50} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(executed ? "Executed" : "Pending");
            Console.ResetColor();
            Console.WriteLine();
            index++;
        }

        Console.WriteLine();
        Console.WriteLine($"Total seeders: {seeders.Count}");
        Console.WriteLine($"Executed: {executedNames.Count}");
        Console.WriteLine($"Pending: {seeders.Count - executedNames.Count}");
        Console.WriteLine();

        return 0;
    }

    private static int CreateNewSeeder(string? seederName)
    {
        if (string.IsNullOrWhiteSpace(seederName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Seeder name is required");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Usage: FAM.Cli create-seeder <SeederName>");
            Console.WriteLine("Example: FAM.Cli create-seeder MyCustomSeeder");
            Console.WriteLine();
            return 1;
        }

        try
        {
            // Get timestamp for seeder ordering
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string className = $"{seederName}Seeder";
            string fileName = $"{timestamp}_{className}.cs";

            // Find the Seeders directory
            string currentDir = Directory.GetCurrentDirectory();
            string? seedersDir = FindSeedersDirectory(currentDir);

            if (seedersDir == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Could not find Seeders directory");
                Console.ResetColor();
                return 1;
            }

            string filePath = Path.Combine(seedersDir, fileName);

            if (File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ File already exists: {fileName}");
                Console.ResetColor();
                return 1;
            }

            // Generate seeder template
            string template = GenerateSeederTemplate(className, timestamp, seederName);

            File.WriteAllText(filePath, template);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Seeder created successfully!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"📁 File: {filePath}");
            Console.WriteLine($"📝 Class: {className}");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  1. Add your seed logic to the SeedAsync method");
            Console.WriteLine("  2. Register the seeder in ServiceCollectionExtensions.PostgreSQL.cs");
            Console.WriteLine("  3. Run 'make seed' to execute the seeder");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error creating seeder: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    private static string? FindSeedersDirectory(string startDirectory)
    {
        DirectoryInfo? current = new(startDirectory);

        while (current != null)
        {
            string seedersDir = Path.Combine(current.FullName, "src", "FAM.Infrastructure", "Seeders");
            if (Directory.Exists(seedersDir))
            {
                return seedersDir;
            }

            current = current.Parent;
        }

        return null;
    }

    private static string GenerateSeederTemplate(string className, string timestamp, string seederName)
    {
        return $@"using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Seeders;

/// <summary>
/// Seeder for {seederName}
/// TODO: Add your seeder description here
/// </summary>
public sealed class {className} : BaseDataSeeder
{{
    private readonly PostgreSqlDbContext _dbContext;

    public {className}(PostgreSqlDbContext dbContext, ILogger<{className}> logger)
        : base(logger)
    {{
        _dbContext = dbContext;
    }}

    public override string Name => ""{timestamp}_{className}"";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {{
        LogInfo(""Starting {seederName} seeding..."");

        try
        {{
            // TODO: Add your seed logic here
            // Example:
            // var hasExistingData = await _dbContext.Set<YourEntity>()
            //     .AnyAsync(cancellationToken);
            //
            // if (hasExistingData)
            // {{
            //     LogInfo(""Data already exists, skipping seed"");
            //     return;
            // }}
            //
            // var entities = new[] {{ /* create entities */ }};
            // await _dbContext.Set<YourEntity>().AddRangeAsync(entities, cancellationToken);
            // await _dbContext.SaveChangesAsync(cancellationToken);

            LogInfo(""{seederName} seeding completed successfully"");
        }}
        catch (Exception ex)
        {{
            LogError(""Error during {seederName} seeding"", ex);
            throw;
        }}
    }}
}}
";
    }
}
