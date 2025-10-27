using FAM.Infrastructure.Common.Seeding;
using FAM.Infrastructure.Providers.PostgreSQL;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds countries data for PostgreSQL
/// </summary>
public class PostgreSqlCountrySeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public PostgreSqlCountrySeeder(PostgreSqlDbContext dbContext, ILogger<PostgreSqlCountrySeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override int Order => 2;

    public override string Name => "PostgreSQL Country Seeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed countries...");

        if (await _dbContext.Countries.AnyAsync(cancellationToken))
        {
            LogInfo("Countries already exist, skipping seed");
            return;
        }

        var countries = new List<CountryEf>
        {
            new CountryEf
            {
                Name = "Vietnam",
                Code = "VN",
                Alpha3Code = "VNM",
                NumericCode = "704",
                Capital = "Hanoi",
                Region = "Asia",
                SubRegion = "South-Eastern Asia",
                CurrencyCode = "VND",
                CurrencyName = "Vietnamese Dong",
                CallingCode = "+84",
                TopLevelDomain = ".vn",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new CountryEf
            {
                Name = "United States",
                Code = "US",
                Alpha3Code = "USA",
                NumericCode = "840",
                Capital = "Washington, D.C.",
                Region = "Americas",
                SubRegion = "Northern America",
                CurrencyCode = "USD",
                CurrencyName = "United States Dollar",
                CallingCode = "+1",
                TopLevelDomain = ".us",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new CountryEf
            {
                Name = "Japan",
                Code = "JP",
                Alpha3Code = "JPN",
                NumericCode = "392",
                Capital = "Tokyo",
                Region = "Asia",
                SubRegion = "Eastern Asia",
                CurrencyCode = "JPY",
                CurrencyName = "Japanese Yen",
                CallingCode = "+81",
                TopLevelDomain = ".jp",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _dbContext.Countries.AddRangeAsync(countries, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {countries.Count} countries successfully");
    }
}
