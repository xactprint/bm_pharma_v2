using Npgsql;
using System;
using System.Text;

const string CONN = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=60";

var log = new StringBuilder();
void Log(string msg) { Console.WriteLine(msg); log.AppendLine(msg); }
void Section(string title) { Console.WriteLine($"\n=== {title} ==="); Log($"\n=== {title} ==="); }

string S(NpgsqlConnection c, string sql) { using var cmd = new NpgsqlCommand(sql, c); return cmd.ExecuteScalar()?.ToString() ?? "(null)"; }

Log("========================================================");
Log("  BM-PHASE-007-I — ROLLBACK CONTRÔLÉ TST002");
Log($"  Date: {DateTime.UtcNow:O}");
Log("========================================================");

using var conn = new NpgsqlConnection(CONN);
await conn.OpenAsync();
Log($"Connected: {conn.ServerVersion} | {conn.UserName}@{conn.Database}");

// ============================================================
// PRE-ROLLBACK SNAPSHOT
// ============================================================
Section("PRE-ROLLBACK SNAPSHOT");
var preFact = S(conn, "SELECT count(*) FROM facture");
var preDet = S(conn, "SELECT count(*) FROM detail_fact");
var preBord = S(conn, "SELECT count(*) FROM bordereau");
var preSign = S(conn, "SELECT count(*) FROM signature");
var preNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var preNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");

Log($"  facture      = {preFact}");
Log($"  detail_fact  = {preDet}");
Log($"  bordereau    = {preBord}");
Log($"  signature    = {preSign}");
Log($"  next_num_fact = {preNextFact}");
Log($"  next_num_bord = {preNextBord}");

// Verify TST002 exists
var tstFact = S(conn, "SELECT count(*) FROM facture WHERE num_fact = 'TST002'");
var tstDet = S(conn, "SELECT count(*) FROM detail_fact WHERE num_fact = 'TST002'");
Log($"\n  TST002 facture: {tstFact}");
Log($"  TST002 detail_fact: {tstDet}");

if (tstFact == "0" && tstDet == "0")
{
    Log("\n  TST002 not found — nothing to rollback");
    await conn.CloseAsync();
    return;
}

// ============================================================
// ROLLBACK: DELETE in transaction
// ============================================================
Section("ROLLBACK — DELETE TST002");

using (var tx = conn.BeginTransaction())
{
    try
    {
        var delDet = new NpgsqlCommand("DELETE FROM detail_fact WHERE num_fact = 'TST002'", conn, tx);
        var detDeleted = await delDet.ExecuteNonQueryAsync();
        Log($"  ✓ DELETE detail_fact: {detDeleted} row(s) deleted");

        var delFact = new NpgsqlCommand("DELETE FROM facture WHERE num_fact = 'TST002'", conn, tx);
        var factDeleted = await delFact.ExecuteNonQueryAsync();
        Log($"  ✓ DELETE facture: {factDeleted} row(s) deleted");

        await tx.CommitAsync();
        Log($"  ✓ TRANSACTION COMMITTED");
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync();
        Log($"  ✗ EXCEPTION: {ex.Message}");
        Log($"  ✗ ROLLBACK APPLIED — TST002 PRESERVED");
        await conn.CloseAsync();
        return;
    }
}

// ============================================================
// POST-ROLLBACK VERIFICATION
// ============================================================
Section("POST-ROLLBACK VERIFICATION");

var postFact = S(conn, "SELECT count(*) FROM facture");
var postDet = S(conn, "SELECT count(*) FROM detail_fact");
var postBord = S(conn, "SELECT count(*) FROM bordereau");
var postSign = S(conn, "SELECT count(*) FROM signature");
var postLn = S(conn, "SELECT count(*) FROM ln");
var postMed = S(conn, "SELECT count(*) FROM medicament");
var postNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var postNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");
var postCodeCentre = S(conn, "SELECT code_centre FROM parametre");
var postCodePs = S(conn, "SELECT code_ps FROM parametre");

Log($"  facture      = {postFact}");
Log($"  detail_fact  = {postDet}");
Log($"  bordereau    = {postBord}");
Log($"  signature    = {postSign}");
Log($"  ln           = {postLn}");
Log($"  medicament   = {postMed}");
Log($"  next_num_fact = {postNextFact}");
Log($"  next_num_bord = {postNextBord}");
Log($"  code_centre   = {postCodeCentre}");
Log($"  code_ps       = {postCodePs}");

var postTstFact = S(conn, "SELECT count(*) FROM facture WHERE num_fact = 'TST002'");
var postTstDet = S(conn, "SELECT count(*) FROM detail_fact WHERE num_fact = 'TST002'");
Log($"\n  TST002 facture rows: {postTstFact} (must be 0)");
Log($"  TST002 detail_fact rows: {postTstDet} (must be 0)");

await conn.CloseAsync();

// ============================================================
// COMPARISON TABLE
// ============================================================
Section("COMPARISON: BASELINE vs POST-ROLLBACK");
Log("  ┌──────────────────────┬──────────┬──────────┬──────────┬──────────┐");
Log("  │ Table/Counter        │ Baseline │ Pre-RB   │ Post-RB  │ Statut   │");
Log("  ├──────────────────────┼──────────┼──────────┼──────────┼──────────┤");
Log($"  │ facture              │ 0        │ {preFact,-8} │ {postFact,-8} │ {(postFact == "0" ? "OK" : "FAIL")  ,-8} │");
Log($"  │ detail_fact          │ 0        │ {preDet,-8} │ {postDet,-8} │ {(postDet == "0" ? "OK" : "FAIL")  ,-8} │");
Log($"  │ bordereau            │ 0        │ {preBord,-8} │ {postBord,-8} │ {(postBord == "0" ? "OK" : "FAIL")  ,-8} │");
Log($"  │ signature            │ 0        │ {preSign,-8} │ {postSign,-8} │ {(postSign == "0" ? "OK" : "FAIL")  ,-8} │");
Log($"  │ next_num_fact        │ 1        │ {preNextFact,-8} │ {postNextFact,-8} │ {(postNextFact == "1" ? "OK" : "FAIL")  ,-8} │");
Log($"  │ next_num_bord        │ 215      │ {preNextBord,-8} │ {postNextBord,-8} │ {(postNextBord == "215" ? "OK" : "FAIL")  ,-8} │");
Log($"  │ ln                   │ 7412276  │ {postLn,-8} │ {postLn,-8} │ {(postLn == "7412276" ? "OK" : "FAIL")  ,-8} │");
Log($"  │ medicament           │ 7596     │ {postMed,-8} │ {postMed,-8} │ {(postMed == "7596" ? "OK" : "FAIL")  ,-8} │");
Log("  └──────────────────────┴──────────┴──────────┴──────────┴──────────┘");

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

Check("facture rows", "0", postFact);
Check("detail_fact rows", "0", postDet);
Check("bordereau rows", "0", postBord);
Check("signature rows", "0", postSign);
Check("next_num_fact", "1", postNextFact);
Check("next_num_bord", "215", postNextBord);
Check("ln rows", "7412276", postLn);
Check("medicament rows", "7596", postMed);
Check("TST001 facture gone", "0", postTstFact);
Check("TST002 detail_fact gone", "0", postTstDet);

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

// Save report
string path = @"C:\Users\pc\Desktop\bm_stock\BMPharma\BM-PHASE-007-I-ROLLBACK-LOG.txt";
System.IO.File.WriteAllText(path, log.ToString());
Console.WriteLine($"\nLog saved: {path}");
