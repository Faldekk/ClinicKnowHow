using DrugCompare.Infrastructure.SQLite;
using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using DrugCompare.Application.Services.Implementations;
using DrugCompare.Application.Services.Contracts;

namespace DrugCompare.Application.Services.Implementations;

public sealed class SqliteDatabaseStatusService 
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteDatabaseStatusService(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DatabaseStatusResult> GetDatabaseStatusAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        return new DatabaseStatusResult
        {
            DrugsCount = await CountAsync(connection, "drugs"),
            ActiveSubstancesCount = await CountAsync(connection, "active_substances"),
            DrugActiveSubstancesCount = await CountAsync(connection, "drug_active_substances"),
            SubstanceInteractionsCount = await CountAsync(connection, "substance_interactions")
        };
    }

    private static async Task<int> CountAsync(Microsoft.Data.Sqlite.SqliteConnection connection, string table)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table};";

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}
