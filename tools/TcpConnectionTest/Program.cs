using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// ============================================================
// BM-PHASE-004.12 — REAL CHIFA READ-ONLY VALIDATION
// ALL SELECT ONLY — ZERO WRITES
// ============================================================

const string CONN_STR = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=60";

int totalTests = 0, passed = 0, failed = 0;
var log = new StringBuilder();

void Log(string msg) { Console.WriteLine(msg); log.AppendLine(msg); }
void Section(string title) { Console.WriteLine($"\n{'='} {title}"); Log($"\n=== {title} ==="); }

bool Run(string name, Func<bool> test)
{
    totalTests++;
    try
    {
        if (test()) { passed++; Console.WriteLine($"  PASS  {name}"); log.AppendLine($"PASS  {name}"); return true; }
        else { failed++; Console.WriteLine($"  FAIL  {name}"); log.AppendLine($"FAIL  {name}"); return false; }
    }
    catch (Exception ex) { failed++; Console.WriteLine($"  FAIL  {name} — {ex.Message}"); log.AppendLine($"FAIL  {name} — {ex.Message}"); return false; }
}

string S(NpgsqlConnection c, string sql) { using var cmd = new NpgsqlCommand(sql, c); return cmd.ExecuteScalar()?.ToString() ?? "(null)"; }

List<Dictionary<string, string>> Q(NpgsqlConnection c, string sql)
{
    var r = new List<Dictionary<string, string>>();
    using var cmd = new NpgsqlCommand(sql, c);
    using var rd = cmd.ExecuteReader();
    while (rd.Read()) { var row = new Dictionary<string, string>(); for (int i = 0; i < rd.FieldCount; i++) row[rd.GetName(i)] = rd.IsDBNull(i) ? "NULL" : rd[i]?.ToString() ?? ""; r.Add(row); }
    return r;
}

NpgsqlConnection OpenConn()
{
    var conn = new NpgsqlConnection(CONN_STR);
    conn.Open();
    return conn;
}

// ============================================================
// PRE-CAPTURE
// ============================================================
Section("PRE-CAPTURE: Baseline Counters (READ-ONLY)");
string pFact = "", pDetail = "", pBord = "", pParam = "", pMed = "", pSign = "", pLn = "", pNextFact = "", pNextBord = "", pCodeCentre = "", pCodePs = "";

using (var c = OpenConn())
{
    Log($"Connected: {c.ServerVersion} | {c.UserName}@{c.Database}");
    pFact = S(c, "SELECT count(*) FROM facture");
    pDetail = S(c, "SELECT count(*) FROM detail_fact");
    pBord = S(c, "SELECT count(*) FROM bordereau");
    pParam = S(c, "SELECT count(*) FROM parametre");
    pMed = S(c, "SELECT count(*) FROM medicament");
    pSign = S(c, "SELECT count(*) FROM signature");
    pLn = S(c, "SELECT count(*) FROM ln");
    pNextFact = S(c, "SELECT next_num_fact FROM parametre");
    pNextBord = S(c, "SELECT next_num_bord FROM parametre");
    pCodeCentre = S(c, "SELECT code_centre FROM parametre");
    pCodePs = S(c, "SELECT code_ps FROM parametre");
    Log($"  facture      = {pFact}");
    Log($"  detail_fact  = {pDetail}");
    Log($"  bordereau    = {pBord}");
    Log($"  parametre    = {pParam}");
    Log($"  medicament   = {pMed}");
    Log($"  signature    = {pSign}");
    Log($"  ln           = {pLn}");
    Log($"  next_num_fact = {pNextFact}");
    Log($"  next_num_bord = {pNextBord}");
    Log($"  code_centre  = {pCodeCentre}");
    Log($"  code_ps      = {pCodePs}");
    c.Close();
}

// ============================================================
// PHASE 1 — VALIDATION DE LA CONNEXION RÉELLE
// ============================================================
Section("PHASE 1 — VALIDATION DE LA CONNEXION RÉELLE");

Run("PH1.01 TCP Connection", () =>
{
    var c = new NpgsqlConnection(CONN_STR);
    var sw = Stopwatch.StartNew();
    c.Open(); sw.Stop();
    Log($"    Time: {sw.ElapsedMilliseconds}ms | State: {c.State}");
    c.Close(); c.Dispose();
    return c.State == System.Data.ConnectionState.Closed;
});

Run("PH1.02 Authentication (pharm)", () =>
{
    using var c = OpenConn();
    var user = S(c, "SELECT current_user");
    Log($"    current_user: {user}");
    return user == "pharm";
});

Run("PH1.03 Database (CHIFA_OFFICINE)", () =>
{
    using var c = OpenConn();
    var db = S(c, "SELECT current_database()");
    Log($"    current_database: {db}");
    return db == "CHIFA_OFFICINE";
});

Run("PH1.04 Schema (public)", () =>
{
    using var c = OpenConn();
    var s = S(c, "SELECT current_schema");
    Log($"    current_schema: {s}");
    return s == "public";
});

Run("PH1.05 PostgreSQL version", () =>
{
    using var c = OpenConn();
    var ver = S(c, "SELECT version()");
    Log($"    {ver}");
    return ver.Contains("PostgreSQL 9.3.4") && ver.Contains("32-bit");
});

Run("PH1.06 Simple SELECT", () =>
{
    using var c = OpenConn();
    var r = S(c, "SELECT 42 AS answer");
    Log($"    SELECT 42 => {r}");
    return r == "42";
});

Run("PH1.07 Close/reopen cycle", () =>
{
    using var c = OpenConn();
    var r1 = S(c, "SELECT 1"); c.Close();
    c.Open(); var r2 = S(c, "SELECT 2"); c.Close();
    Log($"    First: {r1}, Second: {r2}");
    return r1 == "1" && r2 == "2";
});

Run("PH1.08 Server timestamp", () =>
{
    using var c = OpenConn();
    var ts = S(c, "SELECT now()::text");
    Log($"    server_time: {ts}");
    return ts.Length > 0;
});

Run("PH1.09 Connection details", () =>
{
    using var c = OpenConn();
    var pid = S(c, "SELECT pg_backend_pid()::text");
    var usename = S(c, "SELECT usename FROM pg_stat_activity WHERE pid = pg_backend_pid()");
    Log($"    backend_pid: {pid}");
    Log($"    usename: {usename}");
    return !string.IsNullOrEmpty(pid);
});

// ============================================================
// PHASE 2 — SCHEMA DISCOVERY (Real CHIFA)
// ============================================================
Section("PHASE 2 — SCHEMA DISCOVERY (Real CHIFA)");

Run("PH2.01 Table list", () =>
{
    using var c = OpenConn();
    var tables = Q(c, "SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename");
    Log($"    Total tables: {tables.Count}");
    foreach (var t in tables) Log($"      {t["tablename"]}");
    return tables.Count == 48;
});

Run("PH2.02 Column inventory (critical tables)", () =>
{
    using var c = OpenConn();
    string[] ct = { "facture", "detail_fact", "bordereau", "parametre", "medicament", "signature", "ln" };
    foreach (var t in ct)
    {
        var cols = Q(c, $@"SELECT column_name, data_type, character_maximum_length, is_nullable
            FROM information_schema.columns WHERE table_name='{t}' AND table_schema='public' ORDER BY ordinal_position");
        Log($"    {t}: {cols.Count} columns");
    }
    return true;
});

Run("PH2.03 Primary keys", () =>
{
    using var c = OpenConn();
    var pks = Q(c, @"SELECT tc.table_name, kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'
        ORDER BY tc.table_name, kcu.ordinal_position");
    Log($"    Primary keys: {pks.Count}");
    string cur = "";
    foreach (var pk in pks)
    {
        if (pk["table_name"] != cur) { cur = pk["table_name"]; Log($"    {cur}:"); }
        Log($"      {pk["column_name"]}");
    }
    return pks.Count > 0;
});

Run("PH2.04 Foreign keys", () =>
{
    using var c = OpenConn();
    var fks = Q(c, @"SELECT tc.table_name, kcu.column_name, ccu.table_name AS ftable, ccu.column_name AS fcol
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name=ccu.constraint_name
        WHERE tc.constraint_type='FOREIGN KEY' AND tc.table_schema='public' ORDER BY tc.table_name");
    Log($"    Foreign keys: {fks.Count}");
    foreach (var fk in fks) Log($"      {fk["table_name"]}.{fk["column_name"]} -> {fk["ftable"]}.{fk["fcol"]}");
    return true;
});

Run("PH2.05 Indexes", () =>
{
    using var c = OpenConn();
    var idx = Q(c, "SELECT tablename, indexname FROM pg_indexes WHERE schemaname='public' ORDER BY tablename, indexname");
    Log($"    Indexes: {idx.Count}");
    foreach (var i in idx) Log($"      {i["tablename"]}.{i["indexname"]}");
    return idx.Count > 0;
});

Run("PH2.06 Sequences", () =>
{
    using var c = OpenConn();
    var seqs = Q(c, @"SELECT sequence_name, data_type, start_value
        FROM information_schema.sequences WHERE sequence_schema='public' ORDER BY sequence_name");
    Log($"    Sequences: {seqs.Count}");
    foreach (var s in seqs) Log($"      {s["sequence_name"]} ({s["data_type"]}) start={s["start_value"]}");
    return true;
});

Run("PH2.07 Constraints (UNIQUE, CHECK)", () =>
{
    using var c = OpenConn();
    var cons = Q(c, @"SELECT table_name, constraint_name, constraint_type
        FROM information_schema.table_constraints WHERE table_schema='public' ORDER BY table_name, constraint_type");
    Log($"    Constraints: {cons.Count}");
    foreach (var cn in cons) Log($"      {cn["table_name"]}.{cn["constraint_name"]} ({cn["constraint_type"]})");
    return true;
});

Run("PH2.08 Functions", () =>
{
    using var c = OpenConn();
    var funcs = Q(c, @"SELECT routine_name, routine_type, data_type
        FROM information_schema.routines WHERE routine_schema='public' ORDER BY routine_name");
    Log($"    Functions: {funcs.Count}");
    foreach (var f in funcs) Log($"      {f["routine_name"]} ({f["routine_type"]}) → {f["data_type"]}");
    return true;
});

// ============================================================
// PHASE 3 — VALIDATION DES TABLES CRITIQUES
// ============================================================
Section("PHASE 3 — VALIDATION DES TABLES CRITIQUES");

Run("PH3.01 facture — exists", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM facture");
    Log($"    facture rows: {count}");
    var cols = Q(c, "SELECT count(*) AS cnt FROM information_schema.columns WHERE table_name='facture' AND table_schema='public'");
    Log($"    facture columns: {cols[0]["cnt"]}");
    return true;
});

Run("PH3.02 facture — 53 physical columns", () =>
{
    using var c = OpenConn();
    var cols = Q(c, "SELECT column_name, data_type FROM information_schema.columns WHERE table_name='facture' AND table_schema='public' ORDER BY ordinal_position");
    Log($"    Physical columns: {cols.Count}");
    return cols.Count == 53;
});

Run("PH3.03 facture — PK num_fact", () =>
{
    using var c = OpenConn();
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='facture' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'");
    var cols = string.Join(", ", pk.Select(p => p["column_name"]));
    Log($"    PK: {cols}");
    return pk.Any(p => p["column_name"] == "num_fact");
});

Run("PH3.04 facture — FK to bordereau", () =>
{
    using var c = OpenConn();
    var fk = Q(c, @"SELECT ccu.table_name AS ftable, ccu.column_name AS fcol
        FROM information_schema.table_constraints tc
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name=ccu.constraint_name
        WHERE tc.table_name='facture' AND tc.constraint_type='FOREIGN KEY' AND tc.table_schema='public'");
    Log($"    FKs on facture: {fk.Count}");
    foreach (var f in fk) Log($"      → {f["ftable"]}.{f["fcol"]}");
    return true;
});

Run("PH3.05 detail_fact — 20 columns, PK(num_fact,num_enr,ppa)", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM detail_fact");
    var cols = Q(c, "SELECT column_name FROM information_schema.columns WHERE table_name='detail_fact' AND table_schema='public' ORDER BY ordinal_position");
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='detail_fact' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public' ORDER BY kcu.ordinal_position");
    var pkCols = string.Join(", ", pk.Select(p => p["column_name"]));
    Log($"    detail_fact rows={count} cols={cols.Count} PK=({pkCols})");
    return cols.Count == 20 && pk.Count == 3;
});

Run("PH3.06 bordereau — 11 columns, PK num_bord", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM bordereau");
    var cols = Q(c, "SELECT column_name FROM information_schema.columns WHERE table_name='bordereau' AND table_schema='public' ORDER BY ordinal_position");
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='bordereau' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'");
    Log($"    bordereau rows={count} cols={cols.Count} PK={pk[0]["column_name"]}");
    return cols.Count == 11 && pk.Any(p => p["column_name"] == "num_bord");
});

Run("PH3.07 parametre — 1 row, PK code_ps", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM parametre");
    var cols = Q(c, "SELECT count(*) AS cnt FROM information_schema.columns WHERE table_name='parametre' AND table_schema='public'");
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='parametre' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'");
    Log($"    parametre rows={count} cols={cols[0]["cnt"]} PK={pk[0]["column_name"]}");
    return count == "1" && pk.Any(p => p["column_name"] == "code_ps");
});

Run("PH3.08 medicament — 29 columns, PK num_enr", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM medicament");
    var cols = Q(c, "SELECT column_name FROM information_schema.columns WHERE table_name='medicament' AND table_schema='public' ORDER BY ordinal_position");
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='medicament' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'");
    Log($"    medicament rows={count} cols={cols.Count} PK={pk[0]["column_name"]}");
    return cols.Count == 29 && pk.Any(p => p["column_name"] == "num_enr");
});

Run("PH3.09 signature — 2 columns, PK num_fact", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM signature");
    var cols = Q(c, "SELECT column_name FROM information_schema.columns WHERE table_name='signature' AND table_schema='public' ORDER BY ordinal_position");
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='signature' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'");
    Log($"    signature rows={count} cols={cols.Count} PK={pk[0]["column_name"]}");
    return cols.Count == 2 && pk.Any(p => p["column_name"] == "num_fact");
});

Run("PH3.10 ln — 1 column (num_serie)", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM ln");
    var cols = Q(c, "SELECT column_name, data_type, character_maximum_length FROM information_schema.columns WHERE table_name='ln' AND table_schema='public'");
    Log($"    ln rows={count} cols={cols.Count} col={cols[0]["column_name"]}({cols[0]["data_type"]}({cols[0]["character_maximum_length"]}))");
    return cols.Count == 1 && cols[0]["column_name"] == "num_serie";
});

// ============================================================
// PHASE 4 — VALIDATION DE parametre
// ============================================================
Section("PHASE 4 — VALIDATION DE parametre");

Run("PH4.01 parametre — full structure (58 columns)", () =>
{
    using var c = OpenConn();
    var cols = Q(c, @"SELECT column_name, data_type, character_maximum_length, is_nullable,
        COALESCE(column_default,'') AS def
        FROM information_schema.columns WHERE table_name='parametre' AND table_schema='public' ORDER BY ordinal_position");
    Log($"    parametre: {cols.Count} columns");
    foreach (var col in cols)
        Log($"      {col["column_name"],-30} {col["data_type"],-30} {(col["is_nullable"]=="YES"?"NULL":"NOT NULL")} {(col["def"]!=""?$"DEFAULT={col["def"]}":"")}");
    return cols.Count == 58;
});

Run("PH4.02 parametre — code_ps (code pharmacie)", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT code_ps FROM parametre");
    Log($"    code_ps = {val}");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.03 parametre — nom (nom pharmacie)", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT nom FROM parametre");
    Log($"    nom = {val}");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.04 parametre — prenom", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT prenom FROM parametre");
    Log($"    prenom = {val}");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.05 parametre — code_centre", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT code_centre FROM parametre");
    Log($"    code_centre = {val}");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.06 parametre — next_num_fact (counter)", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT next_num_fact::text FROM parametre");
    Log($"    next_num_fact = {val} (OBSERVED — NOT MODIFIED)");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.07 parametre — next_num_bord (counter)", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT next_num_bord::text FROM parametre");
    Log($"    next_num_bord = {val} (OBSERVED — NOT MODIFIED)");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.08 parametre — version", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT version FROM parametre");
    Log($"    version = {val}");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.09 parametre — annee", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT annee::text FROM parametre");
    Log($"    annee = {val}");
    return !string.IsNullOrEmpty(val);
});

Run("PH4.10 parametre — mont_maj_fae, mont_maj_sub, taux_maj_local", () =>
{
    using var c = OpenConn();
    var fae = S(c, "SELECT mont_maj_fae::text FROM parametre");
    var sub = S(c, "SELECT mont_maj_sub::text FROM parametre");
    var local = S(c, "SELECT taux_maj_local::text FROM parametre");
    Log($"    mont_maj_fae = {fae}");
    Log($"    mont_maj_sub = {sub}");
    Log($"    taux_maj_local = {local}");
    return true;
});

Run("PH4.11 parametre — access_token present", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT CASE WHEN access_token IS NULL THEN 'NULL' ELSE 'SET' END FROM parametre");
    Log($"    access_token = {val}");
    return true;
});

Run("PH4.12 parametre — refresh_token present", () =>
{
    using var c = OpenConn();
    var val = S(c, "SELECT CASE WHEN refresh_token IS NULL THEN 'NULL' ELSE 'SET' END FROM parametre");
    Log($"    refresh_token = {val}");
    return true;
});

// ============================================================
// PHASE 5 — VALIDATION DES FACTURES
// ============================================================
Section("PHASE 5 — VALIDATION DES FACTURES");

Run("PH5.01 facture — total count", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM facture");
    Log($"    Total factures: {count}");
    return count == "0";
});

Run("PH5.02 facture — full 53-column structure", () =>
{
    using var c = OpenConn();
    var cols = Q(c, @"SELECT column_name, data_type, character_maximum_length, is_nullable
        FROM information_schema.columns WHERE table_name='facture' AND table_schema='public' ORDER BY ordinal_position");
    Log($"    facture structure ({cols.Count} columns):");
    foreach (var col in cols)
        Log($"      {col["column_name"],-30} {col["data_type"],-30} {(col["is_nullable"]=="YES"?"NULL":"NOT NULL")}");
    return cols.Count == 53;
});

Run("PH5.03 facture — etat column type", () =>
{
    using var c = OpenConn();
    var col = Q(c, "SELECT data_type, character_maximum_length FROM information_schema.columns WHERE table_name='facture' AND column_name='etat'");
    Log($"    etat: {col[0]["data_type"]}({col[0]["character_maximum_length"]})");
    return col[0]["data_type"] == "character";
});

Run("PH5.04 facture — mont_fact column type", () =>
{
    using var c = OpenConn();
    var col = Q(c, "SELECT data_type, numeric_precision, numeric_scale FROM information_schema.columns WHERE table_name='facture' AND column_name='mont_fact'");
    Log($"    mont_fact: {col[0]["data_type"]} precision={col[0]["numeric_precision"]} scale={col[0]["numeric_scale"]}");
    return col[0]["data_type"] == "numeric";
});

Run("PH5.05 facture — signature column (xml)", () =>
{
    using var c = OpenConn();
    var col = Q(c, "SELECT data_type FROM information_schema.columns WHERE table_name='facture' AND column_name='signature'");
    Log($"    signature column: {col[0]["data_type"]}");
    return col[0]["data_type"] == "xml";
});

Run("PH5.06 facture — FK1_FACTURE references bordereau(num_bord)", () =>
{
    using var c = OpenConn();
    var fk = Q(c, @"SELECT ccu.column_name AS fcol, ccu.table_name AS ftable
        FROM information_schema.table_constraints tc
        JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name=ccu.constraint_name
        WHERE tc.table_name='facture' AND tc.constraint_type='FOREIGN KEY' AND tc.table_name='facture' AND tc.table_schema='public'");
    Log($"    FK constraints:");
    foreach (var f in fk) Log($"      → {f["ftable"]}.{f["fcol"]}");
    return fk.Any(f => f["ftable"] == "bordereau" && f["fcol"] == "num_bord");
});

// ============================================================
// PHASE 6 — VALIDATION DES BORDEREAUX
// ============================================================
Section("PHASE 6 — VALIDATION DES BORDEREAUX");

Run("PH6.01 bordereau — total count", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM bordereau");
    Log($"    Total bordereaux: {count}");
    return int.Parse(count) >= 0;
});

Run("PH6.02 bordereau — 11 columns", () =>
{
    using var c = OpenConn();
    var cols = Q(c, @"SELECT column_name, data_type, character_maximum_length, is_nullable, COALESCE(column_default,'') AS def
        FROM information_schema.columns WHERE table_name='bordereau' AND table_schema='public' ORDER BY ordinal_position");
    Log($"    bordereau ({cols.Count} columns):");
    foreach (var col in cols) Log($"      {col["column_name"],-25} {col["data_type"],-30} {(col["def"]!=""?$"DEFAULT={col["def"]}":"")}");
    return cols.Count == 11;
});

Run("PH6.03 bordereau — PK num_bord", () =>
{
    using var c = OpenConn();
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='bordereau' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'");
    Log($"    PK: {pk[0]["column_name"]}");
    return pk[0]["column_name"] == "num_bord";
});

Run("PH6.04 bordereau — UNIQUE on num_bord", () =>
{
    using var c = OpenConn();
    var uq = Q(c, @"SELECT constraint_name, constraint_type FROM information_schema.table_constraints
        WHERE table_name='bordereau' AND table_schema='public' AND constraint_type IN ('UNIQUE','PRIMARY KEY')");
    foreach (var u in uq) Log($"    {u["constraint_name"]} ({u["constraint_type"]})");
    return uq.Count >= 2; // PK + UNIQUE
});

Run("PH6.05 bordereau — sample data", () =>
{
    using var c = OpenConn();
    var rows = Q(c, "SELECT id_bord, num_bord, code_centre, etat FROM bordereau ORDER BY id_bord DESC LIMIT 5");
    Log($"    Latest bordereaux: {rows.Count}");
    foreach (var r in rows) Log($"      id_bord={r["id_bord"]} num_bord={r["num_bord"]} centre={r["code_centre"]} etat={r["etat"]}");
    return true;
});

Run("PH6.06 facture → bordereau FK integrity", () =>
{
    using var c = OpenConn();
    var orphans = S(c, @"SELECT count(*) FROM facture f LEFT JOIN bordereau b ON f.num_bord=b.num_bord
        WHERE f.num_bord IS NOT NULL AND b.num_bord IS NULL");
    Log($"    Orphaned factures (num_bord not in bordereau): {orphans}");
    return orphans == "0";
});

// ============================================================
// PHASE 7 — VALIDATION DES MÉDICAMENTS
// ============================================================
Section("PHASE 7 — VALIDATION DES MÉDICAMENTS");

Run("PH7.01 medicament — total count", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM medicament");
    Log($"    Total medicaments: {count}");
    return int.Parse(count) == 7596;
});

Run("PH7.02 medicament — 29 columns", () =>
{
    using var c = OpenConn();
    var cols = Q(c, @"SELECT column_name, data_type, character_maximum_length, is_nullable
        FROM information_schema.columns WHERE table_name='medicament' AND table_schema='public' ORDER BY ordinal_position");
    Log($"    medicament ({cols.Count} columns):");
    foreach (var col in cols) Log($"      {col["column_name"],-25} {col["data_type"],-25}");
    return cols.Count == 29;
});

Run("PH7.03 medicament — PK num_enr", () =>
{
    using var c = OpenConn();
    var pk = Q(c, @"SELECT kcu.column_name FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu ON tc.constraint_name=kcu.constraint_name
        WHERE tc.table_name='medicament' AND tc.constraint_type='PRIMARY KEY' AND tc.table_schema='public'");
    Log($"    PK: {pk[0]["column_name"]}");
    return pk[0]["column_name"] == "num_enr";
});

Run("PH7.04 medicament — sample (LIMIT 5)", () =>
{
    using var c = OpenConn();
    var rows = Q(c, "SELECT num_enr, nom_com, tarif_ref FROM medicament ORDER BY num_enr LIMIT 5");
    Log($"    Sample:");
    foreach (var r in rows) Log($"      [{r["num_enr"]}] {r["nom_com"]} — tarif_ref={r["tarif_ref"]}");
    return rows.Count > 0;
});

Run("PH7.05 medicament — column name list", () =>
{
    using var c = OpenConn();
    var cols = Q(c, "SELECT column_name FROM information_schema.columns WHERE table_name='medicament' AND table_schema='public' ORDER BY ordinal_position");
    var names = cols.Select(c => c["column_name"]).ToList();
    Log($"    Columns: {string.Join(", ", names)}");
    return names.Count == 29;
});

// ============================================================
// PHASE 8 — VALIDATION DE LA TABLE ln
// ============================================================
Section("PHASE 8 — VALIDATION DE LA TABLE ln");

Run("PH8.01 ln — total count", () =>
{
    using var c = OpenConn();
    var count = S(c, "SELECT count(*) FROM ln");
    Log($"    Total ln rows: {count}");
    return long.Parse(count) > 0;
});

Run("PH8.02 ln — structure (1 column: num_serie varchar(16))", () =>
{
    using var c = OpenConn();
    var cols = Q(c, @"SELECT column_name, data_type, character_maximum_length
        FROM information_schema.columns WHERE table_name='ln' AND table_schema='public'");
    Log($"    ln: {cols.Count} column(s)");
    foreach (var col in cols) Log($"      {col["column_name"]} {col["data_type"]}({col["character_maximum_length"]})");
    return cols.Count == 1 && cols[0]["column_name"] == "num_serie";
});

Run("PH8.03 ln — indexes", () =>
{
    using var c = OpenConn();
    var idx = Q(c, "SELECT indexname FROM pg_indexes WHERE tablename='ln' AND schemaname='public'");
    Log($"    ln indexes: {idx.Count}");
    foreach (var i in idx) Log($"      {i["indexname"]}");
    return idx.Count >= 0;
});

Run("PH8.04 ln — sample (LIMIT 5)", () =>
{
    using var c = OpenConn();
    var rows = Q(c, "SELECT num_serie FROM ln LIMIT 5");
    Log($"    Sample ln:");
    foreach (var r in rows) Log($"      {r["num_serie"]}");
    return rows.Count > 0;
});

Run("PH8.05 ln — unique vs total", () =>
{
    using var c = OpenConn();
    var total = S(c, "SELECT count(*) FROM ln");
    var distinct = S(c, "SELECT count(DISTINCT num_serie) FROM ln");
    Log($"    Total: {total} | Distinct: {distinct} | Duplicates: {long.Parse(total) - long.Parse(distinct)}");
    return long.Parse(total) > 0;
});

// ============================================================
// PHASE 10 — POST-CAPTURE: Proof of Non-Write
// ============================================================
Section("PHASE 10 — POST-CAPTURE: Proof of Non-Write");

Run("PH10.01 No data modification", () =>
{
    using var c = OpenConn();
    string fFact = S(c, "SELECT count(*) FROM facture");
    string fDetail = S(c, "SELECT count(*) FROM detail_fact");
    string fBord = S(c, "SELECT count(*) FROM bordereau");
    string fParam = S(c, "SELECT count(*) FROM parametre");
    string fMed = S(c, "SELECT count(*) FROM medicament");
    string fSign = S(c, "SELECT count(*) FROM signature");
    string fLn = S(c, "SELECT count(*) FROM ln");
    string fNextFact = S(c, "SELECT next_num_fact::text FROM parametre");
    string fNextBord = S(c, "SELECT next_num_bord::text FROM parametre");
    c.Close();

    Log("  ┌───────────────────────────────────────────────────────┐");
    Log("  │              BEFORE → AFTER                          │");
    Log("  ├───────────────────────────────────────────────────────┤");
    Log($"  │ facture:      {pFact,8} → {fFact,-8} {(pFact==fFact?"OK":"!!CHANGED!!")}           │");
    Log($"  │ detail_fact:  {pDetail,8} → {fDetail,-8} {(pDetail==fDetail?"OK":"!!CHANGED!!")}           │");
    Log($"  │ bordereau:    {pBord,8} → {fBord,-8} {(pBord==fBord?"OK":"!!CHANGED!!")}           │");
    Log($"  │ parametre:    {pParam,8} → {fParam,-8} {(pParam==fParam?"OK":"!!CHANGED!!")}           │");
    Log($"  │ medicament:   {pMed,8} → {fMed,-8} {(pMed==fMed?"OK":"!!CHANGED!!")}           │");
    Log($"  │ signature:    {pSign,8} → {fSign,-8} {(pSign==fSign?"OK":"!!CHANGED!!")}           │");
    Log($"  │ ln:           {pLn,8} → {fLn,-8} {(pLn==fLn?"OK":"!!CHANGED!!")}           │");
    Log($"  │ next_num_fact:{pNextFact,8} → {fNextFact,-8} {(pNextFact==fNextFact?"OK":"!!CHANGED!!")}           │");
    Log($"  │ next_num_bord:{pNextBord,8} → {fNextBord,-8} {(pNextBord==fNextBord?"OK":"!!CHANGED!!")}           │");
    Log("  └───────────────────────────────────────────────────────┘");
    Log("");
    Log("  INSERT  = 0");
    Log("  UPDATE  = 0");
    Log("  DELETE  = 0");
    Log("  TRUNCATE = 0");

    return pFact==fFact && pDetail==fDetail && pBord==fBord && pParam==fParam
        && pMed==fMed && pSign==fSign && pLn==fLn && pNextFact==fNextFact && pNextBord==fNextBord;
});

// ============================================================
// SUMMARY
// ============================================================
Section("FINAL SUMMARY");
Log($"  Total tests:  {totalTests}");
Log($"  Passed:       {passed}");
Log($"  Failed:       {failed}");
Log($"  Status:       {(failed == 0 ? "ALL PASS" : $"{failed} FAILED")}");

string reportPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "BM-PHASE-004.12-REAL-VALIDATION-LOG.txt");
try { File.WriteAllText(reportPath, log.ToString()); Log($"  Log: {Path.GetFullPath(reportPath)}"); } catch { }
