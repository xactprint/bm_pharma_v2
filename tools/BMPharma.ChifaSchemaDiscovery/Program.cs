using Npgsql;
using System;
using System.Threading.Tasks;

const string DEFAULT_TEST_CONN = "Host=localhost;Port=5433;Database=CHIFA_OFFICINE;Username=pharm;Password=pharm;TrustServerCertificate=true;Timeout=5;CommandTimeout=10;";
const string DEFAULT_REAL_CONN = "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;TrustServerCertificate=true;Timeout=5;CommandTimeout=10;";

string connectionString;
string targetLabel;

if (args.Length == 0)
{
    Console.WriteLine("Usage: BMPharma.ChifaSchemaDiscovery <target> [label]");
    Console.WriteLine();
    Console.WriteLine("Targets:");
    Console.WriteLine("  test     Use Docker test PostgreSQL (port 5433)");
    Console.WriteLine("  real     Use real CHIFA-OFFICINE PostgreSQL (port 5432)");
    Console.WriteLine("  <conn>   Custom Npgsql connection string");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run -- test");
    Console.WriteLine("  dotnet run -- real");
    Console.WriteLine("  dotnet run -- \"Host=myserver;Port=5432;Database=DB;Username=U;TrustServerCertificate=true\" MYTARGET");
    return;
}

if (args[0].Equals("test", StringComparison.OrdinalIgnoreCase))
{
    connectionString = DEFAULT_TEST_CONN;
    targetLabel = "TEST (Docker PostgreSQL :5433)";
}
else if (args[0].Equals("real", StringComparison.OrdinalIgnoreCase))
{
    connectionString = DEFAULT_REAL_CONN;
    targetLabel = "REAL (CHIFA-OFFICINE PostgreSQL :5432)";
}
else
{
    connectionString = args[0];
    targetLabel = args.Length >= 2 ? args[1] : "CUSTOM";
}

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  BM PHARMA — CHIFA Schema Discovery (READ-ONLY ONLY)          ║");
Console.WriteLine("║  Date: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + "                                  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"  Target: {targetLabel}");
Console.WriteLine($"  Connection: { connectionString.Replace("Password=pharm", "Password=***") }");
Console.WriteLine();

try
{
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    Console.WriteLine("✅ CONNECTION: SUCCESS");
    Console.WriteLine($"   Server Version: {conn.ServerVersion}");
    Console.WriteLine($"   Database: {conn.Database}");
    Console.WriteLine($"   ProcessID: {conn.ProcessID}");
    Console.WriteLine();

    // 1. PostgreSQL Version
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  1. POSTGRESQL VERSION & SETTINGS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, "SELECT version() AS pg_version, current_setting('server_version_num') AS version_num, current_setting('server_encoding') AS encoding, current_setting('max_identifier_length') AS max_id_len");

    Console.WriteLine();

    // 2. All tables
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  2. ALL TABLES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT table_name,
               (SELECT count(*) FROM information_schema.columns c WHERE c.table_name = t.table_name AND c.table_schema = 'public') AS columns
        FROM information_schema.tables t
        WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
        ORDER BY table_name");

    Console.WriteLine();

    // 3. Row counts
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  3. TABLE ROW COUNTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre" })
    {
        await Run(conn, $"SELECT '{table}' AS table_name, count(*) AS row_count FROM {table}");
    }

    Console.WriteLine();

    // 4-7. Columns for each critical table
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre" })
    {
        var sectionNum = table switch
        {
            "facture" => "4",
            "detail_fact" => "5",
            "bordereau" => "6",
            "parametre" => "7",
            _ => "?"
        };
        Console.WriteLine($"═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  {sectionNum}. {table.ToUpper()} — COLUMNS, TYPES, NULLS, DEFAULTS");
        Console.WriteLine($"═══════════════════════════════════════════════════════════════");
        await Run(conn, $@"
            SELECT column_name, data_type,
                   character_maximum_length, numeric_precision, numeric_scale,
                   is_nullable, column_default,
                   CASE WHEN column_default LIKE 'nextval%' THEN 'AUTO-INC' ELSE '' END AS extra
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = '{table}'
            ORDER BY ordinal_position");
        Console.WriteLine();
    }

    // 8. Primary Keys
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  8. PRIMARY KEYS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT tc.table_name, tc.constraint_name, kcu.column_name, kcu.ordinal_position
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
            ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
        WHERE tc.table_schema = 'public' AND tc.constraint_type = 'PRIMARY KEY'
        ORDER BY tc.table_name, kcu.ordinal_position");

    Console.WriteLine();

    // 9. Foreign Keys
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  9. FOREIGN KEYS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT
            tc.table_name AS source_table,
            kcu.column_name AS source_column,
            ccu.table_name AS target_table,
            ccu.column_name AS target_column,
            tc.constraint_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
            ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
        JOIN information_schema.constraint_column_usage ccu
            ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema
        WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_schema = 'public'
        ORDER BY tc.table_name");

    Console.WriteLine();

    // 10. Indexes
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  10. INDEXES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT indexname, tablename, indexdef
        FROM pg_indexes
        WHERE schemaname = 'public' AND tablename IN ('facture','detail_fact','bordereau','parametre')
        ORDER BY tablename, indexname");

    Console.WriteLine();

    // 11. Sequences
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  11. SEQUENCES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT sequence_name, data_type, start_value, minimum_value, maximum_value, increment_by, cycle
        FROM information_schema.sequences
        WHERE sequence_schema = 'public'
        ORDER BY sequence_name");

    Console.WriteLine();

    // 12. CHECK constraints
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  12. CHECK CONSTRAINTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT conname, conrelid::regclass AS table_name, pg_get_constraintdef(oid) AS definition
        FROM pg_constraint
        WHERE contype = 'c' AND connamespace = 'public'::regnamespace
        ORDER BY conrelid::regclass::text, conname");

    Console.WriteLine();

    // 13. Views
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  13. VIEWS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, "SELECT table_name FROM information_schema.views WHERE table_schema = 'public' ORDER BY table_name");

    Console.WriteLine();

    // 14. Triggers
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  14. TRIGGERS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT trigger_name, event_manipulation, event_object_table, action_timing
        FROM information_schema.triggers
        WHERE trigger_schema = 'public'
        ORDER BY event_object_table, trigger_name");

    Console.WriteLine();

    // 15. parametre current values
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  15. PARAMETRE — CRITICAL COUNTER VALUES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, "SELECT code_ps, code_centre, nom_pharmacie, next_num_fact, next_num_bord FROM parametre");

    Console.WriteLine();

    // 16. Sample facture (last row)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  16. SAMPLE FACTURE (last 1)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT num_fact, date_fact, etat, num_bord, mont_off, mont_as, mont_fact,
               num_assure, code_centre, type_maj, mont_maj_fae, mont_maj, mont_mut,
               date_fin_mut, nat_remb, version, echifa, e_ord
        FROM facture
        ORDER BY num_fact DESC LIMIT 1");

    Console.WriteLine();

    // 17. Sample bordereau (last row)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  17. SAMPLE BORDEREAU (last 1)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT id_bord, num_bord, code_centre, etat, mont_vir, duplicata,
               date_cloture, date_ouverture, date_depot_ftp
        FROM bordereau
        ORDER BY id_bord DESC LIMIT 1");

    Console.WriteLine();

    // 18. Sample detail_fact (last 3)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  18. SAMPLE DETAIL_FACT (last 3)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT num_fact, num_enr, ppa, qte, mont, mont_as, mont_pharm,
               num_enr_prescrit, num_lot, remboursable, local, posologie
        FROM detail_fact
        ORDER BY num_fact DESC, num_enr DESC LIMIT 3");

    Console.WriteLine();

    // 19. FK relationship facture.num_bord → bordereau.num_bord
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  19. FK: facture.num_bord → bordereau.num_bord");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT f.num_fact, f.num_bord, b.num_bord AS bord_num_bord, b.etat AS bord_etat
        FROM facture f
        LEFT JOIN bordereau b ON f.num_bord = b.num_bord
        WHERE f.num_bord IS NOT NULL AND f.num_bord != ''
        ORDER BY f.num_fact DESC LIMIT 5");

    Console.WriteLine();

    // 20. NULL stats for critical facture columns
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  20. NULL STATS — CRITICAL FACTURE COLUMNS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT
            count(*) AS total_rows,
            count(type_maj) AS type_maj_not_null,
            count(mont_maj_fae) AS mont_maj_fae_not_null,
            count(mont_maj) AS mont_maj_not_null,
            count(num_bord) AS num_bord_not_null,
            count(mont_fact) AS mont_fact_not_null,
            count(num_assure) AS num_assure_not_null
        FROM facture");

    Console.WriteLine();

    // 21. BM-SPEC-028/029/031 Validation
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  21. BM-SPEC VALIDATION");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT 'BM-SPEC-028: num_fact max 8 chars' AS check_name,
               max(length(num_fact)) AS actual_max_len,
               CASE WHEN max(length(num_fact)) <= 8 THEN 'PASS' ELSE 'FAIL' END AS result
        FROM facture WHERE num_fact IS NOT NULL");

    await Run(conn, @"
        SELECT 'BM-SPEC-029: mont_maj_fae NOT NULL' AS check_name,
               count(*) FILTER (WHERE mont_maj_fae IS NULL) AS null_count,
               CASE WHEN count(*) FILTER (WHERE mont_maj_fae IS NULL) = 0 THEN 'PASS' ELSE 'FAIL' END AS result
        FROM facture");

    await Run(conn, @"
        SELECT 'BM-SPEC-029: mont_maj NOT NULL' AS check_name,
               count(*) FILTER (WHERE mont_maj IS NULL) AS null_count,
               CASE WHEN count(*) FILTER (WHERE mont_maj IS NULL) = 0 THEN 'PASS' ELSE 'FAIL' END AS result
        FROM facture");

    await Run(conn, @"
        SELECT 'BM-SPEC-029: type_maj NOT NULL' AS check_name,
               count(*) FILTER (WHERE type_maj IS NULL) AS null_count,
               CASE WHEN count(*) FILTER (WHERE type_maj IS NULL) = 0 THEN 'PASS' ELSE 'FAIL' END AS result
        FROM facture");

    await Run(conn, @"
        SELECT 'BM-SPEC-031: next_num_bord type' AS check_name,
               data_type,
               CASE WHEN data_type IN ('integer','smallint') THEN 'PASS' ELSE 'FAIL' END AS result
        FROM information_schema.columns
        WHERE table_name = 'parametre' AND column_name = 'next_num_bord'");

    await Run(conn, @"
        SELECT 'BM-SPEC-031: next_num_fact type' AS check_name,
               data_type,
               CASE WHEN data_type IN ('integer','smallint','bigint') THEN 'PASS' ELSE 'FAIL' END AS result
        FROM information_schema.columns
        WHERE table_name = 'parametre' AND column_name = 'next_num_fact'");

    Console.WriteLine();

    // 22. READ-ONLY verification (confirm SELECTs work)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  22. READ-ONLY VERIFICATION");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre" })
    {
        try
        {
            await using var cmd = new NpgsqlCommand($"SELECT count(*) FROM {table}", conn);
            var result = await cmd.ExecuteScalarAsync();
            Console.WriteLine($"  ✅ SELECT count(*) FROM {table} => {result} rows");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ SELECT count(*) FROM {table} => {ex.Message}");
        }
    }
    Console.WriteLine();

    // 23. EF Core Column Mapping Matrix
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  23. EF CORE COLUMN MAPPING MATRIX");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine();
    Console.WriteLine("  Table        | Column             | PG Type        | PG Nullable | PG Default          | EF Type     | Match?");
    Console.WriteLine("  -------------|--------------------|----------------|-------------|---------------------|-------------|-------");

    var efMappings = new (string table, string column, string efType, string efNullable)[]
    {
        ("facture", "num_fact", "string", "NO"),
        ("facture", "date_fact", "DateTime?", "YES"),
        ("facture", "etat", "string?", "YES"),
        ("facture", "num_bord", "string?", "YES"),
        ("facture", "mont_off", "decimal?", "YES"),
        ("facture", "mont_as", "decimal?", "YES"),
        ("facture", "mont_fact", "decimal?", "YES"),
        ("facture", "num_assure", "string?", "YES"),
        ("facture", "code_centre", "string?", "YES"),
        ("facture", "type_maj", "int", "NO"),
        ("facture", "mont_maj_fae", "decimal", "NO"),
        ("facture", "mont_maj", "decimal", "NO"),
        ("facture", "nat_remb", "string?", "YES"),
        ("facture", "mont_mut", "decimal?", "YES"),
        ("facture", "date_fin_mut", "DateTime?", "YES"),
        ("facture", "version", "string?", "YES"),
        ("facture", "num_serie_ps", "long?", "YES"),
        ("facture", "version_carte", "int?", "YES"),
        ("facture", "echifa", "bool?", "YES"),
        ("facture", "id_fact_echifa", "long?", "YES"),
        ("facture", "e_ord", "bool?", "YES"),
        ("facture", "id_e_ord", "long?", "YES"),
        ("detail_fact", "num_fact", "string", "NO"),
        ("detail_fact", "num_enr", "string", "NO"),
        ("detail_fact", "ppa", "decimal", "NO"),
        ("detail_fact", "qte", "decimal", "NO"),
        ("detail_fact", "mont", "decimal", "NO"),
        ("detail_fact", "mont_as", "decimal?", "YES"),
        ("detail_fact", "mont_pharm", "decimal?", "YES"),
        ("detail_fact", "num_enr_prescrit", "string", "NO"),
        ("detail_fact", "num_lot", "string?", "YES"),
        ("detail_fact", "remboursable", "bool?", "YES"),
        ("detail_fact", "local", "bool?", "YES"),
        ("detail_fact", "inf_tr", "bool?", "YES"),
        ("detail_fact", "applic_tr", "bool?", "YES"),
        ("detail_fact", "medic", "bool?", "YES"),
        ("detail_fact", "ts", "bool?", "YES"),
        ("bordereau", "id_bord", "long", "NO"),
        ("bordereau", "num_bord", "string", "NO"),
        ("bordereau", "code_centre", "string", "NO"),
        ("bordereau", "etat", "string?", "YES"),
        ("bordereau", "mont_vir", "decimal?", "YES"),
        ("bordereau", "duplicata", "bool?", "YES"),
        ("bordereau", "date_cloture", "DateTime?", "YES"),
        ("bordereau", "date_ouverture", "DateTime?", "YES"),
        ("bordereau", "date_depot_ftp", "DateTime?", "YES"),
        ("bordereau", "id_user_cloture", "int?", "YES"),
        ("bordereau", "poste_cloture", "string?", "YES"),
        ("parametre", "code_ps", "string?", "YES"),
        ("parametre", "code_centre", "string?", "YES"),
        ("parametre", "nom_pharmacie", "string?", "YES"),
        ("parametre", "next_num_fact", "int?", "YES"),
        ("parametre", "next_num_bord", "short?", "YES"),
    };

    foreach (var (table, column, efType, efNullable) in efMappings)
    {
        try
        {
            await using var cmd = new NpgsqlCommand(@"
                SELECT data_type, is_nullable, column_default,
                       character_maximum_length, numeric_precision, numeric_scale
                FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @t AND column_name = @c", conn);
            cmd.Parameters.AddWithValue("@t", table);
            cmd.Parameters.AddWithValue("@c", column);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var pgType = reader.GetString(0);
                var pgNullable = reader.GetString(1) == "YES" ? "YES" : "NO";
                var pgDefault = reader.IsDBNull(2) ? "NULL" : reader.GetString(2);
                var charLen = reader.IsDBNull(3) ? "" : $"({reader.GetInt32(3)})";
                var numPrec = reader.IsDBNull(4) ? "" : $"({reader.GetInt32(4)},{reader.GetInt32(5)})";

                var pgTypeDisplay = pgType switch
                {
                    "character varying" => $"varchar{charLen}",
                    "character" => $"char{charLen}",
                    "numeric" => $"numeric{numPrec}",
                    "integer" => "integer",
                    "smallint" => "smallint",
                    "bigint" => "bigint",
                    "boolean" => "boolean",
                    "timestamp without time zone" => "timestamp",
                    "timestamp with time zone" => "timestamptz",
                    "date" => "date",
                    _ => pgType
                };

                var nullableMatch = (pgNullable == "YES" && efNullable == "YES") || (pgNullable == "NO" && efNullable == "NO");
                var match = nullableMatch ? "✅" : "❌ NULL MISMATCH";

                Console.WriteLine($"  {table,-13}| {column,-18}| {pgTypeDisplay,-14}| {pgNullable,-11}| {pgDefault,-19}| {efType,-11}| {match}");
            }
            else
            {
                Console.WriteLine($"  {table,-13}| {column,-18}| {"MISSING",-14}| {"?",-11}| {"?",-19}| {efType,-11}| ❌ COLUMN NOT FOUND");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  {table,-13}| {column,-18}| ERROR: {ex.Message.Substring(0, Math.Min(60, ex.Message.Length))}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  SCHEMA DISCOVERY COMPLETE — Target: {targetLabel}");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");

    await conn.CloseAsync();
}
catch (NpgsqlException ex)
{
    Console.WriteLine($"❌ CONNECTION FAILED: {ex.Message}");
    Console.WriteLine($"   Error Code: {ex.ErrorCode}");
    Console.WriteLine($"   SQL State: {ex.SqlState}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR: {ex.GetType().Name}: {ex.Message}");
}

static async Task Run(NpgsqlConnection conn, string sql)
{
    try
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.CommandTimeout = 10;
        await using var reader = await cmd.ExecuteReaderAsync();

        var fieldCount = reader.FieldCount;
        var headers = new string[fieldCount];
        var widths = new int[fieldCount];

        for (int i = 0; i < fieldCount; i++)
        {
            headers[i] = reader.GetName(i);
            widths[i] = Math.Max(headers[i].Length, 4);
        }

        var rows = new System.Collections.Generic.List<string[]>();
        while (await reader.ReadAsync())
        {
            var row = new string[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                row[i] = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "";
                widths[i] = Math.Max(widths[i], Math.Min(row[i].Length, 60));
            }
            rows.Add(row);
        }

        if (rows.Count == 0) { Console.WriteLine("  (no rows)"); return; }

        var hdr = "  ";
        var sep = "  ";
        for (int i = 0; i < fieldCount; i++)
        {
            hdr += headers[i].PadRight(widths[i] + 2);
            sep += new string('─', widths[i] + 2);
        }
        Console.WriteLine(hdr);
        Console.WriteLine(sep);

        foreach (var row in rows)
        {
            var line = "  ";
            for (int i = 0; i < fieldCount; i++)
            {
                var val = row[i].Length > 60 ? row[i][..57] + "..." : row[i];
                line += val.PadRight(widths[i] + 2);
            }
            Console.WriteLine(line);
        }
        Console.WriteLine($"  ({rows.Count} row{(rows.Count != 1 ? "s" : "")})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ❌ {ex.Message.Substring(0, Math.Min(100, ex.Message.Length))}");
    }
}
