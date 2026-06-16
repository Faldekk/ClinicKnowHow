using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DrugCompare.Infrastructure.Postgres;

public sealed class PostgresIcdCodeRepository : IIcdCodeRepository
{
    private readonly string _connectionString;

    public PostgresIcdCodeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DefaultConnection connection string.");
    }

    public Task<List<IcdCodeItem>> SearchAsync(
        string query,
        string? categoryFilter,
        int limit = 50)
    {
        return SearchCodesAsync(query, categoryFilter, limit);
    }

    public async Task<List<IcdCodeItem>> SearchCodesAsync(
        string query,
        string? categoryFilter,
        int limit = 50)
    {
        var result = new List<IcdCodeItem>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = """
            SELECT
                id,
                code,
                title,
                description
            FROM icd_codes
            WHERE 1 = 1
            """;

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql += """
                
                AND (
                    code ILIKE @query
                    OR title ILIKE @query
                    
                )
                """;
        }

        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            sql += """
                
                AND category = @category
                """;
        }

        sql += """
            
            ORDER BY code
            LIMIT @limit;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        if (!string.IsNullOrWhiteSpace(query))
            command.Parameters.AddWithValue("query", $"%{query.Trim()}%");

        if (!string.IsNullOrWhiteSpace(categoryFilter))
            command.Parameters.AddWithValue("category", categoryFilter.Trim());

        command.Parameters.AddWithValue("limit", limit);

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
                    : reader.GetString(reader.GetOrdinal("description"))
            });
        }

        return result;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var result = new List<string>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
        SELECT DISTINCT category
        FROM icd_codes
        WHERE category IS NOT NULL
          AND trim(category) <> ''
        ORDER BY category;
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