using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;

namespace DrugCompare.Infrastructure.SQLite;

public sealed class SqlitePolishDrugRegistryRepository : IPolishDrugRegistryRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqlitePolishDrugRegistryRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<PolishDrugRegistryItem>> SearchAsync(string query, int limit = 100)
    {
        var result = new List<PolishDrugRegistryItem>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = """
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
            WHERE 1 = 1
            """;

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql += """
                
                AND (
                    lower(product_name) LIKE @query
                    OR lower(normalized_product_name) LIKE @query
                    OR lower(active_substance_text) LIKE @query
                    OR lower(marketing_authorization_holder) LIKE @query
                    OR lower(authorization_number) LIKE @query
                    OR lower(rpl_id) LIKE @query
                    OR lower(chpl_url) LIKE @query
                    OR lower(leaflet_url) LIKE @query
                )
                """;
        }

        sql += """
            
            ORDER BY
                CASE
                    WHEN lower(product_name) = @exactQuery THEN 0
                    WHEN lower(product_name) LIKE @startsWithQuery THEN 1
                    WHEN lower(normalized_product_name) LIKE @startsWithQuery THEN 2
                    WHEN lower(active_substance_text) LIKE @startsWithQuery THEN 3
                    ELSE 4
                END,
                product_name
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

        command.Parameters.AddWithValue("@limit", limit);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new PolishDrugRegistryItem
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                RplId = ReadNullableString(reader, "rpl_id"),

                ProductName = ReadNullableString(reader, "product_name") ?? string.Empty,
                NormalizedProductName = ReadNullableString(reader, "normalized_product_name") ?? string.Empty,

                ActiveSubstanceText = ReadNullableString(reader, "active_substance_text"),
                Strength = ReadNullableString(reader, "strength"),
                PharmaceuticalForm = ReadNullableString(reader, "pharmaceutical_form"),
                MarketingAuthorizationHolder = ReadNullableString(reader, "marketing_authorization_holder"),

                AuthorizationNumber = ReadNullableString(reader, "authorization_number"),
                AuthorizationValidity = ReadNullableString(reader, "authorization_validity"),
                ProductType = ReadNullableString(reader, "product_type"),
                ProcedureType = ReadNullableString(reader, "procedure_type"),

                ChplUrl = ReadNullableString(reader, "chpl_url"),
                LeafletUrl = ReadNullableString(reader, "leaflet_url"),

                Source = ReadNullableString(reader, "source") ?? "RPL",
                SourceVersion = ReadNullableString(reader, "source_version"),

                ImportedAt = ReadNullableDateTime(reader, "imported_at")
            });
        }

        return result;
    }

    private static string? ReadNullableString(System.Data.Common.DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }

    private static DateTime ReadNullableDateTime(System.Data.Common.DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
        {
            return default;
        }

        var value = reader.GetValue(ordinal);

        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        if (DateTime.TryParse(value.ToString(), out var parsed))
        {
            return parsed;
        }

        return default;
    }
}