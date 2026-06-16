using DrugCompare.Infrastructure.SQLite;
using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using Microsoft.Data.Sqlite;

namespace DrugCompare.Infrastructure.SQLite;

public class SqliteIcdCodeRepository : IIcdCodeRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteIcdCodeRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<IcdCodeItem>> SearchAsync(string query, string? categoryFilter = null, int limit = 100)
    {
        var results = new List<IcdCodeItem>();

        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        var trimmedQuery = query.Trim();
        var normalizedQuery = trimmedQuery.Replace(".", "").ToUpperInvariant();
        var hasCategoryFilter = !string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "All";

        const string sql = """
            SELECT
                id,
                code,
                normalized_code,
                title,
                normalized_title,
                description,
                chapter,
                parent_code,
                source,
                version,
                imported_at
            FROM icd_codes
            WHERE
                (
                    normalized_code LIKE '%' || @normalized_query || '%'
                    OR lower(COALESCE(title, '')) LIKE '%' || lower(@query) || '%'
                    OR lower(COALESCE(normalized_title, '')) LIKE '%' || lower(@query) || '%'
                    OR lower(COALESCE(description, '')) LIKE '%' || lower(@query) || '%'
                )
                AND
                (
                    @has_category_filter = 0
                    OR COALESCE(chapter, '') = @category_filter
                )
            ORDER BY
                CASE
                    WHEN normalized_code = @normalized_query THEN 0
                    WHEN normalized_code LIKE @normalized_query || '%' THEN 1
                    WHEN lower(COALESCE(title, '')) LIKE lower(@query) || '%' THEN 2
                    ELSE 3
                END,
                code
            LIMIT @limit;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@query", trimmedQuery);
        command.Parameters.AddWithValue("@normalized_query", normalizedQuery);
        command.Parameters.AddWithValue("@has_category_filter", hasCategoryFilter ? 1 : 0);
        command.Parameters.AddWithValue("@category_filter", categoryFilter?.Trim() ?? "");
        command.Parameters.AddWithValue("@limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(Map(reader));
        }

        return results;
    }

    public async Task<IcdCodeItem?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT
                id,
                code,
                normalized_code,
                title,
                normalized_title,
                description,
                chapter,
                parent_code,
                source,
                version,
                imported_at
            FROM icd_codes
            WHERE id = @id;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? Map(reader)
            : null;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = new List<string>();

        const string sql = """
            SELECT DISTINCT chapter
            FROM icd_codes
            WHERE chapter IS NOT NULL
              AND trim(chapter) <> ''
            ORDER BY chapter;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(reader.GetString(0));
        }

        return categories;
    }

    private static IcdCodeItem Map(SqliteDataReader reader)
    {
        return new IcdCodeItem
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            Code = GetString(reader, "code"),
            NormalizedCode = GetString(reader, "normalized_code"),
            Title = GetString(reader, "title"),
            NormalizedTitle = GetString(reader, "normalized_title"),
            Description = GetNullableString(reader, "description"),
            Chapter = GetNullableString(reader, "chapter"),
            ParentCode = GetNullableString(reader, "parent_code"),
            Source = GetString(reader, "source"),
            Version = GetNullableString(reader, "version"),
            //ImportedAt = GetNullableDateTime(reader, "imported_at")
        };
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

    private static DateTime? GetNullableDateTime(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);

        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        return DateTime.TryParse(reader.GetString(ordinal), out var value)
            ? value
            : null;
    }
}
