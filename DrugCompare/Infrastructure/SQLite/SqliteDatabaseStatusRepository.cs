using DrugCompare.Infrastructure.SQLite;
using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrugCompare.Infrastructure.SQLite;

public class SqliteDatabaseStatusRepository : IDatabaseStatusRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteDatabaseStatusRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Dictionary<string, long>> GetTableCountsAsync()
    {
        var tables = new[]
        {
            "drugs",
            "drug_active_substances",
            "active_substances",
            "substance_interactions",
            "icd_codes",
            "polish_drug_registry_items",
            "audit_logs",
            "interaction_history"
        };

        var result = new Dictionary<string, long>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        foreach (var table in tables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {table};";

            var value = await command.ExecuteScalarAsync();
            result[table] = Convert.ToInt64(value);
        }

        return result;
    }

    public async Task<DatabaseStatusResult> GetDatabaseStatusAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM drugs) AS drugs_count,
                (SELECT COUNT(*) FROM active_substances) AS active_substances_count,
                (SELECT COUNT(*) FROM drug_active_substances) AS drug_active_substances_count,
                (SELECT COUNT(*) FROM substance_interactions) AS substance_interactions_count;
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return new DatabaseStatusResult();

        return new DatabaseStatusResult
        {
            DrugsCount = reader.GetInt64(reader.GetOrdinal("drugs_count")),
            ActiveSubstancesCount = reader.GetInt64(reader.GetOrdinal("active_substances_count")),
            DrugActiveSubstancesCount = reader.GetInt64(reader.GetOrdinal("drug_active_substances_count")),
            SubstanceInteractionsCount = reader.GetInt64(reader.GetOrdinal("substance_interactions_count"))
        };
    }
}
