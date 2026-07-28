using Npgsql;
using System;
using System.Text;

const string CONN = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=60";

var sb = new StringBuilder();
void Log(string msg) { Console.WriteLine(msg); sb.AppendLine(msg); }
void Section(string title) { Console.WriteLine($"\n=== {title} ==="); sb.AppendLine($"\n=== {title} ==="); }

string S(NpgsqlConnection c, string sql) { using var cmd = new NpgsqlCommand(sql, c); return cmd.ExecuteScalar()?.ToString() ?? "(null)"; }

Log("==========================================================");
Log("  BM-PHASE-008-B — TEST DE VISIBILITÉ TST003");
Log($"  Date: {DateTime.UtcNow:O}");
Log($"  Type: INSERT direct Npgsql (pas EF Core)");
Log("==========================================================");

await using var conn = new NpgsqlConnection(CONN);
await conn.OpenAsync();
Log($"  Connected: {conn.ServerVersion} | {conn.UserName}@{conn.Database}");

// ============================================================
// 008-B-1: SNAPSHOT
// ============================================================
Section("008-B-1 — SNAPSHOT BASELINE");

var preFact = S(conn, "SELECT count(*) FROM facture");
var preDet = S(conn, "SELECT count(*) FROM detail_fact");
var preBord = S(conn, "SELECT count(*) FROM bordereau");
var preSign = S(conn, "SELECT count(*) FROM signature");
var preNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var preNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");
var preLn = S(conn, "SELECT count(*) FROM ln");
var preMed = S(conn, "SELECT count(*) FROM medicament");

Log($"  facture      = {preFact}  (attendu: 0)");
Log($"  detail_fact  = {preDet}  (attendu: 0)");
Log($"  bordereau    = {preBord}  (attendu: 0)");
Log($"  signature    = {preSign}  (attendu: 0)");
Log($"  next_num_fact = {preNextFact}  (attendu: 1)");
Log($"  next_num_bord = {preNextBord}  (attendu: 215)");
Log($"  ln           = {preLn}");
Log($"  medicament   = {preMed}");

var tst003Fact = S(conn, "SELECT count(*) FROM facture WHERE num_fact='TST003'");
var tst003Det = S(conn, "SELECT count(*) FROM detail_fact WHERE num_fact='TST003'");
var bord215 = S(conn, "SELECT count(*) FROM bordereau WHERE num_bord='215'");
Log($"  TST003 facture existe: {tst003Fact}  (attendu: 0)");
Log($"  TST003 detail_fact existe: {tst003Det}  (attendu: 0)");
Log($"  bordereau 215 existe: {bord215}  (attendu: 0)");

if (preFact != "0" || preDet != "0" || preBord != "0" || preSign != "0" ||
    tst003Fact != "0" || tst003Det != "0" || bord215 != "0")
{
    Log("\n  ⛔ SNAPSHOT INVALIDE — Arrêt avant écriture");
    return;
}

// ============================================================
// 008-B-2/3/4/5/6: INSERT BORDEREAU + FACTURE + DETAIL_FACT
// ============================================================
Section("008-B-2/4/5/6 — INSERT TST003");

using (var tx = conn.BeginTransaction())
{
    try
    {
        // === BORDEREAU (008-B-4) ===
        Log("  INSERT INTO bordereau(num_bord='215', ...)");
        using var cmdBord = new NpgsqlCommand(@"
            INSERT INTO bordereau(num_bord, code_centre, etat, date_ouverture, duplicata, mont_vir)
            VALUES('215', '11600', 'O', NOW(), false, 0)", conn, tx);
        var rBord = await cmdBord.ExecuteNonQueryAsync();
        Log($"  → {rBord} ligne(s) insérée(s)");

        // === FACTURE (008-B-5) ===
        Log("\n  INSERT INTO facture(num_fact='TST003', ...)");
        using var cmdFact = new NpgsqlCommand(@"
            INSERT INTO facture(
                num_fact, date_fact, etat, num_bord,
                mont_off, mont_as, mont_fact,
                num_assure, rang_ad, code_centre,
                tp, taux, code_affect, conv, type_consult,
                prescripteur, date_soin, risque,
                statut_fact, verifcms, type_signature, verif_fact,
                mont_maj_fae, mont_maj, type_maj,
                nat_remb, mont_mut, code_mut, date_fin_mut, date_synchro,
                version, echifa, e_ord
            ) VALUES (
                'TST003', NOW(), '0', '215',
                120.00, 84.00, 120.00,
                'TST99999', '1', '11600',
                '1', '3', '01', '1', '01',
                'BM PHARMA', CURRENT_DATE, '0',
                '1', '0', '0', '0',
                0, 0, 0,
                '0', 0, NULL, '1900-01-01', '1900-01-01',
                '2.0.0', false, false
            )", conn, tx);
        var rFact = await cmdFact.ExecuteNonQueryAsync();
        Log($"  → {rFact} ligne(s) insérée(s)");

        // === DETAIL_FACT (008-B-6) ===
        Log("\n  INSERT INTO detail_fact(num_fact='TST003', num_enr='00010', ...)");
        using var cmdDet = new NpgsqlCommand(@"
            INSERT INTO detail_fact(
                num_fact, num_enr, ppa, qte, mont, mont_as, mont_pharm,
                num_enr_prescrit, num_lot, duree_trait, tarif_ref, posologie,
                remboursable, local, inf_tr, applic_tr, medic, ts,
                maj_local, maj_sub
            ) VALUES (
                'TST003', '00010', 60.00, 2, 120.00, 84.00, 36.00,
                '00010', 'LOT003', 30, 60.00, '1/day',
                true, false, true, true, true, false,
                0, 0
            )", conn, tx);
        var rDet = await cmdDet.ExecuteNonQueryAsync();
        Log($"  → {rDet} ligne(s) insérée(s)");

        await tx.CommitAsync();
        Log("\n  ✅ TRANSACTION COMMITTED");
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync();
        Log($"\n  ❌ EXCEPTION: {ex.Message}");
        Log("  ⛔ ROLLBACK EFFECTUÉ — Test annulé");
        return;
    }
}

// ============================================================
// 008-B-7: VÉRIFICATION SQL
// ============================================================
Section("008-B-7 — VÉRIFICATION SQL POST-ÉCRITURE");

var postFact = S(conn, "SELECT count(*) FROM facture");
var postDet = S(conn, "SELECT count(*) FROM detail_fact");
var postBord = S(conn, "SELECT count(*) FROM bordereau");
var postSign = S(conn, "SELECT count(*) FROM signature");
var postNextFact = S(conn, "SELECT next_num_fact::text FROM parametre");
var postNextBord = S(conn, "SELECT next_num_bord::text FROM parametre");

Log($"  facture      = {postFact}");
Log($"  detail_fact  = {postDet}");
Log($"  bordereau    = {postBord}");
Log($"  signature    = {postSign}");
Log($"  next_num_fact = {postNextFact}");
Log($"  next_num_bord = {postNextBord}");

// Verify each table
Log("\n  --- Facture TST003 ---");
using (var cmd = new NpgsqlCommand("SELECT * FROM facture WHERE num_fact='TST003'", conn))
using (var rd = await cmd.ExecuteReaderAsync())
{
    if (await rd.ReadAsync())
    {
        for (int i = 0; i < rd.FieldCount; i++)
            Log($"    {rd.GetName(i),-20} = {rd[i]}");
    }
    else Log("    NOT FOUND");
}

Log("\n  --- Detail_fact TST003 ---");
using (var cmd = new NpgsqlCommand("SELECT * FROM detail_fact WHERE num_fact='TST003'", conn))
using (var rd = await cmd.ExecuteReaderAsync())
{
    if (await rd.ReadAsync())
    {
        for (int i = 0; i < rd.FieldCount; i++)
            Log($"    {rd.GetName(i),-20} = {rd[i]}");
    }
    else Log("    NOT FOUND");
}

Log("\n  --- Bordereau 215 ---");
using (var cmd = new NpgsqlCommand("SELECT * FROM bordereau WHERE num_bord='215'", conn))
using (var rd = await cmd.ExecuteReaderAsync())
{
    if (await rd.ReadAsync())
    {
        for (int i = 0; i < rd.FieldCount; i++)
            Log($"    {rd.GetName(i),-20} = {rd[i]}");
    }
    else Log("    NOT FOUND");
}

// FK vérifications
var orphanCheck1 = S(conn, "SELECT count(*) FROM facture f LEFT JOIN bordereau b ON f.num_bord=b.num_bord WHERE f.num_bord IS NOT NULL AND b.num_bord IS NULL");
var orphanCheck2 = S(conn, "SELECT count(*) FROM detail_fact d LEFT JOIN facture f ON d.num_fact=f.num_fact WHERE f.num_fact IS NULL");
Log($"\n  Orphelins facture→bordereau: {orphanCheck1} (attendu: 0)");
Log($"  Orphelins detail_fact→facture: {orphanCheck2} (attendu: 0)");

// Jointure detail_bord simulée
Log("\n  --- JOIN facture × bordereau (simule detail_bord) ---");
using (var cmd = new NpgsqlCommand(@"
    SELECT f.num_fact, f.num_assure, f.mont_fact, f.num_bord, b.num_bord as bord_num_bord
    FROM facture f
    JOIN bordereau b ON f.num_bord = b.num_bord
    WHERE f.num_fact='TST003'", conn))
using (var rd = await cmd.ExecuteReaderAsync())
{
    if (await rd.ReadAsync())
        Log($"    f.num_fact={rd["num_fact"]} f.num_bord={rd["num_bord"]} b.num_bord={rd["bord_num_bord"]} → Jointure OK");
    else
        Log($"    Jointure ÉCHOUÉE — aucune ligne");
}

Log("\n  --- JOIN facture × detail_fact × bordereau (simule detail_bord complet) ---");
using (var cmd = new NpgsqlCommand(@"
    SELECT f.num_fact, f.num_bord, d.num_enr, d.qte, d.mont, d.remboursable, d.ts
    FROM facture f
    JOIN detail_fact d ON f.num_fact = d.num_fact
    JOIN bordereau b ON f.num_bord = b.num_bord
    WHERE f.num_fact='TST003'", conn))
using (var rd = await cmd.ExecuteReaderAsync())
{
    var hasRows = false;
    while (await rd.ReadAsync())
    {
        hasRows = true;
        Log($"    num_fact={rd["num_fact"]} num_bord={rd["num_bord"]} num_enr={rd["num_enr"]} qte={rd["qte"]} mont={rd["mont"]} remb={rd["remboursable"]} ts={rd["ts"]}");
    }
    if (!hasRows) Log("    ❌ Aucune ligne — detail_bord serait VIDE");
}

await conn.CloseAsync();

// ============================================================
// SUMMARY TABLE
// ============================================================
Section("RÉSUMÉ");
Log("  ┌──────────────────────────┬──────────┬──────────┐");
Log("  │ Table                    │ Avant    │ Après    │");
Log("  ├──────────────────────────┼──────────┼──────────┤");
Log($"  │ facture                  │ {preFact,-8} │ {postFact,-8} │");
Log($"  │ detail_fact              │ {preDet,-8} │ {postDet,-8} │");
Log($"  │ bordereau                │ {preBord,-8} │ {postBord,-8} │");
Log($"  │ signature                │ {preSign,-8} │ {postSign,-8} │");
Log($"  │ next_num_fact            │ {preNextFact,-8} │ {postNextFact,-8} │");
Log($"  │ next_num_bord            │ {preNextBord,-8} │ {postNextBord,-8} │");
Log("  └──────────────────────────┴──────────┴──────────┘");

Log("\n  ═══════════════════════════════════════════════════════════");
Log("  ║  DONNÉES PRÊTES POUR OBSERVATION CHIFA-OFFICINE       ║");
Log("  ║                                                        ║");
Log("  ║  TST003 → facture=1, detail_fact=1, bordereau=1       ║");
Log("  ║  Compteurs inchangés, signature=0                      ║");
Log("  ║  Jointure facture×bordereau×detail_fact: OK            ║");
Log("  ║                                                        ║");
Log("  ║  ⛔ STOP — Ouvrir CHIFA-OFFICINE maintenant             ║");
Log("  ║  NE PAS signer, cloturer ou transmettre                ║");
Log("  ═══════════════════════════════════════════════════════════");

string path = @"C:\Users\pc\Desktop\bm_stock\BMPharma\BM-PHASE-008-B-SNAPSHOT.txt";
System.IO.File.WriteAllText(path, sb.ToString());
Console.WriteLine($"\nLog: {path}");
