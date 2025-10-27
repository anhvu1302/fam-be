namespace FAM.Infrastructure.Common.Seeding;

/// <summary>
/// Interface for data seeders
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Seed data asynchronously
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the order in which this seeder should run (lower numbers run first)
    /// </summary>
    int Order { get; }
    
    /// <summary>
    /// Get the name of this seeder
    /// </summary>
    string Name { get; }
}
