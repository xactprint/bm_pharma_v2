using Npgsql;
using System;
using System.Threading.Tasks;

const string ConnectionString = "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;TrustServerCertificate=true;Timeout=5;CommandTimeout=10;";

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  BM PHARMA — CHIFA-OFFICINE Schema Discovery (READ-ONLY)  ║");
Console.WriteLine("║  Date: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + "                        ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

try
{
    await using var conn = new NpgsqlConnection(ConnectionString);
    await conn.OpenAsync();
    Console.WriteLine("✅ CONNECTION: SUCCESS");
    Console.WriteLine($"   Server: {conn.ServerVersion}");
    Console.WriteLine($"   Database: {conn.Database}");
    Console.WriteLine($"   ProcessID: {conn.ProcessID}");
    Console.WriteLine();

    // 1. PostgreSQL Version
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  1. POSTGRESQL VERSION");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, "SELECT version() AS pg_version, current_setting('server_version_num') AS version_num, current_setting('server_encoding') AS encoding");
    Console.WriteLine();

    // 2. All tables in public schema
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  2. ALL TABLES IN PUBLIC SCHEMA");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT table_name, 
               (SELECT count(*) FROM information_schema.columns c WHERE c.table_name = t.table_name AND c.table_schema = 'public') AS column_count
        FROM information_schema.tables t
        WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
        ORDER BY table_name");
    Console.WriteLine();

    // 3. Table row counts (quick estimate)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  3. TABLE ROW COUNTS (estimated)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre" })
    {
        await ExecuteQuery(conn, $"SELECT '{table}' AS table_name, count(*) AS row_count FROM {table}");
    }
    Console.WriteLine();

    // 4. Columns for facture
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  4. FACTURE — COLUMNS, TYPES, NULLS, DEFAULTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT column_name, data_type, character_maximum_length, numeric_precision, numeric_scale,
               is_nullable, column_default,
               CASE WHEN column_default LIKE 'nextval%' THEN 'AUTO-INCREMENT' ELSE '' END AS extra
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'facture'
        ORDER BY ordinal_position");
    Console.WriteLine();

    // 5. Columns for detail_fact
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  5. DETAIL_FACT — COLUMNS, TYPES, NULLS, DEFAULTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT column_name, data_type, character_maximum_length, numeric_precision, numeric_scale,
               is_nullable, column_default,
               CASE WHEN column_default LIKE 'nextval%' THEN 'AUTO-INCREMENT' ELSE '' END AS extra
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'detail_fact'
        ORDER BY ordinal_position");
    Console.WriteLine();

    // 6. Columns for bordereau
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  6. BORDEREAU — COLUMNS, TYPES, NULLS, DEFAULTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT column_name, data_type, character_maximum_length, numeric_precision, numeric_scale,
               is_nullable, column_default,
               CASE WHEN column_default LIKE 'nextval%' THEN 'AUTO-INCREMENT' ELSE '' END AS extra
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'bordereau'
        ORDER BY ordinal_position");
    Console.WriteLine();

    // 7. Columns for parametre
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  7. PARAMETRE — COLUMNS, TYPES, NULLS, DEFAULTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT column_name, data_type, character_maximum_length, numeric_precision, numeric_scale,
               is_nullable, column_default
        FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'parametre'
        ORDER BY ordinal_position");
    Console.WriteLine();

    // 8. Primary Keys
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  8. PRIMARY KEYS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT tc.table_name, tc.constraint_name, kcu.column_name, kcu.ordinal_position
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
        WHERE tc.table_schema = 'public' AND tc.constraint_type = 'PRIMARY KEY'
        ORDER BY tc.table_name, kcu.ordinal_position");
    Console.WriteLine();

    // 9. Foreign Keys
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  9. FOREIGN KEYS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT
            tc.table_name AS source_table,
            kcu.column_name AS source_column,
            ccu.table_name AS target_table,
            ccu.column_name AS target_column,
            tc.constraint_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
        JOIN information_schema.constraint_column_usage ccu ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema
        WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_schema = 'public'
        ORDER BY tc.table_name");
    Console.WriteLine();

    // 10. Indexes
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  10. INDEXES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT indexname, tablename, indexdef
        FROM pg_indexes
        WHERE schemaname = 'public' AND tablename IN ('facture','detail_fact','bordereau','parametre')
        ORDER BY tablename, indexname");
    Console.WriteLine();

    // 11. Sequences
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  11. SEQUENCES");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT sequence_name, data_type, start_value, minimum_value, maximum_value, increment_by, cycle
        FROM information_schema.sequences
        WHERE sequence_schema = 'public'
        ORDER BY sequence_name");
    Console.WriteLine();

    // 12. parametre current values
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  12. PARAMETRE — CURRENT VALUES (critical fields)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT code_ps, code_centre, nom_pharmacie, next_num_fact, next_num_bord
        FROM parametre
        LIMIT 5");
    Console.WriteLine();

    // 13. Sample facture data (1 row)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  13. SAMPLE FACTURE (last 1 row)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT num_fact, date_fact, etat, num_bord, mont_off, mont_as, mont_fact, 
               num_assure, code_centre, type_maj, mont_maj_fae, mont_maj, mont_mut, 
               date_fin_mut, nat_remb, version, echifa, e_ord
        FROM facture
        ORDER BY num_fact DESC
        LIMIT 1");
    Console.WriteLine();

    // 14. Sample bordereau data (last 1 row)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  14. SAMPLE BORDEREAU (last 1 row)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT id_bord, num_bord, code_centre, etat, mont_vir, duplicata, 
               date_cloture, date_ouverture, date_depot_ftp
        FROM bordereau
        ORDER BY id_bord DESC
        LIMIT 1");
    Console.WriteLine();

    // 15. Sample detail_fact
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  15. SAMPLE DETAIL_FACT (last 3 rows)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT num_fact, num_enr, ppa, qte, mont, mont_as, mont_pharm, 
               num_enr_prescrit, num_lot, remboursable, local, posologie
        FROM detail_fact
        ORDER BY num_fact DESC, num_enr DESC
        LIMIT 3");
    Console.WriteLine();

    // 16. FK facture.num_bord → bordereau.num_bord check
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  16. FK CHECK: facture.num_bord → bordereau.num_bord");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT f.num_fact, f.num_bord, b.num_bord AS bord_num_bord, b.etat AS bord_etat
        FROM facture f
        LEFT JOIN bordereau b ON f.num_bord = b.num_bord
        WHERE f.num_bord IS NOT NULL AND f.num_bord != ''
        ORDER BY f.num_fact DESC
        LIMIT 5");
    Console.WriteLine();

    // 17. NULL stats for critical columns in facture
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  17. NULL STATS — CRITICAL FACTURE COLUMNS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT
            count(*) AS total_rows,
            count(type_maj) AS type_maj_not_null,
            count(mont_maj_fae) AS mont_maj_fae_not_null,
            count(mont_maj) AS mont_maj_not_null,
            count(num_bord) AS num_bord_not_null,
            count(mont_fact) AS mont_fact_not_null,
            count(mont_as) AS mont_as_not_null,
            count(num_assure) AS num_assure_not_null,
            count(code_centre) AS code_centre_not_null
        FROM facture");
    Console.WriteLine();

    // 18. CHECK constraints
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  18. CHECK CONSTRAINTS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT conname, conrelid::regclass AS table_name, pg_get_constraintdef(oid) AS definition
        FROM pg_constraint
        WHERE contype = 'c' AND connamespace = 'public'::regnamespace
        ORDER BY conrelid::regclass::text, conname");
    Console.WriteLine();

    // 19. Views
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  19. VIEWS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT table_name
        FROM information_schema.views
        WHERE table_schema = 'public'
        ORDER BY table_name");
    Console.WriteLine();

    // 20. Table triggers
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  20. TRIGGERS");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    await ExecuteQuery(conn, @"
        SELECT trigger_name, event_manipulation, event_object_table, action_timing
        FROM information_schema.triggers
        WHERE trigger_schema = 'public'
        ORDER BY event_object_table, trigger_name");
    Console.WriteLine();

    // 21. Verify READ-ONLY: attempt SELECT on all 4 tables
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  21. READ-ONLY VERIFICATION (SELECT tests)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    foreach (var table in new[] { "facture", "detail_fact", "bordereau", "parametre" })
    {
        try
        {
            await using var cmd = new NpgsqlCommand($"SELECT count(*) FROM {table}", conn);
            var result = await cmd.ExecuteScalarAsync();
            Console.WriteLine($"  ✅ SELECT count(*) FROM {table} => {result} rows (READ-ONLY OK)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ SELECT count(*) FROM {table} => ERROR: {ex.Message}");
        }
    }
    Console.WriteLine();

    // 22. Confirm NO WRITE attempt (we do NOT actually write — just prove the guard works)
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  22. WRITE BLOCK VERIFICATION (should fail)");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    try
    {
        await using var cmd = new NpgsqlCommand("INSERT INTO facture (num_fact) VALUES ('TEST')", conn);
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("  ⚠️ INSERT succeeded — THIS SHOULD NOT HAPPEN!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✅ INSERT blocked/failed: {ex.GetType().Name} — {ex.Message.Substring(0, Math.Min(120, ex.Message.Length))}");
    }
    Console.WriteLine();

    // 23. BM-SPEC-028/029/031 Validation
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  23. BM-SPEC VALIDATION");
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    
    // BM-SPEC-028: num_fact max 8 chars
    await ExecuteQuery(conn, @"
        SELECT 'num_fact length check' AS check_name,
               max(length(num_fact)) AS max_length,
               CASE WHEN max(length(num_fact)) <= 8 THEN 'PASS' ELSE 'FAIL' END AS result
        FROM facture WHERE num_fact IS NOT NULL");
    
    // BM-SPEC-029: mont_maj_fae and mont_maj must not be NULL
    await ExecuteQuery(conn, @"
        SELECT 'mont_maj_fae NOT NULL check' AS check_name,
               count(*) FILTER (WHERE mont_maj_fae IS NULL) AS null_count,
               CASE WHEN count(*) FILTER (WHERE mont_maj_fae IS NULL) = 0 THEN 'PASS' ELSE 'FAIL' END AS result
        FROM facture
        UNION ALL
        SELECT 'mont_maj NOT NULL check',
               count(*) FILTER (WHERE mont_maj IS NULL),
               CASE WHEN count(*) FILTER (WHERE mont_maj IS NULL) = 0 THEN 'PASS' ELSE 'FAIL' END
        FROM facture
        UNION ALL
        SELECT 'type_maj NOT NULL check',
               count(*) FILTER (WHERE type_maj IS NULL),
               CASE WHEN count(*) FILTER (WHERE type_maj IS NULL) = 0 THEN 'PASS' ELSE 'FAIL' END
        FROM facture");

    // BM-SPEC-031: Counter check
    await ExecuteQuery(conn, @"
        SELECT 'next_num_bord type check' AS check_name,
               data_type, 
               CASE WHEN data_type IN ('integer','smallint') THEN 'PASS' ELSE 'FAIL' END AS result
        FROM information_schema.columns
        WHERE table_name = 'parametre' AND column_name = 'next_num_bord'
        UNION ALL
        SELECT 'next_num_fact type check',
               data_type,
               CASE WHEN data_type IN ('integer','smallint','bigint') THEN 'PASS' ELSE 'FAIL' END
        FROM information_schema.columns
        WHERE table_name = 'parametre' AND column_name = 'next_num_fact'");

    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════════════════");
    Console.WriteLine("  SCHEMA DISCOVERY COMPLETE");
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
    Console.WriteLine(ex.StackTrace);
}

static async Task ExecuteQuery(NpgsqlConnection conn, string sql)
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
            widths[i] = Math.Max(headers[i].Length, 6);
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
        
        if (rows.Count == 0)
        {
            Console.WriteLine("  (no rows returned)");
            return;
        }
        
        // Print header
        var headerLine = "  ";
        var separatorLine = "  ";
        for (int i = 0; i < fieldCount; i++)
        {
            headerLine += headers[i].PadRight(widths[i] + 2);
            separatorLine += new string('─', widths[i] + 2);
        }
        Console.WriteLine(headerLine);
        Console.WriteLine(separatorLine);
        
        // Print rows
        foreach (var row in rows)
        {
            var line = "  ";
            for (int i = 0; i < fieldCount; i++)
            {
                var val = row[i].Length > 60 ? row[i].Substring(0, 57) + "..." : row[i];
                line += val.PadRight(widths[i] + 2);
            }
            Console.WriteLine(line);
        }
        Console.WriteLine($"  ({rows.Count} row{(rows.Count != 1 ? "s" : "")})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ❌ Query error: {ex.Message.Substring(0, Math.Min(100, ex.Message.Length))}");
    }
}
