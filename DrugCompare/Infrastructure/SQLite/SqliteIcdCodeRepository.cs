using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;

namespace DrugCompare.Infrastructure.SQLite;

public sealed class SqliteIcdCodeRepository : IIcdCodeRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteIcdCodeRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<List<IcdCodeItem>> SearchAsync(
        string query,
        string? chapterFilter,
        int limit = 100)
    {
        return SearchCodesAsync(query, chapterFilter, limit);
    }

    public async Task<List<IcdCodeItem>> SearchCodesAsync(
        string query,
        string? chapterFilter,
        int limit = 100)
    {
        var result = new List<IcdCodeItem>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT
                id,
                code,
                title,
                description,
                chapter,
                parent_code
            FROM icd_codes
            WHERE 1 = 1
              AND code IS NOT NULL
              AND trim(code) <> ''
              AND code GLOB '*[A-Za-z]*'
            """;

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql += """
                
                AND (
                    lower(code) LIKE @query
                    OR lower(title) LIKE @query
                    OR lower(parent_code) LIKE @query
                )
                """;
        }

        if (!string.IsNullOrWhiteSpace(chapterFilter) &&
            !chapterFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            sql += """
                
                AND chapter = @chapter
                """;
        }

        sql += """
            
            ORDER BY
                CASE
                    WHEN lower(code) = @exactQuery THEN 0
                    WHEN lower(code) LIKE @startsWithQuery THEN 1
                    WHEN lower(title) LIKE @startsWithQuery THEN 2
                    ELSE 3
                END,
                code
            LIMIT @limit;
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var trimmedQuery = query?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(query))
        {
            command.Parameters.AddWithValue("@query", $"%{trimmedQuery}%");
            command.Parameters.AddWithValue("@exactQuery", trimmedQuery);
            command.Parameters.AddWithValue("@startsWithQuery", $"{trimmedQuery}%");
        }
        else
        {
            command.Parameters.AddWithValue("@exactQuery", string.Empty);
            command.Parameters.AddWithValue("@startsWithQuery", string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(chapterFilter) &&
            !chapterFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            command.Parameters.AddWithValue("@chapter", chapterFilter.Trim());
        }

        command.Parameters.AddWithValue("@limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new IcdCodeItem
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Code = reader.GetString(reader.GetOrdinal("code")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("description")),
                Chapter = reader.IsDBNull(reader.GetOrdinal("chapter"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("chapter")),
                ParentCode = reader.IsDBNull(reader.GetOrdinal("parent_code"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("parent_code"))
            });
        }

        return result;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var result = new List<string>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT DISTINCT chapter
            FROM icd_codes
            WHERE chapter IS NOT NULL
              AND trim(chapter) <> ''
            ORDER BY chapter;
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
            {
                result.Add(reader.GetString(0));
            }
        }

        return result;
    }
}