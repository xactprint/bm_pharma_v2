using Npgsql;
using System;
using System.Text;

const string CONN = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=60";

var sb = new StringBuilder();
void Log(string msg) { Console.WriteLine(msg); sb.AppendLine(msg); }
void Section(string title) { Console.WriteLine($"\n=== {title} ==="); sb.AppendLine($"\n=== {title} ==="); }

string S(NpgsqlConnection c, string sql) { using var cmd = new NpgsqlCommand(sql, c); return cmd.ExecuteScalar()?.ToString() ?? "(null)"; }

Log("==========================================================");
Log("  BM-PHASE-008-B — ROLLBACK TST003");
Log($"  Date: {DateTime.UtcNow:O}");
Log("==========================================================");

await using var conn = new NpgsqlConnection(CONN);
await conn.OpenAsync();
Log($"  Connected: {conn.ServerVersion} | {conn.UserName}@{conn.Database}");

// ============================================================
// STEP 1: PRE-ROLLBACK SNAPSHOT
// ============================================================
Section("STEP 1 — PRE-ROLLBACK SNAPSHOT");

var preFact = S(conn, "SELECT count(*) FROM facture");
var preDet = S(conn, "SELECT count(*) FROM detail_fact");
var preBord = S(conn, "SELECT count(*) FROM bordereau");
var preSign = S(conn, "SELECT count(*) FROM signature");
var preNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var preNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");

var tst003Fact = S(conn, "SELECT count(*) FROM facture WHERE num_fact='TST003'");
var tst003Det = S(conn, "SELECT count(*) FROM detail_fact WHERE num_fact='TST003'");
var bord215 = S(conn, "SELECT count(*) FROM bordereau WHERE num_bord='215'");

Log($"  facture           = {preFact}");
Log($"  detail_fact       = {preDet}");
Log($"  bordereau         = {preBord}");
Log($"  signature         = {preSign}");
Log($"  next_num_fact     = {preNextFact}");
Log($"  next_num_bord     = {preNextBord}");
Log($"  TST003 facture    = {tst003Fact}");
Log($"  TST003 detail_fact= {tst003Det}");
Log($"  bordereau 215     = {bord215}");

if (tst003Fact == "0" && tst003Det == "0" && bord215 == "0")
{
    Log("\n  ⏭ Aucune donnée TST003 à rollback — baseline déjà propre");
    goto Verify;
}

// ============================================================
// STEP 2: DELETE detail_fact → facture → bordereau
// ============================================================
Section("STEP 2 — DELETE TST003 DATA (dans l'ordre FK inverse)");

using (var tx = conn.BeginTransaction())
{
    try
    {
        // 1. Delete detail_fact (FK → facture)
        Log("  DELETE detail_fact WHERE num_fact='TST003'");
        using var cmdDet = new NpgsqlCommand("DELETE FROM detail_fact WHERE num_fact='TST003'", conn, tx);
        var rDet = await cmdDet.ExecuteNonQueryAsync();
        Log($"  → {rDet} ligne(s) supprimée(s)");

        // 2. Delete facture (FK → bordereau)
        Log("\n  DELETE facture WHERE num_fact='TST003'");
        using var cmdFact = new NpgsqlCommand("DELETE FROM facture WHERE num_fact='TST003'", conn, tx);
        var rFact = await cmdFact.ExecuteNonQueryAsync();
        Log($"  → {rFact} ligne(s) supprimée(s)");

        // 3. Delete bordereau (pas de FK entrante après suppression facture)
        Log("\n  DELETE bordereau WHERE num_bord='215'");
        using var cmdBord = new NpgsqlCommand("DELETE FROM bordereau WHERE num_bord='215'", conn, tx);
        var rBord = await cmdBord.ExecuteNonQueryAsync();
        Log($"  → {rBord} ligne(s) supprimée(s)");

        await tx.CommitAsync();
        Log("\n  ✅ TRANSACTION ROLLBACK COMMITTED");
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync();
        Log($"\n  ❌ EXCEPTION: {ex.Message}");
        Log("  ⛔ ROLLBACK ÉCHOUÉ");
        return;
    }
}

// ============================================================
// STEP 3: VÉRIFICATION POST-ROLLBACK
// ============================================================
Section("STEP 3 — VÉRIFICATION POST-ROLLBACK");

Verify:
var postFact = S(conn, "SELECT count(*) FROM facture");
var postDet = S(conn, "SELECT count(*) FROM detail_fact");
var postBord = S(conn, "SELECT count(*) FROM bordereau");
var postSign = S(conn, "SELECT count(*) FROM signature");
var postNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var postNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");
var postTstFact = S(conn, "SELECT count(*) FROM facture WHERE num_fact='TST003'");
var postTstDet = S(conn, "SELECT count(*) FROM detail_fact WHERE num_fact='TST003'");
var postBord215 = S(conn, "SELECT count(*) FROM bordereau WHERE num_bord='215'");
var orphan1 = S(conn, "SELECT count(*) FROM facture f LEFT JOIN bordereau b ON f.num_bord=b.num_bord WHERE f.num_bord IS NOT NULL AND b.num_bord IS NULL");
var orphan2 = S(conn, "SELECT count(*) FROM detail_fact d LEFT JOIN facture f ON d.num_fact=f.num_fact WHERE f.num_fact IS NULL");

Log($"  facture           = {postFact}  (attendu: 0)");
Log($"  detail_fact       = {postDet}  (attendu: 0)");
Log($"  bordereau         = {postBord}  (attendu: 0)");
Log($"  signature         = {postSign}  (attendu: 0)");
Log($"  next_num_fact     = {postNextFact}  (attendu: 1)");
Log($"  next_num_bord     = {postNextBord}  (attendu: 215)");
Log($"  TST003 facture    = {postTstFact}  (attendu: 0)");
Log($"  TST003 detail_fact= {postTstDet}  (attendu: 0)");
Log($"  bordereau 215     = {postBord215}  (attendu: 0)");
Log($"  orphelins f→b     = {orphan1}  (attendu: 0)");
Log($"  orphelins d→f     = {orphan2}  (attendu: 0)");

bool ok = postFact == "0" && postDet == "0" && postBord == "0" && postSign == "0"
       && postNextFact == "1" && postNextBord == "215"
       && postTstFact == "0" && postTstDet == "0" && postBord215 == "0"
       && orphan1 == "0" && orphan2 == "0";

Log($"\n  ➡ Statut: {(ok ? "✅ BASELINE RESTORED" : "❌ VERIFICATION ÉCHOUÉE")}");

await conn.CloseAsync();

string path = @"C:\Users\pc\Desktop\bm_stock\BMPharma\BM-PHASE-008-B-ROLLBACK-REPORT.md";
System.IO.File.WriteAllText(path, sb.ToString());
Console.WriteLine($"\nReport: {path}");
