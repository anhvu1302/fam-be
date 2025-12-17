namespace FAM.WebApi.Services;

/// <summary>
/// Service that validates all external connections at application startup.
/// If any connection fails, the application should not start.
/// Validates connections based on configured providers (Database, Cache, Storage).
/// </summary>
public interface IConnectionValidator
{
    /// <summary>
    /// Validates all external connections based on configured providers.
    /// - Database: PostgreSQL
    /// - Cache: Redis or InMemory (based on CACHE_PROVIDER)
    /// - Storage: MinIO
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task that completes when all connections are validated.</returns>
    /// <exception cref="InvalidOperationException">Thrown when any connection fails.</exception>
    Task ValidateAllAsync(CancellationToken cancellationToken = default);
}
