using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DrugCompare.Infrastructure.Postgres;

public class PostgresDrugExplorerRepository : IDrugExplorerRepository
{
    private readonly string _connectionString;

    public PostgresDrugExplorerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection");
    }

    public async Task<List<DrugExplorerResult>> SearchAsync(string query, int limit = 50)
    {
        var results = new List<DrugExplorerResult>();

        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        const string sql = """
            SELECT
                d.id AS drug_id,
                d.name AS drug_name,
                d.normalized_name,
                d.manufacturer,
                d.source,
                COUNT(a.id) AS active_substance_count,
                COALESCE(string_agg(a.name, ', ' ORDER BY a.name), '') AS active_substances
            FROM drugs d
            LEFT JOIN drug_active_substances das
                ON das.drug_id = d.id
            LEFT JOIN active_substances a
                ON a.id = das.active_substance_id
            WHERE
                d.normalized_name ILIKE '%' || lower(@query) || '%'
                OR d.name ILIKE '%' || @query || '%'
            GROUP BY
                d.id,
                d.name,
                d.normalized_name,
                d.manufacturer,
                d.source
            ORDER BY
                CASE
                    WHEN lower(d.name) = lower(@query) THEN 0
                    WHEN d.normalized_name = lower(@query) THEN 1
                    WHEN d.normalized_name LIKE lower(@query) || '%' THEN 2
                    ELSE 3
                END,
                d.name
            LIMIT @limit;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("query", query.Trim());
        command.Parameters.AddWithValue("limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new DrugExplorerResult
            {
                DrugId = reader.GetInt64(reader.GetOrdinal("drug_id")),
                DrugName = reader.GetString(reader.GetOrdinal("drug_name")),
                NormalizedName = reader.GetString(reader.GetOrdinal("normalized_name")),
                Manufacturer = reader["manufacturer"] as string,
                Source = reader["source"] as string,
                ActiveSubstanceCount = Convert.ToInt32(reader["active_substance_count"]),
                ActiveSubstances = reader["active_substances"] as string ?? ""
            });
        }

        return results;
    }
}
