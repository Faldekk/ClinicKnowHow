using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using Npgsql;

namespace DrugCompare.Infrastructure.Postgres;

public sealed class PostgresAuditLogRepository : IAuditLogRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresAuditLogRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddAsync(string eventType, string? details = null)
    {
        const string sql = """
            INSERT INTO audit_logs (event_type, details, created_at)
            VALUES (@event_type, @details, NOW());
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("event_type", eventType);
        command.Parameters.AddWithValue("details", (object?)details ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<AuditLogItem>> GetRecentLogsAsync(int limit = 50)
    {
        const string sql = """
            SELECT id, event_type, details, created_at
            FROM audit_logs
            ORDER BY created_at DESC
            LIMIT @limit;
            """;

        var result = new List<AuditLogItem>();

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new AuditLogItem
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                EventType = reader.GetString(reader.GetOrdinal("event_type")),
                Details = reader.IsDBNull(reader.GetOrdinal("details"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("details")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            });
        }

        return result;
    }
}