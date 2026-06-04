using DrugCompare.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DrugCompare.Services;

public sealed class PostgresDrugDataService : IDrugDataService
{
    private readonly string _connectionString;

    public PostgresDrugDataService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DefaultConnection connection string.");
    }

    public async Task<DrugLookupResult?> FindDrugAsync(string drugName)
    {
        if (string.IsNullOrWhiteSpace(drugName))
            return null;

        var normalizedSearch = Normalize(drugName);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT
                d.name AS drug_name,
                d.manufacturer,
                s.id AS substance_id,
                s.name AS substance_name,
                s.normalized_name,
                s.ddinter_id,
                s.source
            FROM drugs d
            JOIN drug_active_substances das
                ON das.drug_id = d.id
            JOIN active_substances s
                ON s.id = das.active_substance_id
            WHERE d.normalized_name ILIKE '%' || @search || '%'
            ORDER BY d.name, s.name
            LIMIT 50;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("search", normalizedSearch);

        await using var reader = await command.ExecuteReaderAsync();

        var substances = new List<ActiveSubstanceItem>();
        string? foundDrugName = null;

        while (await reader.ReadAsync())
        {
            foundDrugName ??= reader.GetString(reader.GetOrdinal("drug_name"));

            substances.Add(new ActiveSubstanceItem
            {
                DatabaseId = reader.GetInt64(reader.GetOrdinal("substance_id")),
                Name = reader.GetString(reader.GetOrdinal("substance_name")),
                NormalizedName = reader.GetString(reader.GetOrdinal("normalized_name")),
                DDInterId = reader.IsDBNull(reader.GetOrdinal("ddinter_id"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ddinter_id")),
                Source = reader.IsDBNull(reader.GetOrdinal("source"))
                    ? "PostgreSQL"
                    : reader.GetString(reader.GetOrdinal("source"))
            });
        }

        if (foundDrugName is null || substances.Count == 0)
            return null;

        return new DrugLookupResult
        {
            DrugName = foundDrugName,
            ActiveSubstances = substances
                .GroupBy(x => x.DatabaseId)
                .Select(x => x.First())
                .ToList()
        };
    }

    public async Task<ActiveSubstanceItem?> FindActiveSubstanceAsync(string substanceName)
    {
        if (string.IsNullOrWhiteSpace(substanceName))
            return null;

        var normalizedSearch = Normalize(substanceName);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT
                id,
                name,
                normalized_name,
                ddinter_id,
                source
            FROM active_substances
            WHERE normalized_name = @search
               OR normalized_name ILIKE '%' || @search || '%'
            ORDER BY
                CASE
                    WHEN normalized_name = @search THEN 0
                    ELSE 1
                END,
                name
            LIMIT 1;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("search", normalizedSearch);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new ActiveSubstanceItem
            {
                DatabaseId = reader.GetInt64(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                NormalizedName = reader.GetString(reader.GetOrdinal("normalized_name")),
                DDInterId = reader.IsDBNull(reader.GetOrdinal("ddinter_id"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ddinter_id")),
                Source = reader.IsDBNull(reader.GetOrdinal("source"))
                    ? "PostgreSQL"
                    : reader.GetString(reader.GetOrdinal("source"))
            };
        }

        return new ActiveSubstanceItem
        {
            DatabaseId = null,
            Name = substanceName.Trim(),
            NormalizedName = normalizedSearch,
            DDInterId = null,
            Source = "Manual - not found in database"
        };
    }

    public async Task<List<InteractionResult>> CheckInteractionsAsync(
        IReadOnlyCollection<ActiveSubstanceItem> substances)
    {
        var items = substances
            .Where(x => x.DatabaseId.HasValue)
            .GroupBy(x => x.DatabaseId!.Value)
            .Select(x => x.First())
            .ToList();

        if (items.Count < 2)
            return new List<InteractionResult>();

        var results = new List<InteractionResult>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT
                sa.name AS substance_a,
                sb.name AS substance_b,
                si.severity,
                si.source
            FROM substance_interactions si
            JOIN active_substances sa
                ON sa.id = si.substance_a_id
            JOIN active_substances sb
                ON sb.id = si.substance_b_id
            WHERE si.substance_a_id = @first_id
              AND si.substance_b_id = @second_id
            LIMIT 1;
            """;

        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                var id1 = items[i].DatabaseId!.Value;
                var id2 = items[j].DatabaseId!.Value;

                var firstId = Math.Min(id1, id2);
                var secondId = Math.Max(id1, id2);

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("first_id", firstId);
                command.Parameters.AddWithValue("second_id", secondId);

                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    continue;

                var severity = reader.GetString(reader.GetOrdinal("severity"));

                results.Add(new InteractionResult
                {
                    SubstanceA = reader.GetString(reader.GetOrdinal("substance_a")),
                    SubstanceB = reader.GetString(reader.GetOrdinal("substance_b")),
                    Severity = severity,
                    Message = BuildMessage(severity),
                    Source = reader.IsDBNull(reader.GetOrdinal("source"))
                        ? "Local DDInter-based database"
                        : reader.GetString(reader.GetOrdinal("source"))
                });
            }
        }

        return results
            .OrderByDescending(x => GetSeverityScore(x.Severity))
            .ToList();
    }

    private static string BuildMessage(string severity)
    {
        return severity switch
        {
            "Contraindicated" =>
                "Interaction found. Physician should verify this combination before use.",

            "Major" =>
                "Major interaction found. Physician should verify this interaction clinically.",

            "Moderate" =>
                "Moderate interaction found. Clinical verification is recommended.",

            "Minor" =>
                "Minor interaction found. Review if clinically relevant.",

            _ =>
                "Interaction found in local DDInter-based database. Verify clinically."
        };
    }

    private static int GetSeverityScore(string severity)
    {
        return severity switch
        {
            "Contraindicated" => 4,
            "Major" => 3,
            "Moderate" => 2,
            "Minor" => 1,
            _ => 0
        };
    }

    private static string Normalize(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace("_", " ")
            .Replace("-", " ");
    }
}