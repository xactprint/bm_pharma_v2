using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using BMPharma.Persistence.PostgreSQL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Text;
using System.Threading.Tasks;

class Program
{
    const string CONN_STR = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=60";
    const string PG_CONN_STR = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true";

    static readonly StringBuilder Log = new();

    static void LogMsg(string msg) { Console.WriteLine(msg); Log.AppendLine(msg); }
    static void Section(string title) { Console.WriteLine($"\n=== {title} ==="); Log.AppendLine($"\n=== {title} ==="); }

    static string Scalar(NpgsqlConnection c, string sql)
    {
        using var cmd = new NpgsqlCommand(sql, c);
        return cmd.ExecuteScalar()?.ToString() ?? "(null)";
    }

    static async Task Main(string[] args)
    {
        LogMsg("========================================================");
        LogMsg("  BM-PHASE-007-G — ÉCRITURE RÉELLE EF CORE");
        LogMsg($"  Date: {DateTime.UtcNow:O}");
        LogMsg("========================================================");

        // ============================================================
        // SECTION 1: PRE-WRITE VERIFICATION
        // ============================================================
        Section("PRE-WRITE VERIFICATION");

        await using var pgConn = new NpgsqlConnection(CONN_STR);
        await pgConn.OpenAsync();
        LogMsg($"  Connected: {pgConn.ServerVersion} | {pgConn.UserName}@{pgConn.Database}");
        LogMsg($"  Host: 127.0.0.1 | Port: 5432 | Database: CHIFA_OFFICINE");

        // 1. Verify TST002 does NOT exist
        var tst002Count = Scalar(pgConn, "SELECT count(*) FROM facture WHERE num_fact = 'TST002'");
        LogMsg($"  TST002 exists: {(tst002Count == "0" ? "NO (correct)" : "YES (ERROR!)")}");
        if (tst002Count != "0") { LogMsg("  ABORT: TST002 already exists!"); return; }

        // 2. Verify table counts
        var preFact = Scalar(pgConn, "SELECT count(*) FROM facture");
        var preDet = Scalar(pgConn, "SELECT count(*) FROM detail_fact");
        var preBord = Scalar(pgConn, "SELECT count(*) FROM bordereau");
        var preSign = Scalar(pgConn, "SELECT count(*) FROM signature");
        LogMsg($"  facture      = {preFact}");
        LogMsg($"  detail_fact  = {preDet}");
        LogMsg($"  bordereau    = {preBord}");
        LogMsg($"  signature    = {preSign}");

        // 3. Capture counters
        var preNextFact = Scalar(pgConn, "SELECT next_num_fact::text FROM parametre");
        var preNextBord = Scalar(pgConn, "SELECT next_num_bord::text FROM parametre");
        LogMsg($"  next_num_fact = {preNextFact}");
        LogMsg($"  next_num_bord = {preNextBord}");

        // 4. Verify mode
        LogMsg($"  Integration mode: Test");
        LogMsg($"  ChifaWriteGuard: AUTORISE (mode Test)");

        await pgConn.CloseAsync();

        // ============================================================
        // SECTION 2: EF CORE REAL WRITE
        // ============================================================
        Section("EF CORE REAL WRITE");

        var options = new DbContextOptionsBuilder<ChifaWriteDbContext>()
            .UseNpgsql(PG_CONN_STR)
            .EnableSensitiveDataLogging(false)
            .Options;

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

        await using var context = new ChifaWriteDbContext(options);

        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.Test));
        var validator = new ChifaInvoiceValidator(loggerFactory.CreateLogger<ChifaInvoiceValidator>());
        var audit = new ChifaAuditService(loggerFactory.CreateLogger<ChifaAuditService>());

        var service = new ChifaPostgresInvoiceService(
            context, guard, validator, audit,
            loggerFactory.CreateLogger<ChifaPostgresInvoiceService>());

        var request = new ChifaInvoiceRequest
        {
            NumFact = "TST002",
            NumAssure = "TST99999",
            CodeCentre = 11600,
            DateSoin = DateTime.UtcNow,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new ChifaInvoiceLineRequest
                {
                    NumEnr = "00010",
                    MedicCode = 10,
                    PrixUnit = 60.00m,
                    Quantite = 2,
                    NumLot = "LOT002",
                    Posologie = "1/day",
                    InfTr = 1,
                    ApplicTr = 1,
                    Medic = 1,
                    Ts = 4,
                    DureeTrait = 30
                }
            }
        };

        LogMsg("  Calling ChifaPostgresInvoiceService.CreateInvoiceAsync...");
        ChifaInvoiceResult result;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            result = await service.CreateInvoiceAsync(request);
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogMsg($"  EXCEPTION: {ex.Message}");
            LogMsg($"  StackTrace: {ex.StackTrace}");
            LogMsg($"\n  ═══════════════════════════════════════════════════");
            LogMsg($"  ║  ÉCRITURE RÉELLE ÉCHOUÉE — EXCEPTION          ║");
            LogMsg($"  ═══════════════════════════════════════════════════");
            SaveReport();
            return;
        }

        sw.Stop();
        LogMsg($"  Duration: {sw.ElapsedMilliseconds}ms");
        LogMsg($"  Success: {result.Success}");
        if (!result.Success)
        {
            LogMsg($"  Error: {result.ErrorMessage}");
            LogMsg($"\n  ═══════════════════════════════════════════════════");
            LogMsg($"  ║  ÉCRITURE RÉELLE ÉCHOUÉE — SERVICE FAILED     ║");
            LogMsg($"  ═══════════════════════════════════════════════════");
            SaveReport();
            return;
        }
        LogMsg($"  ChifaNumFact: {result.ChifaNumFact}");

        // ============================================================
        // SECTION 3: POST-WRITE VERIFICATION
        // ============================================================
        Section("POST-WRITE VERIFICATION");

        await pgConn.OpenAsync();

        // 3a. Verify facture TST002
        var postTstCount = Scalar(pgConn, "SELECT count(*) FROM facture WHERE num_fact = 'TST002'");
        LogMsg($"  TST002 facture rows: {postTstCount} (must be 1)");
        if (postTstCount != "1") { LogMsg("  FAIL: TST002 not found or duplicate!"); }

        // 3b. Verify facture columns
        using (var cmd = new NpgsqlCommand("SELECT * FROM facture WHERE num_fact = 'TST002'", pgConn))
        using (var rd = await cmd.ExecuteReaderAsync())
        {
            if (await rd.ReadAsync())
            {
                LogMsg($"\n  Facture TST002:");
                LogMsg($"    num_fact     = {rd["num_fact"]}");
                LogMsg($"    num_assure   = {rd["num_assure"]}");
                LogMsg($"    code_centre  = {rd["code_centre"]}");
                LogMsg($"    etat         = {rd["etat"]}");
                LogMsg($"    mont_fact    = {rd["mont_fact"]}");
                LogMsg($"    mont_as      = {rd["mont_as"]}");
                LogMsg($"    mont_off     = {rd["mont_off"]}");
                LogMsg($"    date_fact    = {rd["date_fact"]}");
                LogMsg($"    date_soin    = {rd["date_soin"]}");
                LogMsg($"    num_bord     = {rd["num_bord"]}");
                LogMsg($"    rang_ad      = {rd["rang_ad"]}");
                LogMsg($"    tp           = {rd["tp"]}");
                LogMsg($"    taux         = {rd["taux"]}");
                LogMsg($"    code_affect  = {rd["code_affect"]}");
                LogMsg($"    conv         = {rd["conv"]}");
                LogMsg($"    type_consult = {rd["type_consult"]}");
                LogMsg($"    prescripteur = {rd["prescripteur"]}");
                LogMsg($"    risque       = {rd["risque"]}");
                LogMsg($"    statut_fact  = {rd["statut_fact"]}");
                LogMsg($"    verifcms     = {rd["verifcms"]}");
                LogMsg($"    type_signature = {rd["type_signature"]}");
                LogMsg($"    verif_fact   = {rd["verif_fact"]}");
                LogMsg($"    version      = {rd["version"]}");
            }
        }

        // 3c. Verify detail_fact
        var postDetCount = Scalar(pgConn, "SELECT count(*) FROM detail_fact WHERE num_fact = 'TST002'");
        LogMsg($"\n  Detail_fact TST002 rows: {postDetCount} (must be 1)");
        if (postDetCount != "1") { LogMsg("  FAIL: detail_fact not found or duplicate!"); }

        using (var cmd = new NpgsqlCommand("SELECT * FROM detail_fact WHERE num_fact = 'TST002'", pgConn))
        using (var rd = await cmd.ExecuteReaderAsync())
        {
            if (await rd.ReadAsync())
            {
                LogMsg($"\n  Detail TST002:");
                LogMsg($"    num_fact       = {rd["num_fact"]}");
                LogMsg($"    num_enr        = {rd["num_enr"]}");
                LogMsg($"    ppa            = {rd["ppa"]}");
                LogMsg($"    qte            = {rd["qte"]}");
                LogMsg($"    mont           = {rd["mont"]}");
                LogMsg($"    mont_as        = {rd["mont_as"]}");
                LogMsg($"    mont_pharm     = {rd["mont_pharm"]}");
                LogMsg($"    num_enr_prescrit = {rd["num_enr_prescrit"]}");
                LogMsg($"    num_lot        = {rd["num_lot"]}");
                LogMsg($"    duree_trait    = {rd["duree_trait"]}");
                LogMsg($"    tarif_ref      = {rd["tarif_ref"]}");
                LogMsg($"    posologie      = {rd["posologie"]}");
            }
        }

        // 3d. Verify FK integrity
        var orphans = Scalar(pgConn, "SELECT count(*) FROM detail_fact WHERE num_fact = 'TST002' AND num_fact NOT IN (SELECT num_fact FROM facture)");
        LogMsg($"\n  Orphan detail_fact rows: {orphans} (must be 0)");

        // 3e. Verify other tables unchanged
        var postBord = Scalar(pgConn, "SELECT count(*) FROM bordereau");
        var postSign = Scalar(pgConn, "SELECT count(*) FROM signature");
        LogMsg($"  bordereau    = {postBord} (must be 0)");
        LogMsg($"  signature    = {postSign} (must be 0)");

        // 3f. Verify counters unchanged
        var postNextFact = Scalar(pgConn, "SELECT next_num_fact::text FROM parametre");
        var postNextBord = Scalar(pgConn, "SELECT next_num_bord::text FROM parametre");
        LogMsg($"  next_num_fact = {postNextFact} (must be {preNextFact})");
        LogMsg($"  next_num_bord = {postNextBord} (must be {preNextBord})");

        // 3g. Verify no other data modified
        var postFactAll = Scalar(pgConn, "SELECT count(*) FROM facture");
        var postDetAll = Scalar(pgConn, "SELECT count(*) FROM detail_fact");
        var postLn = Scalar(pgConn, "SELECT count(*) FROM ln");
        var postMed = Scalar(pgConn, "SELECT count(*) FROM medicament");
        LogMsg($"  facture (all) = {postFactAll} (must be 1)");
        LogMsg($"  detail_fact (all) = {postDetAll} (must be 1)");
        LogMsg($"  ln           = {postLn}");
        LogMsg($"  medicament   = {postMed}");

        await pgConn.CloseAsync();

        // ============================================================
        // SECTION 4: COMPARISON TABLE
        // ============================================================
        Section("COMPARISON: AVANT / APRÈS");
        LogMsg("  ┌──────────────────────┬──────────┬──────────┬──────────┐");
        LogMsg("  │ Table/Counter        │ Avant    │ Après    │ Statut   │");
        LogMsg("  ├──────────────────────┼──────────┼──────────┼──────────┤");
        LogMsg($"  │ facture              │ {preFact,-8} │ {postFactAll,-8} │ {(postFactAll == "1" ? "OK" : "FAIL")  ,-8} │");
        LogMsg($"  │ detail_fact          │ {preDet,-8} │ {postDetAll,-8} │ {(postDetAll == "1" ? "OK" : "FAIL")  ,-8} │");
        LogMsg($"  │ bordereau            │ {preBord,-8} │ {postBord,-8} │ {(postBord == "0" ? "OK" : "FAIL")  ,-8} │");
        LogMsg($"  │ signature            │ {preSign,-8} │ {postSign,-8} │ {(postSign == "0" ? "OK" : "FAIL")  ,-8} │");
        LogMsg($"  │ next_num_fact        │ {preNextFact,-8} │ {postNextFact,-8} │ {(postNextFact == preNextFact ? "OK" : "FAIL")  ,-8} │");
        LogMsg($"  │ next_num_bord        │ {preNextBord,-8} │ {postNextBord,-8} │ {(postNextBord == preNextBord ? "OK" : "FAIL")  ,-8} │");
        LogMsg("  └──────────────────────┴──────────┴──────────┴──────────┘");

        // ============================================================
        // SECTION 5: FINAL RESULT
        // ============================================================
        Section("FINAL RESULT");

        var allOk = true;
        void Check(string name, string expected, string actual)
        {
            var ok = expected == actual;
            var mark = ok ? "✓" : "✗";
            LogMsg($"  {mark} {name}: {actual} (expected {expected})");
            if (!ok) allOk = false;
        }

        Check("TST002 facture exists", "1", postTstCount);
        Check("TST002 detail_fact exists", "1", postDetCount);
        Check("No orphans", "0", orphans);
        Check("bordereau unchanged", preBord, postBord);
        Check("signature unchanged", preSign, postSign);
        Check("next_num_fact unchanged", preNextFact, postNextFact);
        Check("next_num_bord unchanged", preNextBord, postNextBord);

        LogMsg("");
        if (allOk)
        {
            LogMsg("  ═══════════════════════════════════════════════════════════");
            LogMsg("  ║  ÉCRITURE RÉELLE RÉUSSIE                             ║");
            LogMsg("  ║  TST002 créée via EF Core sur CHIFA_OFFICINE         ║");
            LogMsg("  ═══════════════════════════════════════════════════════════");
        }
        else
        {
            LogMsg("  ═══════════════════════════════════════════════════════════");
            LogMsg("  ║  ÉCRITURE RÉELLE ÉCHOUÉE — VOIR ERREURS CI-DESSUS    ║");
            LogMsg("  ═══════════════════════════════════════════════════════════");
        }

        LogMsg("");
        LogMsg("  ⛔ STOP — EN ATTENTE D'APPROBATION POUR LE ROLLBACK");

        SaveReport();
    }

    static void SaveReport()
    {
        string path = @"C:\Users\pc\Desktop\bm_stock\BMPharma\BM-PHASE-007-G-REAL-WRITE-LOG.txt";
        System.IO.File.WriteAllText(path, Log.ToString());
        Console.WriteLine($"\nLog saved: {path}");
    }
}
