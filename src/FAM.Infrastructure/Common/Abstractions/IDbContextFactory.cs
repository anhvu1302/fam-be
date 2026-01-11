namespace FAM.Infrastructure.Common.Abstractions;

/// <summary>
/// Factory for creating DbContext instances based on database provider
/// Enables easy switching between different database providers
/// Follows Clean Architecture for dependency injection
/// </summary>
public interface IDbContextFactory
{
    IDbContext CreateContext();
}
