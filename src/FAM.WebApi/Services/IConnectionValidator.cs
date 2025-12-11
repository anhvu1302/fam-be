namespace FAM.WebApi.Services;

/// <summary>
/// Service that validates all external connections at application startup.
/// If any connection fails, the application should not start.
/// </summary>
public interface IConnectionValidator
{
    /// <summary>
    /// Validates all external connections (PostgreSQL, Redis, MinIO).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task that completes when all connections are validated.</returns>
    /// <exception cref="InvalidOperationException">Thrown when any connection fails.</exception>
    Task ValidateAllAsync(CancellationToken cancellationToken = default);
}