using FAM.Infrastructure.Common.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

public class SeedHistoryRepositoryPostgreSql : ISeedHistoryRepository
{
    private readonly PostgreSqlDbContext _dbContext;

    public SeedHistoryRepositoryPostgreSql(PostgreSqlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasBeenExecutedAsync(string seederName, CancellationToken cancellationToken = default)
    {
        // Check if table exists
        var tableExists = await _dbContext.Database
            .ExecuteSqlAsync($@"
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_name = '__seed_history'", 
                cancellationToken) > 0;

        if (!tableExists)
        {
            await CreateTableAsync(cancellationToken);
            return false;
        }

        // Check if seeder has been executed
        var result = await _dbContext.Database
            .ExecuteSqlAsync($@"
                SELECT COUNT(*) 
                FROM __seed_history 
                WHERE seeder_name = {seederName} AND success = true",
                cancellationToken);

        return result > 0;
    }

    public async Task RecordExecutionAsync(SeedHistory history, CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO __seed_history 
                (seeder_name, ""order"", executed_at, executed_by, success, error_message, duration_ms)
            VALUES 
                (@seederName, @order, @executedAt, @executedBy, @success, @errorMessage, @durationMs)";

        AddParameter(command, "@seederName", history.SeederName);
        AddParameter(command, "@order", history.Order);
        AddParameter(command, "@executedAt", history.ExecutedAt);
        AddParameter(command, "@executedBy", history.ExecutedBy);
        AddParameter(command, "@success", history.Success);
        AddParameter(command, "@errorMessage", (object?)history.ErrorMessage ?? DBNull.Value);
        AddParameter(command, "@durationMs", history.Duration.TotalMilliseconds);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    public async Task<List<SeedHistory>> GetAllHistoryAsync(CancellationToken cancellationToken = default)
    {
        // Ensure table exists
        await HasBeenExecutedAsync("_init_check_", cancellationToken);
        
        var histories = new List<SeedHistory>();
        
        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM __seed_history ORDER BY executed_at DESC";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            histories.Add(new SeedHistory
            {
                Id = reader.GetInt64(0),
                SeederName = reader.GetString(1),
                Order = reader.GetInt32(2),
                ExecutedAt = reader.GetDateTime(3),
                ExecutedBy = reader.GetString(4),
                Success = reader.GetBoolean(5),
                ErrorMessage = reader.IsDBNull(6) ? null : reader.GetString(6),
                Duration = TimeSpan.FromMilliseconds(reader.GetDouble(7))
            });
        }

        return histories;
    }

    private async Task CreateTableAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS __seed_history (
                id BIGSERIAL PRIMARY KEY,
                seeder_name VARCHAR(255) NOT NULL,
                ""order"" INT NOT NULL,
                executed_at TIMESTAMP NOT NULL,
                executed_by VARCHAR(100) NOT NULL,
                success BOOLEAN NOT NULL,
                error_message TEXT,
                duration_ms DOUBLE PRECISION NOT NULL,
                CONSTRAINT uq_seeder_name UNIQUE (seeder_name)
            );
            
            CREATE INDEX IF NOT EXISTS ix_seed_history_executed_at 
                ON __seed_history(executed_at);
            
            CREATE INDEX IF NOT EXISTS ix_seed_history_seeder_name 
                ON __seed_history(seeder_name);
        ", cancellationToken);
    }
}
