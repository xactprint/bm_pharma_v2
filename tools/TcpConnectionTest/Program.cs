using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// ============================================================
// BM-PHASE-005-F — ROLLBACK TST001
// ============================================================

const string CONN_STR = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=60";
var log = new StringBuilder();

void Log(string msg) { Console.WriteLine(msg); log.AppendLine(msg); }
void Section(string title) { Console.WriteLine($"\n=== {title} ==="); Log($"\n=== {title} ==="); }

string S(NpgsqlConnection c, string sql) { using var cmd = new NpgsqlCommand(sql, c); return cmd.ExecuteScalar()?.ToString() ?? "(null)"; }

List<Dictionary<string, string>> Q(NpgsqlConnection c, string sql)
{
    var r = new List<Dictionary<string, string>>();
    using var cmd = new NpgsqlCommand(sql, c);
    using var rd = cmd.ExecuteReader();
    while (rd.Read()) { var row = new Dictionary<string, string>(); for (int i = 0; i < rd.FieldCount; i++) row[rd.GetName(i)] = rd.IsDBNull(i) ? "NULL" : rd[i]?.ToString() ?? ""; r.Add(row); }
    return r;
}

using var conn = new NpgsqlConnection(CONN_STR);
conn.Open();
Log($"Connected: {conn.ServerVersion} | {conn.UserName}@{conn.Database}");
Log($"Timestamp: {DateTime.UtcNow:O}");

// ============================================================
// PRE-ROLLBACK SNAPSHOT
// ============================================================
Section("PRE-ROLLBACK SNAPSHOT");
var preFactCount = S(conn, "SELECT count(*) FROM facture");
var preDetCount = S(conn, "SELECT count(*) FROM detail_fact");
var preBordCount = S(conn, "SELECT count(*) FROM bordereau");
var preSignCount = S(conn, "SELECT count(*) FROM signature");
var preLnCount = S(conn, "SELECT count(*) FROM ln");
var preMedCount = S(conn, "SELECT count(*) FROM medicament");
var preNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var preNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");
var preCodeCentre = S(conn, "SELECT code_centre FROM parametre");
var preCodePs = S(conn, "SELECT code_ps FROM parametre");

Log($"  facture      = {preFactCount}");
Log($"  detail_fact  = {preDetCount}");
Log($"  bordereau    = {preBordCount}");
Log($"  signature    = {preSignCount}");
Log($"  ln           = {preLnCount}");
Log($"  medicament   = {preMedCount}");
Log($"  next_num_fact = {preNextFact}");
Log($"  next_num_bord = {preNextBord}");
Log($"  code_centre   = {preCodeCentre}");
Log($"  code_ps       = {preCodePs}");

// ============================================================
// VERIFY TST001 EXISTS
// ============================================================
Section("PRE-ROLLBACK VERIFICATION");
var tstFact = Q(conn, "SELECT * FROM facture WHERE num_fact = 'TST001'");
var tstDet = Q(conn, "SELECT * FROM detail_fact WHERE num_fact = 'TST001'");
Log($"  TST001 facture rows: {tstFact.Count}");
Log($"  TST001 detail_fact rows: {tstDet.Count}");

if (tstFact.Count == 0)
{
    Log($"  WARNING: TST001 not found — nothing to rollback");
}

// ============================================================
// 005-F: ROLLBACK
// ============================================================
Section("005-F: ROLLBACK — DELETE TST001");

using (var tx = conn.BeginTransaction())
{
    try
    {
        // 1. Delete detail_fact first (FK dependency)
        var delDet = new NpgsqlCommand("DELETE FROM detail_fact WHERE num_fact = 'TST001'", conn, tx);
        var detDeleted = delDet.ExecuteNonQuery();
        Log($"  ✓ DELETE detail_fact: {detDeleted} row(s) deleted");

        // 2. Delete facture
        var delFact = new NpgsqlCommand("DELETE FROM facture WHERE num_fact = 'TST001'", conn, tx);
        var factDeleted = delFact.ExecuteNonQuery();
        Log($"  ✓ DELETE facture: {factDeleted} row(s) deleted");

        tx.Commit();
        Log($"  ✓ TRANSACTION COMMITTED");
    }
    catch (Exception ex)
    {
        tx.Rollback();
        Log($"  ✗ ROLLBACK FAILED: {ex.Message}");
        conn.Close();
        return;
    }
}

// ============================================================
// POST-ROLLBACK VERIFICATION
// ============================================================
Section("POST-ROLLBACK VERIFICATION");

var postFactCount = S(conn, "SELECT count(*) FROM facture");
var postDetCount = S(conn, "SELECT count(*) FROM detail_fact");
var postBordCount = S(conn, "SELECT count(*) FROM bordereau");
var postSignCount = S(conn, "SELECT count(*) FROM signature");
var postLnCount = S(conn, "SELECT count(*) FROM ln");
var postMedCount = S(conn, "SELECT count(*) FROM medicament");
var postNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var postNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");
var postCodeCentre = S(conn, "SELECT code_centre FROM parametre");
var postCodePs = S(conn, "SELECT code_ps FROM parametre");

Log($"  facture      = {postFactCount}");
Log($"  detail_fact  = {postDetCount}");
Log($"  bordereau    = {postBordCount}");
Log($"  signature    = {postSignCount}");
Log($"  ln           = {postLnCount}");
Log($"  medicament   = {postMedCount}");
Log($"  next_num_fact = {postNextFact}");
Log($"  next_num_bord = {postNextBord}");
Log($"  code_centre   = {postCodeCentre}");
Log($"  code_ps       = {postCodePs}");

// Verify TST001 gone
var tstFactAfter = Q(conn, "SELECT * FROM facture WHERE num_fact = 'TST001'");
var tstDetAfter = Q(conn, "SELECT * FROM detail_fact WHERE num_fact = 'TST001'");
Log($"\n  TST001 facture rows after rollback: {tstFactAfter.Count} (must be 0)");
Log($"  TST001 detail_fact rows after rollback: {tstDetAfter.Count} (must be 0)");

// ============================================================
// COMPARISON TABLE
// ============================================================
Section("COMPARISON: BASELINE vs POST-ROLLBACK");
Log("  ┌──────────────────────┬──────────┬──────────┬──────────┐");
Log("  │ Table/Counter        │ Baseline │ Pre-RB   │ Post-RB  │");
Log("  ├──────────────────────┼──────────┼──────────┼──────────┤");
Log($"  │ facture              │ 0        │ {preFactCount,-8} │ {postFactCount,-8} │");
Log($"  │ detail_fact          │ 0        │ {preDetCount,-8} │ {postDetCount,-8} │");
Log($"  │ bordereau            │ 0        │ {preBordCount,-8} │ {postBordCount,-8} │");
Log($"  │ signature            │ 0        │ {preSignCount,-8} │ {postSignCount,-8} │");
Log($"  │ ln                   │ 7412276  │ {preLnCount,-8} │ {postLnCount,-8} │");
Log($"  │ medicament           │ 7596     │ {preMedCount,-8} │ {postMedCount,-8} │");
Log($"  │ next_num_fact        │ 1        │ {preNextFact,-8} │ {postNextFact,-8} │");
Log($"  │ next_num_bord        │ 215      │ {preNextBord,-8} │ {postNextBord,-8} │");
Log($"  │ code_centre          │ 11600    │ {preCodeCentre,-8} │ {postCodeCentre,-8} │");
Log($"  │ code_ps              │ 1234567890│ {preCodePs,-8} │ {postCodePs,-8} │");
Log("  └──────────────────────┴──────────┴──────────┴──────────┘");

// ============================================================
// FINAL VALIDATION
// ============================================================
Section("FINAL VALIDATION");
var allOk = true;

void Check(string name, string expected, string actual)
{
    var ok = expected == actual;
    var mark = ok ? "✓" : "✗";
    Log($"  {mark} {name}: {actual} (expected {expected})");
    if (!ok) allOk = false;
}

Check("facture rows", "0", postFactCount);
Check("detail_fact rows", "0", postDetCount);
Check("bordereau rows", "0", postBordCount);
Check("signature rows", "0", postSignCount);
Check("next_num_fact", "1", postNextFact);
Check("next_num_bord", "215", postNextBord);
Check("code_centre", "11600", postCodeCentre);
Check("code_ps", "1234567890", postCodePs);
Check("ln rows", "7412276", postLnCount);
Check("medicament rows", "7596", postMedCount);
Check("TST001 facture gone", "0", tstFactAfter.Count.ToString());
Check("TST001 detail_fact gone", "0", tstDetAfter.Count.ToString());

Log("");
if (allOk)
{
    Log("  ═══════════════════════════════════════════════════");
    Log("  ║  ROLLBACK COMPLET — BASELINE RESTAURÉ          ║");
    Log("  ═══════════════════════════════════════════════════");
}
else
{
    Log("  ═══════════════════════════════════════════════════");
    Log("  ║  ATTENTION: CERTAINS CHIFFRES DIFFÈRENT        ║");
    Log("  ═══════════════════════════════════════════════════");
}

conn.Close();

// Save report
string path = @"C:\Users\pc\Desktop\bm_stock\BMPharma\BM-PHASE-005-F-ROLLBACK-LOG.txt";
File.WriteAllText(path, log.ToString());
Console.WriteLine($"\nLog saved: {path}");
