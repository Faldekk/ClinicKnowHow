using DrugCompare.Models;
using DrugCompare.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DrugCompare.Repositories;

public sealed class PostgresAuditLogRepository : IAuditLogRepository
{
    private readonly string _connectionString;

    public PostgresAuditLogRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DefaultConnection connection string.");
    }

    public async Task WriteAsync(string action, string? detailsJson = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            return;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO audit_logs (
                action,
                details_json,
                created_at
            )
            VALUES (
                @action,
                @details_json,
                now()
            );
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("action", action.Trim());
        command.Parameters.AddWithValue("details_json", (object?)detailsJson ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<AuditLogItem>> GetRecentAsync(int limit = 100)
    {
        var items = new List<AuditLogItem>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT
                id,
                action,
                details_json,
                created_at
            FROM audit_logs
            ORDER BY created_at DESC
            LIMIT @limit;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AuditLogItem
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Action = reader.GetString(reader.GetOrdinal("action")),
                DetailsJson = reader.IsDBNull(reader.GetOrdinal("details_json"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("details_json")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            });
        }

        return items;
    }
}