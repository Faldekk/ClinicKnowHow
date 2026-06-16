using DrugCompare.Application.Repositories.Contracts;
using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using DrugCompare.Infrastructure.SQLite;
using Microsoft.Data.Sqlite;

namespace DrugCompare.Infrastructure.SQLite;

public sealed class SqliteSubstanceRepository : ISubstanceRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteSubstanceRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ActiveSubstanceItem?> FindActiveSubstanceAsync(string substanceName)
    {
        if (string.IsNullOrWhiteSpace(substanceName))
        {
            return null;
        }

        var query = substanceName.Trim();
        var normalized = Normalize(query);

        const string sql = """
            SELECT
                id,
                name,
                normalized_name,
                ddinter_id,
                source
            FROM active_substances
            WHERE
                normalized_name = @normalized
                OR normalized_name LIKE '%' || @normalized || '%'
                OR name LIKE '%' || @query || '%'
            ORDER BY
                CASE
                    WHEN normalized_name = @normalized THEN 0
                    WHEN normalized_name LIKE @normalized || '%' THEN 1
                    WHEN name LIKE @query || '%' THEN 2
                    ELSE 3
                END,
                name
            LIMIT 1;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@query", query);
        command.Parameters.AddWithValue("@normalized", normalized);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return new ActiveSubstanceItem
            {
                DatabaseId = null,
                Name = query,
                NormalizedName = normalized,
                DDInterId = null,
                Source = "Manual - not found in SQLite database"
            };
        }

        return new ActiveSubstanceItem
        {
            DatabaseId = reader.GetInt64(reader.GetOrdinal("id")),
            Name = GetString(reader, "name"),
            NormalizedName = GetString(reader, "normalized_name"),
            DDInterId = GetNullableString(reader, "ddinter_id"),
            Source = GetNullableString(reader, "source") ?? "SQLite"
        };
    }

    public async Task AddSynonymAsync(long activeSubstanceId, string synonym, string source = "manual")
    {
        if (string.IsNullOrWhiteSpace(synonym))
        {
            return;
        }

        const string sql = """
            INSERT OR IGNORE INTO active_substance_synonyms (
                active_substance_id,
                synonym,
                normalized_synonym,
                source,
                created_at
            )
            VALUES (
                @active_substance_id,
                @synonym,
                @normalized_synonym,
                @source,
                datetime('now')
            );
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@active_substance_id", activeSubstanceId);
        command.Parameters.AddWithValue("@synonym", synonym.Trim());
        command.Parameters.AddWithValue("@normalized_synonym", Normalize(synonym));
        command.Parameters.AddWithValue("@source", source);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<ActiveSubstanceSynonymItem>> GetSynonymsAsync(long activeSubstanceId)
    {
        var results = new List<ActiveSubstanceSynonymItem>();

        const string sql = """
            SELECT
                id,
                active_substance_id,
                synonym,
                normalized_synonym,
                source,
                created_at
            FROM active_substance_synonyms
            WHERE active_substance_id = @active_substance_id
            ORDER BY synonym;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@active_substance_id", activeSubstanceId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new ActiveSubstanceSynonymItem
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                ActiveSubstanceId = reader.GetInt64(reader.GetOrdinal("active_substance_id")),
                Synonym = GetString(reader, "synonym"),
                NormalizedSynonym = GetString(reader, "normalized_synonym"),
                Source = GetString(reader, "source"),
                CreatedAt = GetDateTime(reader, "created_at")
            });
        }

        return results;
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant().Replace("_", " ").Replace("-", " ");
    }

    private static string GetString(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
    }

    private static string? GetNullableString(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static DateTime GetDateTime(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);

        if (reader.IsDBNull(ordinal))
        {
            return DateTime.MinValue;
        }

        return DateTime.TryParse(reader.GetString(ordinal), out var value)
            ? value
            : DateTime.MinValue;
    }
}
