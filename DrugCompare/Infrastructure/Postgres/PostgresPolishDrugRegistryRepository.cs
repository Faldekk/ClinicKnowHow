using DrugCompare.Application.Models;
using DrugCompare.Application.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DrugCompare.Infrastructure.Postgres;

public class PostgresPolishDrugRegistryRepository : IPolishDrugRegistryRepository
{
    private readonly string _connectionString;

    public PostgresPolishDrugRegistryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection");
    }

    public async Task<List<PolishDrugRegistryItem>> SearchAsync(string query, int limit = 50)
    {
        var result = new List<PolishDrugRegistryItem>();

        await using var connection = new NpgsqlConnection(_connectionString);
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
        FROM polish_drug_registry
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

                ImportedAt = reader.IsDBNull(reader.GetOrdinal("imported_at"))
                    ? default
                    : reader.GetDateTime(reader.GetOrdinal("imported_at"))
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

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return Map(reader);
        }

        return null;
    }

    private static PolishDrugRegistryItem Map(NpgsqlDataReader reader)
    {
        return new PolishDrugRegistryItem
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            RplId = reader["rpl_id"] as string,
            ProductName = reader.GetString(reader.GetOrdinal("product_name")),
            NormalizedProductName = reader.GetString(reader.GetOrdinal("normalized_product_name")),
            ActiveSubstanceText = reader["active_substance_text"] as string,
            Strength = reader["strength"] as string,
            PharmaceuticalForm = reader["pharmaceutical_form"] as string,
            MarketingAuthorizationHolder = reader["marketing_authorization_holder"] as string,
            AuthorizationNumber = reader["authorization_number"] as string,
            AuthorizationValidity = reader["authorization_validity"] as string,
            ProductType = reader["product_type"] as string,
            ProcedureType = reader["procedure_type"] as string,
            ChplUrl = reader["chpl_url"] as string,
            LeafletUrl = reader["leaflet_url"] as string,
            Source = reader["source"] as string ?? "RPL",
            SourceVersion = reader["source_version"] as string,
            ImportedAt = reader.GetDateTime(reader.GetOrdinal("imported_at"))
        };
    }
}
