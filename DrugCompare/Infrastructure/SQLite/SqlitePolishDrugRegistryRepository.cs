using DrugCompare.Infrastructure.SQLite;
using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using Microsoft.Data.Sqlite;

namespace DrugCompare.Infrastructure.SQLite;

public class SqlitePolishDrugRegistryRepository : IPolishDrugRegistryRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqlitePolishDrugRegistryRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<PolishDrugRegistryItem>> SearchAsync(string query, int limit = 100)
    {
        var results = new List<PolishDrugRegistryItem>();

        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        var trimmedQuery = query.Trim();

        const string sql = """
            SELECT
                id,
                rpl_id,
                product_name,
                normalized_product_name,
                active_substance_text,
                strength,
                pharmaceutical_form,
                marketing_authorization_holder,
                authorization_number,
                authorization_validity,
                product_type,
                procedure_type,
                chpl_url,
                leaflet_url,
                source,
                source_version,
                imported_at
            FROM polish_drug_registry_items
            WHERE
                lower(COALESCE(product_name, '')) LIKE '%' || lower(@query) || '%'
                OR lower(COALESCE(normalized_product_name, '')) LIKE '%' || lower(@query) || '%'
                OR lower(COALESCE(active_substance_text, '')) LIKE '%' || lower(@query) || '%'
                OR lower(COALESCE(authorization_number, '')) LIKE '%' || lower(@query) || '%'
            ORDER BY
                CASE
                    WHEN lower(product_name) = lower(@query) THEN 0
                    WHEN lower(product_name) LIKE lower(@query) || '%' THEN 1
                    ELSE 2
                END,
                product_name
            LIMIT @limit;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@query", trimmedQuery);
        command.Parameters.AddWithValue("@limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(Map(reader));
        }

        return results;
    }

    public async Task<PolishDrugRegistryItem?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT
                id,
                rpl_id,
                product_name,
                normalized_product_name,
                active_substance_text,
                strength,
                pharmaceutical_form,
                marketing_authorization_holder,
                authorization_number,
                authorization_validity,
                product_type,
                procedure_type,
                chpl_url,
                leaflet_url,
                source,
                source_version,
                imported_at
            FROM polish_drug_registry_items
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

    private static PolishDrugRegistryItem Map(SqliteDataReader reader)
    {
        return new PolishDrugRegistryItem
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            RplId = GetNullableString(reader, "rpl_id"),
            ProductName = GetString(reader, "product_name"),
            NormalizedProductName = GetString(reader, "normalized_product_name"),
            ActiveSubstanceText = GetNullableString(reader, "active_substance_text"),
            Strength = GetNullableString(reader, "strength"),
            PharmaceuticalForm = GetNullableString(reader, "pharmaceutical_form"),
            MarketingAuthorizationHolder = GetNullableString(reader, "marketing_authorization_holder"),
            AuthorizationNumber = GetNullableString(reader, "authorization_number"),
            AuthorizationValidity = GetNullableString(reader, "authorization_validity"),
            ProductType = GetNullableString(reader, "product_type"),
            ProcedureType = GetNullableString(reader, "procedure_type"),
            ChplUrl = GetNullableString(reader, "chpl_url"),
            LeafletUrl = GetNullableString(reader, "leaflet_url"),
            Source = GetString(reader, "source"),
            SourceVersion = GetNullableString(reader, "source_version"),
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
