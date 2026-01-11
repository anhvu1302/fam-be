using FAM.Infrastructure.Common.Abstractions;
using FAM.Infrastructure.Common.Options;

namespace FAM.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// PostgreSQL DbContext Factory implementation
/// Creates PostgreSqlDbContext instances with proper configuration
/// Enables clean architecture by abstracting context creation
/// </summary>
public class PostgreSqlDbContextFactory : IDbContextFactory
{
    private readonly PostgreSqlOptions _options;

    public PostgreSqlDbContextFactory(PostgreSqlOptions options)
    {
        _options = options;
    }

    public IDbContext CreateContext()
    {
        return new PostgreSqlDbContext(_options);
    }
}
