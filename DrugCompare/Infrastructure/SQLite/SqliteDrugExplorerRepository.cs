using DrugCompare.Infrastructure.SQLite;
using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using Microsoft.Data.Sqlite;

namespace DrugCompare.Infrastructure.SQLite;

public sealed class SqliteDrugExplorerRepository : IDrugExplorerRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteDrugExplorerRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<DrugExplorerResult>> SearchAsync(string query, int limit = 50)
    {
        var results = new List<DrugExplorerResult>();

        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        var trimmedQuery = query.Trim();
        var queryLower = trimmedQuery.ToLowerInvariant();

        const string sql = """
            SELECT
                id,
                product_name,
                normalized_product_name,
                marketing_authorization_holder,
                source,
                active_substance_text
            FROM polish_drug_registry_items
            WHERE
                normalized_product_name LIKE '%' || @query_lower || '%'
                OR product_name LIKE '%' || @query || '%'
                OR active_substance_text LIKE '%' || @query || '%'
            ORDER BY
                CASE
                    WHEN normalized_product_name = @query_lower THEN 0
                    WHEN normalized_product_name LIKE @query_lower || '%' THEN 1
                    WHEN product_name LIKE @query || '%' THEN 2
                    ELSE 3
                END,
                product_name
            LIMIT @limit;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@query", trimmedQuery);
        command.Parameters.AddWithValue("@query_lower", queryLower);
        command.Parameters.AddWithValue("@limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var substances = GetNullableString(reader, "active_substance_text") ?? "";

            results.Add(new DrugExplorerResult
            {
                DrugId = reader.GetInt64(reader.GetOrdinal("id")),
                DrugName = GetString(reader, "product_name"),
                NormalizedName = GetString(reader, "normalized_product_name"),
                Manufacturer = GetNullableString(reader, "marketing_authorization_holder"),
                Source = GetNullableString(reader, "source") ?? "RPL SQLite",
                ActiveSubstances = substances,
                ActiveSubstanceCount = CountSubstances(substances)
            });
        }

        return results;
    }

    private static int CountSubstances(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return value
            .Split(new[] { ',', ';', '+', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
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
}
