using Npgsql;
using System;
using System.Threading.Tasks;

const string DEFAULT_TEST_CONN = "Host=localhost;Port=5433;Database=CHIFA_OFFICINE;Username=pharm;Password=pharm;TrustServerCertificate=true;SslMode=Disable;Timeout=5;CommandTimeout=10;";
const string DEFAULT_REAL_CONN = "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=postgres;SslMode=Disable;TrustServerCertificate=true;Timeout=5;CommandTimeout=10;";

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
               (SELECT count(*) FROM information_schema.columns c WHERE c.table_name = t.table_name AND c.table_schema = 'public') AS columns,
               pg_size_size(pg_total_relation_size(quote_ident(table_name))) AS total_size
        FROM information_schema.tables t
        WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
        ORDER BY table_name");

    Console.WriteLine();

    // 3. Row counts for all critical tables
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  3. TABLE ROW COUNTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre", "medicament", "ln", "signature", "forme", "specialite", "tarif", "centre", "utilisateur", "medic_sp", "medic_demuni", "beneficiaire" })
    {
        await Run(conn, $"SELECT '{table}' AS table_name, count(*) AS row_count FROM {table}");
    }

    Console.WriteLine();

    // 4-9. Columns for each core table
    var sectionNum = 4;
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre", "medicament", "ln" })
    {
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
        sectionNum++;
    }

    // 10. Primary Keys
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. PRIMARY KEYS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT tc.table_name, tc.constraint_name, kcu.column_name, kcu.ordinal_position
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
            ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
        WHERE tc.table_schema = 'public' AND tc.constraint_type = 'PRIMARY KEY'
        ORDER BY tc.table_name, kcu.ordinal_position");

    Console.WriteLine();
    sectionNum++;

    // 11. Foreign Keys
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. FOREIGN KEYS");
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
    sectionNum++;

    // 12. Indexes for core tables
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. INDEXES (Core Tables)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT indexname, tablename, indexdef
        FROM pg_indexes
        WHERE schemaname = 'public' AND tablename IN ('facture','detail_fact','bordereau','parametre','medicament','ln')
        ORDER BY tablename, indexname");

    Console.WriteLine();
    sectionNum++;

    // 13. Sequences
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. SEQUENCES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT sequence_name, data_type, start_value, minimum_value, maximum_value, increment_by, cycle
        FROM information_schema.sequences
        WHERE sequence_schema = 'public'
        ORDER BY sequence_name");

    Console.WriteLine();
    sectionNum++;

    // 14. Functions
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. FUNCTIONS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT routine_name, routine_type, data_type AS return_type
        FROM information_schema.routines
        WHERE routine_schema = 'public'
        ORDER BY routine_name");

    Console.WriteLine();
    sectionNum++;

    // 15. parametre current values
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. PARAMETRE — CRITICAL COUNTER VALUES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, "SELECT code_ps, code_centre, nom_pharmacie, next_num_fact, next_num_bord, version, annee FROM parametre");

    Console.WriteLine();
    sectionNum++;

    // 16. Medicament sample
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. MEDICAMENT — SAMPLE (first 5)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT num_enr, nom_com, nom_dci, dosage, tarif_ref, taux, remboursable, generic
        FROM medicament ORDER BY num_enr LIMIT 5");

    Console.WriteLine();
    sectionNum++;

    // 17. LN sample
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. LN — STRUCTURE & SAMPLE");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await Run(conn, @"
        SELECT column_name, data_type, character_maximum_length
        FROM information_schema.columns
        WHERE table_name = 'ln' AND table_schema = 'public'
        ORDER BY ordinal_position");

    Console.WriteLine();
    sectionNum++;

    // 18. READ-ONLY verification
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. READ-ONLY VERIFICATION");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre", "medicament", "ln", "signature" })
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

    // 19. EF Core Column Mapping Matrix
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine($"  {sectionNum}. EF CORE COLUMN MAPPING VALIDATION");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine();
    Console.WriteLine("  Validating critical columns exist in real DB...");

    var criticalColumns = new (string table, string column)[]
    {
        ("facture", "num_fact"), ("facture", "date_fact"), ("facture", "etat"),
        ("facture", "rang_ad"), ("facture", "taux"), ("facture", "id_user"),
        ("facture", "statut_fact"), ("facture", "signature"), ("facture", "fact_xml"),
        ("detail_fact", "num_fact"), ("detail_fact", "num_enr"), ("detail_fact", "ppa"),
        ("bordereau", "id_bord"), ("bordereau", "num_bord"),
        ("parametre", "code_ps"), ("parametre", "next_num_fact"), ("parametre", "next_num_bord"),
        ("parametre", "nom"), ("parametre", "prenom"), ("parametre", "access_token"),
        ("medicament", "num_enr"), ("medicament", "nom_com"), ("medicament", "tarif_ref"),
    };

    foreach (var (table, column) in criticalColumns)
    {
        try
        {
            await using var cmd = new NpgsqlCommand(@"
                SELECT count(*) FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @t AND column_name = @c", conn);
            cmd.Parameters.AddWithValue("@t", table);
            cmd.Parameters.AddWithValue("@c", column);
            var exists = (long)(await cmd.ExecuteScalarAsync()!);
            Console.WriteLine($"  {(exists > 0 ? "✅" : "❌")} {table}.{column}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ {table}.{column} — ERROR: {ex.Message}");
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
        cmd.CommandTimeout = 15;
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
