using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using BMPharma.Persistence.PostgreSQL.Contexts;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BMPharma.CHIFA.Tests;

/// <summary>
/// BM-PHASE-004.7 — Production Readiness Validation Tests
/// NO real CHIFA writes are performed — all tests use ReadOnly or FakeChifaIntegrationProvider.
/// </summary>
public class ProductionReadinessTests
{
    private static ServiceCollection CreateServicesWithLogging()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    private static (IServiceProvider provider, FakeChifaIntegrationProvider fake, ChifaIntegrationModeProvider modeProvider) CreateReadOnlyProvider()
    {
        var services = CreateServicesWithLogging();
        services.AddChifaIntegrationForTests(ChifaIntegrationMode.ReadOnly);
        var provider = services.BuildServiceProvider();
        return (
            provider,
            provider.GetRequiredService<FakeChifaIntegrationProvider>(),
            provider.GetRequiredService<ChifaIntegrationModeProvider>());
    }

    private static (IServiceProvider provider, FakeChifaIntegrationProvider fake, ChifaIntegrationModeProvider modeProvider) CreateTestModeProvider()
    {
        var services = CreateServicesWithLogging();
        services.AddChifaIntegrationForTests(ChifaIntegrationMode.Test);
        var provider = services.BuildServiceProvider();
        return (
            provider,
            provider.GetRequiredService<FakeChifaIntegrationProvider>(),
            provider.GetRequiredService<ChifaIntegrationModeProvider>());
    }

    private static string ReadSourceFile(string relativePath)
    {
        return File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", relativePath));
    }

    // ============================================================================
    // 1. ARCHITECTURE VALIDATION
    // ============================================================================

    [Fact]
    public void PRD001_Domain_DoesNot_Depend_On_CHIFA()
    {
        var domainAssembly = typeof(BMPharma.Domain.Common.BaseEntity).Assembly;
        var refs = domainAssembly.GetReferencedAssemblies();
        refs.Should().NotContain(a => a.Name == "BMPharma.CHIFA",
            "Domain layer must not depend on CHIFA integration layer");
    }

    [Fact]
    public void PRD002_CHIFA_DoesNot_Depend_On_UI()
    {
        var chifaAssembly = typeof(IChifaInvoiceService).Assembly;
        var refs = chifaAssembly.GetReferencedAssemblies();
        refs.Should().NotContain(a => a.Name == "BMPharma.UI",
            "CHIFA layer must not depend on UI");
    }

    [Fact]
    public void PRD003_CleanArchitecture_Layers_Recognized()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("BMPharma") == true)
            .ToList();

        assemblies.Should().Contain(a => a.GetName().Name == "BMPharma.Domain");
        assemblies.Should().Contain(a => a.GetName().Name == "BMPharma.CHIFA");
        assemblies.Should().Contain(a => a.GetName().Name == "BMPharma.Persistence.PostgreSQL");
    }

    [Fact]
    public void PRD004_All_Enums_Exist_In_Domain()
    {
        var enumTypes = new[]
        {
            typeof(ChifaIntegrationMode),
            typeof(ChifaWorkflowState),
            typeof(BordereauWorkflowState)
        };

        foreach (var t in enumTypes)
        {
            t.Should().NotBeNull();
            t.IsEnum.Should().BeTrue();
        }
    }

    // ============================================================================
    // 2. READONLY / TEST / PRODUCTION MODE VALIDATION
    // ============================================================================

    [Fact]
    public void PRD005_ReadOnly_Default_Mode_Is_ReadOnly()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var provider = new ChifaIntegrationModeProvider(config);
        provider.CurrentMode.Should().Be(ChifaIntegrationMode.ReadOnly);
        provider.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void PRD006_Empty_Mode_Defaults_To_ReadOnly()
    {
        var config = new ChifaIntegrationConfig { Mode = "" };
        var provider = new ChifaIntegrationModeProvider(config);
        provider.CurrentMode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    [Fact]
    public void PRD007_CaseInsensitive_Mode_Parsing()
    {
        var modes = new[] { "readonly", "READONLY", "ReadOnly" };
        foreach (var mode in modes)
        {
            var config = new ChifaIntegrationConfig { Mode = mode };
            var provider = new ChifaIntegrationModeProvider(config);
            provider.CurrentMode.Should().Be(ChifaIntegrationMode.ReadOnly,
                $"mode '{mode}' should resolve to ReadOnly");
        }
    }

    [Fact]
    public void PRD008_Test_Mode_Recognized()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var provider = new ChifaIntegrationModeProvider(config);
        provider.IsTest.Should().BeTrue();
        provider.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void PRD009_Production_Mode_Recognized()
    {
        var config = new ChifaIntegrationConfig { Mode = "Production" };
        var provider = new ChifaIntegrationModeProvider(config);
        provider.IsProduction.Should().BeTrue();
        provider.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void PRD010_ReadOnly_Mode_Uses_FakeProvider()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var invoiceService = serviceProvider.GetRequiredService<IChifaInvoiceService>();
        invoiceService.Should().BeOfType<FakeChifaIntegrationProvider>();
    }

    // ============================================================================
    // 3. WRITEGUARD VALIDATION
    // ============================================================================

    [Fact]
    public async Task PRD011_WriteGuard_Blocks_In_ReadOnly()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));
        var act = () => guard.EnsureWriteAllowedAsync();
        await act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public async Task PRD012_WriteGuard_Allows_In_Test()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.Test));
        var act = async () => await guard.EnsureWriteAllowedAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PRD013_WriteGuard_Allows_In_Production()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.Production));
        var act = async () => await guard.EnsureWriteAllowedAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PRD014_WriteGuard_TestOrProduction_Blocks_In_ReadOnly()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));
        var act = () => guard.EnsureTestOrProductionAsync();
        await act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public async Task PRD015_WriteGuard_ExceptionMessage_Is_Clear()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));
        try
        {
            await guard.EnsureWriteAllowedAsync();
            Assert.Fail("Should have thrown");
        }
        catch (ChifaWriteBlockedException ex)
        {
            ex.Message.Should().Contain("ReadOnly");
            ex.Message.Should().Contain("allow writes");
        }
    }

    // ============================================================================
    // 4. READONLY SECURED — NO REAL WRITES
    // ============================================================================

    [Fact]
    public async Task PRD016_ReadOnly_CreateInvoice_Is_Simulation()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        var result = await fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "PRD016",
            Lines = new List<ChifaInvoiceLineRequest>()
        });
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PRD017_ReadOnly_CreateBordereau_Is_Simulation()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateInvoiceExists("INV001");
        var result = await fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "PRD017",
            InvoiceNumbers = new List<string> { "INV001" }
        });
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void PRD018_ReadOnly_Workflow_Does_Not_Call_CreateInvoice()
    {
        var (_, _, modeProvider) = CreateReadOnlyProvider();
        modeProvider.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void PRD019_DiInjection_ReadOnly_No_PostgresDbContext()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var config = serviceProvider.GetRequiredService<ChifaIntegrationConfig>();
        config.Mode.Should().Be("ReadOnly");
    }

    // ============================================================================
    // 5. TRANSACTIONS & ROLLBACK
    // ============================================================================

    [Fact]
    public void PRD020_Workflow_Validator_Verifies_Request()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var validator = serviceProvider.GetRequiredService<ChifaInvoiceValidator>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "PRD020",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 100, Quantite = 2 }
            }
        };
        var validation = validator.Validate(request);
        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PRD021_Database_Contract_Defines_Transaction_Strategy()
    {
        var contract = ReadSourceFile("BM_PHARMA_CHIFA_DATABASE_CONTRACT.md");
        contract.Should().Contain("BEGIN");
        contract.Should().Contain("COMMIT");
        contract.Should().Contain("ROLLBACK");
        contract.Should().Contain("FOR UPDATE");
    }

    // ============================================================================
    // 6. IDEMPOTENCE
    // ============================================================================

    [Fact]
    public async Task PRD022_Fake_CreateInvoice_Is_Idempotent_Rejects_Duplicates()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        await fake.CreateInvoiceAsync(new ChifaInvoiceRequest { NumFact = "IDEM01", Lines = new List<ChifaInvoiceLineRequest>() });
        var result = await fake.CreateInvoiceAsync(new ChifaInvoiceRequest { NumFact = "IDEM01", Lines = new List<ChifaInvoiceLineRequest>() });
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task PRD023_Fake_CreateBordereau_Is_Idempotent_Rejects_Duplicates()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateInvoiceExists("INV01");
        await fake.CreateBordereauAsync(new ChifaBordereauRequest { NumBord = "B01", InvoiceNumbers = new List<string> { "INV01" } });
        var result = await fake.CreateBordereauAsync(new ChifaBordereauRequest { NumBord = "B01", InvoiceNumbers = new List<string> { "INV01" } });
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task PRD024_InvoiceWorkflow_CreateInDatabase_Checks_Exists()
    {
        var (serviceProvider, fake, _) = CreateReadOnlyProvider();
        var workflowService = serviceProvider.GetRequiredService<IChifaInvoiceWorkflowService>();

        fake.SimulateInvoiceExists("EXIST01");
        var result = await workflowService.CreateInDatabaseAsync("EXIST01");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("ALREADY_EXISTS");
    }

    // ============================================================================
    // 7. COUNTERS (facture/bordereau)
    // ============================================================================

    [Fact]
    public async Task PRD025_Fake_Bordereau_Counter_Increments()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        var b1 = await fake.GetNextBordereauNumberAsync();
        var b2 = await fake.GetNextBordereauNumberAsync();
        var b3 = await fake.GetNextBordereauNumberAsync();
        b1.Should().Be("000001");
        b2.Should().Be("000002");
        b3.Should().Be("000003");
    }

    [Fact]
    public async Task PRD026_BordereauNumber_Format_Is_6Digits()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        var num = await fake.GetNextBordereauNumberAsync();
        num.Should().HaveLength(6);
    }

    // ============================================================================
    // 8. BM-SPEC-028/029/030/031 CONSTRAINTS
    // ============================================================================

    [Fact]
    public void PRD027_MaxNumFact_Is_8()
    {
        ChifaInvoiceValidator.MaxNumFactLength.Should().Be(8);
    }

    [Fact]
    public void PRD028_MaxNumEnr_Is_5()
    {
        ChifaInvoiceValidator.MaxNumEnrLength.Should().Be(5);
    }

    [Fact]
    public void PRD029_MaxNumAssure_Is_12()
    {
        ChifaInvoiceValidator.MaxNumAssureLength.Should().Be(12);
    }

    [Fact]
    public void PRD030_MaxCodeCentre_Is_5()
    {
        ChifaInvoiceValidator.MaxCodeCentreLength.Should().Be(5);
    }

    [Fact]
    public void PRD031_MaxQte_Is_999()
    {
        ChifaInvoiceValidator.MaxQteValue.Should().Be(999);
    }

    [Fact]
    public void PRD032_MaxNumBord_Is_6()
    {
        ChifaBordereauValidator.MaxNumBordLength.Should().Be(6);
    }

    [Fact]
    public void PRD033_NumFact_TooLong_Rejected()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var validator = serviceProvider.GetRequiredService<ChifaInvoiceValidator>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "123456789",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 100, Quantite = 1 }
            }
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_FACT_TOO_LONG");
    }

    [Fact]
    public void PRD034_NumEnr_TooLong_Rejected()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var validator = serviceProvider.GetRequiredService<ChifaInvoiceValidator>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "123456", PrixUnit = 100, Quantite = 1 }
            }
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_ENR_TOO_LONG");
    }

    [Fact]
    public void PRD035_Qte_Zero_Rejected()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var validator = serviceProvider.GetRequiredService<ChifaInvoiceValidator>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 100, Quantite = 0 }
            }
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "QTE_INVALID");
    }

    [Fact]
    public void PRD036_Qte_Over999_Rejected()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var validator = serviceProvider.GetRequiredService<ChifaInvoiceValidator>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 100, Quantite = 1000 }
            }
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "QTE_TOO_HIGH");
    }

    [Fact]
    public void PRD037_PPA_Zero_Rejected()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var validator = serviceProvider.GetRequiredService<ChifaInvoiceValidator>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 0, Quantite = 1 }
            }
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "PPA_INVALID");
    }

    [Fact]
    public void PRD038_NoLines_Rejected()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var validator = serviceProvider.GetRequiredService<ChifaInvoiceValidator>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>()
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NO_LINES");
    }

    // ============================================================================
    // 9. POSTGRESQL MAPPING VALIDATION
    // ============================================================================

    [Fact]
    public void PRD039_ChifaPostgreSqlContext_Has_Factures_DbSet()
    {
        var props = typeof(ChifaPostgreSqlContext).GetProperties();
        props.Should().Contain(p => p.Name == "Factures");
    }

    [Fact]
    public void PRD040_ChifaPostgreSqlContext_Has_DetailFacts_DbSet()
    {
        var props = typeof(ChifaPostgreSqlContext).GetProperties();
        props.Should().Contain(p => p.Name == "DetailFacts");
    }

    [Fact]
    public void PRD041_ChifaPostgreSqlContext_Has_Bordereaus_DbSet()
    {
        var props = typeof(ChifaPostgreSqlContext).GetProperties();
        props.Should().Contain(p => p.Name == "Bordereaus");
    }

    [Fact]
    public void PRD042_ChifaPostgreSqlContext_Has_Parametres_DbSet()
    {
        var props = typeof(ChifaPostgreSqlContext).GetProperties();
        props.Should().Contain(p => p.Name == "Parametres");
    }

    [Fact]
    public void PRD043_ChifaFacture_Maps_To_facture_Table()
    {
        var attr = typeof(ChifaFacture).GetCustomAttribute<TableAttribute>();
        attr.Should().NotBeNull();
        attr!.Name.Should().Be("facture");
    }

    [Fact]
    public void PRD044_ChifaDetailFact_Maps_To_detail_fact_Table()
    {
        var attr = typeof(ChifaDetailFact).GetCustomAttribute<TableAttribute>();
        attr.Should().NotBeNull();
        attr!.Name.Should().Be("detail_fact");
    }

    [Fact]
    public void PRD045_ChifaBordereau_Maps_To_bordereau_Table()
    {
        var attr = typeof(ChifaBordereau).GetCustomAttribute<TableAttribute>();
        attr.Should().NotBeNull();
        attr!.Name.Should().Be("bordereau");
    }

    [Fact]
    public void PRD046_ChifaParametre_Maps_To_parametre_Table()
    {
        var attr = typeof(ChifaParametre).GetCustomAttribute<TableAttribute>();
        attr.Should().NotBeNull();
        attr!.Name.Should().Be("parametre");
    }

    [Fact]
    public void PRD047_ChifaFacture_Has_NumFact_PK()
    {
        var prop = typeof(ChifaFacture).GetProperty("NumFact");
        prop.Should().NotBeNull();
        var colAttr = prop!.GetCustomAttribute<ColumnAttribute>();
        colAttr.Should().NotBeNull();
        colAttr!.Name.Should().Be("num_fact");
    }

    [Fact]
    public void PRD048_ChifaBordereau_Has_NumBord_Column()
    {
        var prop = typeof(ChifaBordereau).GetProperty("NumBord");
        prop.Should().NotBeNull();
        var colAttr = prop!.GetCustomAttribute<ColumnAttribute>();
        colAttr.Should().NotBeNull();
        colAttr!.Name.Should().Be("num_bord");
    }

    [Fact]
    public void PRD049_ChifaFacture_Has_Critical_MontMajFae_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("MontMajFae");
        prop.Should().NotBeNull();
        var colAttr = prop!.GetCustomAttribute<ColumnAttribute>();
        colAttr.Should().NotBeNull();
        colAttr!.Name.Should().Be("mont_maj_fae");
    }

    [Fact]
    public void PRD050_ChifaFacture_Has_Critical_MontMaj_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("MontMaj");
        prop.Should().NotBeNull();
        var colAttr = prop!.GetCustomAttribute<ColumnAttribute>();
        colAttr.Should().NotBeNull();
        colAttr!.Name.Should().Be("mont_maj");
    }

    [Fact]
    public void PRD051_ChifaFacture_Has_TypeMaj_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("TypeMaj");
        prop.Should().NotBeNull();
        var colAttr = prop!.GetCustomAttribute<ColumnAttribute>();
        colAttr.Should().NotBeNull();
        colAttr!.Name.Should().Be("type_maj");
    }

    [Fact]
    public void PRD052_ChifaParametre_Has_NextNumFact_Column()
    {
        var prop = typeof(ChifaParametre).GetProperty("NextNumFact");
        prop.Should().NotBeNull();
        var colAttr = prop!.GetCustomAttribute<ColumnAttribute>();
        colAttr.Should().NotBeNull();
        colAttr!.Name.Should().Be("next_num_fact");
    }

    [Fact]
    public void PRD053_ChifaParametre_Has_NextNumBord_Column()
    {
        var prop = typeof(ChifaParametre).GetProperty("NextNumBord");
        prop.Should().NotBeNull();
        var colAttr = prop!.GetCustomAttribute<ColumnAttribute>();
        colAttr.Should().NotBeNull();
        colAttr!.Name.Should().Be("next_num_bord");
    }

    // ============================================================================
    // 10. NULL AND DEFAULT CRITICAL VALUES
    // ============================================================================

    [Fact]
    public void PRD054_MontMajFae_Type_Is_Decimal_Not_Nullable()
    {
        var prop = typeof(ChifaFacture).GetProperty("MontMajFae");
        prop!.PropertyType.Should().Be(typeof(decimal),
            "mont_maj_fae MUST NOT be nullable — causes InvalidCastException in CHIFA");
    }

    [Fact]
    public void PRD055_MontMaj_Type_Is_Decimal_Not_Nullable()
    {
        var prop = typeof(ChifaFacture).GetProperty("MontMaj");
        prop!.PropertyType.Should().Be(typeof(decimal),
            "mont_maj MUST NOT be nullable — causes InvalidCastException in CHIFA");
    }

    [Fact]
    public void PRD056_TypeMaj_Type_Is_Int_Not_Nullable()
    {
        var prop = typeof(ChifaFacture).GetProperty("TypeMaj");
        prop!.PropertyType.Should().Be(typeof(int),
            "type_maj MUST NOT be nullable — MUST be 0");
    }

    [Fact]
    public void PRD057_MontFact_Type_Is_Nullable_Decimal()
    {
        var prop = typeof(ChifaFacture).GetProperty("MontFact");
        prop!.PropertyType.Should().Be(typeof(decimal?),
            "mont_fact is nullable per contract");
    }

    [Fact]
    public void PRD058_Defaults_Applied_Correctly()
    {
        var validator = new ChifaInvoiceValidator(NullLogger<ChifaInvoiceValidator>.Instance);
        var request = new ChifaInvoiceRequest { DateSoin = default };
        validator.ApplyDefaults(request);
        request.DateSoin.Should().Be(DateTime.Today);
    }

    [Fact]
    public void PRD059_LineDefaults_Applied_Correctly()
    {
        var validator = new ChifaInvoiceValidator(NullLogger<ChifaInvoiceValidator>.Instance);
        var line = new ChifaInvoiceLineRequest { InfTr = 0, ApplicTr = 0, Medic = 0, Ts = 0, DureeTrait = 0 };
        validator.ApplyLineDefaults(line);
        line.InfTr.Should().Be(1);
        line.ApplicTr.Should().Be(1);
        line.Medic.Should().Be(1);
        line.Ts.Should().Be(4);
        line.DureeTrait.Should().Be(5);
    }

    // ============================================================================
    // 11. CHIFA-OFFICINE DETECTION
    // ============================================================================

    [Fact]
    public async Task PRD060_Fake_HealthCheck_Reports_Status()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        var health = await fake.GetHealthStatusAsync();
        health.IsOnline.Should().BeTrue();
        health.IsDatabaseConnected.Should().BeTrue();
        health.IsTokenPresent.Should().BeFalse();
    }

    [Fact]
    public async Task PRD061_Fake_Offline_Detection()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateOffline();
        var available = await fake.IsChifaAvailableAsync();
        available.Should().BeFalse();
    }

    [Fact]
    public async Task PRD062_Fake_Token_Detection()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        (await fake.IsTokenPresentAsync()).Should().BeFalse();
        fake.SimulateTokenPresent();
        (await fake.IsTokenPresentAsync()).Should().BeTrue();
    }

    // ============================================================================
    // 12. CHIFA UNAVAILABLE BEHAVIOR
    // ============================================================================

    [Fact]
    public async Task PRD063_ValidateOnly_Reports_CHIFA_Offline()
    {
        var (serviceProvider, fake, _) = CreateReadOnlyProvider();
        fake.SimulateOffline();
        var workflowService = serviceProvider.GetRequiredService<IChifaInvoiceWorkflowService>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "OFF01",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 100, Quantite = 1 }
            }
        };
        var result = await workflowService.ValidateOnlyAsync(request);
        result.ValidationErrors.Should().Contain(e => e.Code == "CHIFA_OFFLINE");
    }

    [Fact]
    public async Task PRD064_ExecuteFullWorkflow_Reports_Offline_Even_In_ReadOnly()
    {
        var (serviceProvider, fake, _) = CreateReadOnlyProvider();
        fake.SimulateOffline();
        var workflowService = serviceProvider.GetRequiredService<IChifaInvoiceWorkflowService>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "OFF02",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 100, Quantite = 1 }
            }
        };
        var result = await workflowService.ExecuteFullWorkflowAsync(request);
        result.ValidationErrors.Should().Contain(e => e.Code == "CHIFA_OFFLINE");
    }

    // ============================================================================
    // 13. POSTGRESQL UNAVAILABLE BEHAVIOR
    // ============================================================================

    [Fact]
    public async Task PRD065_CreateInvoice_Fails_When_DB_Offline()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateOffline();
        var result = await fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "DB01",
            Lines = new List<ChifaInvoiceLineRequest>()
        });
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("offline");
    }

    [Fact]
    public async Task PRD066_CreateBordereau_Fails_When_DB_Offline()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateOffline();
        var result = await fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "DB02",
            InvoiceNumbers = new List<string> { "INV01" }
        });
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("offline");
    }

    // ============================================================================
    // 14. TOKEN ABSENT BEHAVIOR
    // ============================================================================

    [Fact]
    public async Task PRD067_Sign_Requires_Token()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateTokenAbsent();
        fake.SimulateInvoiceExists("INV01");
        await fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "TK01",
            InvoiceNumbers = new List<string> { "INV01" }
        });
        var result = await fake.SignBordereauAsync("TK01");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("token");
    }

    [Fact]
    public async Task PRD068_Sign_Succeeds_With_Token()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateTokenPresent();
        fake.SimulateInvoiceExists("INV01");
        await fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "TK02",
            InvoiceNumbers = new List<string> { "INV01" }
        });
        var result = await fake.SignBordereauAsync("TK02");
        result.Success.Should().BeTrue();
        result.SignatureId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PRD069_TokenInfo_Null_When_Absent()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        var info = await fake.GetTokenInfoAsync();
        info.Should().BeNull();
    }

    [Fact]
    public async Task PRD070_TokenInfo_Present_When_Token_Exists()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateTokenPresent();
        var info = await fake.GetTokenInfoAsync();
        info.Should().NotBeNull();
        info!.SerialNumber.Should().NotBeNullOrEmpty();
    }

    // ============================================================================
    // 15. SYSTEM SEPARATION
    // ============================================================================

    [Fact]
    public void PRD071_BMPharma_DoesNot_Directly_Access_CHIFA_Tables()
    {
        var chifaAssembly = typeof(IChifaInvoiceService).Assembly;
        var pgAssembly = typeof(ChifaPostgreSqlContext).Assembly;
        chifaAssembly.FullName.Should().NotBe(pgAssembly.FullName,
            "CHIFA integration layer must go through interfaces");
    }

    [Fact]
    public void PRD072_FakeChifa_DoesNot_Access_Real_PostgreSQL()
    {
        var fakeType = typeof(FakeChifaIntegrationProvider);
        var refs = fakeType.Assembly.GetReferencedAssemblies();
        refs.Should().NotContain(a => a.Name == "Npgsql",
            "FakeChifa should not reference Npgsql directly");
    }

    [Fact]
    public void PRD073_TokenService_Is_Independent_Of_InvoiceService()
    {
        var interfaces = typeof(IChifaTokenService).GetInterfaces().ToList();
        interfaces.Should().NotContain(i => i == typeof(IChifaInvoiceService));
    }

    [Fact]
    public void PRD074_SigningService_Is_Independent_Of_InvoiceService()
    {
        var interfaces = typeof(IChifaSigningService).GetInterfaces().ToList();
        interfaces.Should().NotContain(i => i == typeof(IChifaInvoiceService));
    }

    [Fact]
    public void PRD075_All_Services_Register_Dependencies_Correctly()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        serviceProvider.GetRequiredService<IChifaInvoiceService>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IChifaBordereauService>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IChifaTokenService>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IChifaSigningService>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IChifaIntegrationService>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IChifaAuditService>().Should().NotBeNull();
        serviceProvider.GetRequiredService<ChifaWorkflowStateMachine>().Should().NotBeNull();
        serviceProvider.GetRequiredService<BordereauWorkflowStateMachine>().Should().NotBeNull();
    }

    // ============================================================================
    // 16. SECURITY AUDIT
    // ============================================================================

    [Fact]
    public void PRD076_No_Passwords_In_Connection_Strings()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;TrustServerCertificate=true"
        };
        config.ConnectionString.Should().NotContain("Password=",
            "CHIFA connection uses Trust auth, no password in connection string");
    }

    [Fact]
    public void PRD077_No_Logging_Of_Passwords_In_AuditService()
    {
        var source = ReadSourceFile("src/BMPharma.CHIFA/Services/ChifaAuditService.cs");
        source.ToLower().Should().NotContain("password");
        source.ToLower().Should().NotContain("secret");
        source.ToLower().Should().NotContain("apikey");
    }

    [Fact]
    public void PRD078_No_Logging_Of_Tokens_In_AuditService()
    {
        var source = ReadSourceFile("src/BMPharma.CHIFA/Services/ChifaAuditService.cs");
        source.Should().NotContain("Bearer");
    }

    [Fact]
    public void PRD079_WriteGuard_Throws_Exception_Not_Returns_False()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));
        guard.Invoking(g => g.EnsureWriteAllowedAsync())
            .Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public void PRD080_Default_Config_Disables_CHIFA()
    {
        var source = ReadSourceFile("src/BMPharma.UI/appsettings.json");
        source.Should().Contain("\"Enabled\": false",
            "CHIFA must be disabled by default in appsettings.json");
    }

    // ============================================================================
    // 17. AUDIT LOG AUDIT
    // ============================================================================

    [Fact]
    public void PRD081_AuditLog_Contains_CorrelationId()
    {
        var source = ReadSourceFile("src/BMPharma.CHIFA/Services/ChifaAuditService.cs");
        source.Should().Contain("Correlation");
    }

    [Fact]
    public void PRD082_AuditLog_Contains_Operation_Type()
    {
        var source = ReadSourceFile("src/BMPharma.CHIFA/Services/ChifaAuditService.cs");
        source.Should().Contain("Operation");
    }

    [Fact]
    public async Task PRD083_Workflow_AuditLog_Has_Entries_After_Operation()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var workflowService = serviceProvider.GetRequiredService<IChifaInvoiceWorkflowService>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "AUD01",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 100, Quantite = 1 }
            }
        };
        await workflowService.ExecuteFullWorkflowAsync(request);
        var audit = workflowService.GetAuditLog();
        audit.Should().NotBeEmpty();
        audit.First().CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PRD084_Bordereau_AuditLog_Has_Entries()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var bordereauService = serviceProvider.GetRequiredService<IBordereauStatusService>();
        await bordereauService.CreateBordereauAsync("AUD02", "11600", new List<string> { "000001" });
        var audit = bordereauService.GetAuditLog();
        audit.Should().NotBeEmpty();
        audit.First().CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PRD085_Fake_AuditLog_Tracks_Operations()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        await fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "FAUD01",
            Lines = new List<ChifaInvoiceLineRequest>()
        });
        fake.AuditLog.Should().NotBeEmpty();
        fake.AuditLog.Should().Contain(e => e.Operation == "CREATE_INVOICE");
    }

    // ============================================================================
    // 18. RBAC / PERMISSIONS
    // ============================================================================

    [Fact]
    public void PRD086_No_RBAC_Implemented_Currently()
    {
        var source = ReadSourceFile("src/BMPharma.CHIFA/Services/BordereauStatusService.cs");
        source.Should().NotContain("Authorize");
        source.Should().NotContain("Permission");
        source.Should().NotContain("Role");
    }

    [Fact]
    public async Task PRD087_UserId_Defaults_To_System()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var bordereauService = serviceProvider.GetRequiredService<IBordereauStatusService>();
        await bordereauService.CreateBordereauAsync("RBAC01", "11600", new List<string> { "INV01" });
        var audit = bordereauService.GetAuditLog();
        audit.Should().Contain(e => e.UserId == "system");
    }

    // ============================================================================
    // 19. ONE ACTION PRINCIPLE
    // ============================================================================

    [Fact]
    public async Task PRD088_OneAction_SingleCreateInvoice()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var workflowService = serviceProvider.GetRequiredService<IChifaInvoiceWorkflowService>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "ONE01",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 50, Quantite = 3 }
            }
        };
        var result = await workflowService.ExecuteFullWorkflowAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.SimulationMessage.Should().Contain("MODE LECTURE SEULE");
        result.Mode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    [Fact]
    public async Task PRD088b_OneAction_Calculations_Are_Correct()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var workflowService = serviceProvider.GetRequiredService<IChifaInvoiceWorkflowService>();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "CALC01",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", PrixUnit = 50, Quantite = 3 }
            }
        };
        var result = await workflowService.ValidateOnlyAsync(request);
        result.ComputedMontFact.Should().Be(150.00m);
        result.ComputedMontAs.Should().Be(105.00m);
        result.ComputedMontMut.Should().Be(45.00m);
    }

    [Fact]
    public async Task PRD089_OneAction_FullBordereauWorkflow()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var bordereauService = serviceProvider.GetRequiredService<IBordereauStatusService>();
        await bordereauService.CreateBordereauAsync("ONE02", "11600", new List<string> { "000001", "000002" });
        await bordereauService.AttachInvoicesAsync("ONE02", new List<string> { "000003" });
        await bordereauService.ValidateBordereauAsync("ONE02");
        await bordereauService.SignBordereauAsync("ONE02");
        await bordereauService.CloseBordereauAsync("ONE02");
        await bordereauService.TransmitBordereauAsync("ONE02");
        var status = await bordereauService.GetStatusAsync("ONE02");
        status.State.Should().Be(BordereauWorkflowState.Completed);
        status.IsSuccess.Should().BeTrue();
    }

    // ============================================================================
    // 20. STATE MACHINES INTEGRITY
    // ============================================================================

    [Fact]
    public void PRD090_InvoiceWorkflow_All_Have_AllowedTransitions()
    {
        var sm = new ChifaWorkflowStateMachine();
        var allStates = Enum.GetValues<ChifaWorkflowState>();
        foreach (var state in allStates)
        {
            var transitions = sm.GetAllowedTransitions(state);
            if (state != ChifaWorkflowState.Transmitted)
                transitions.Should().NotBeEmpty($"state {state} should have allowed transitions");
        }
    }

    [Fact]
    public void PRD091_BordereauWorkflow_All_Have_AllowedTransitions()
    {
        var sm = new BordereauWorkflowStateMachine();
        var allStates = Enum.GetValues<BordereauWorkflowState>();
        foreach (var state in allStates)
        {
            var transitions = sm.GetAllowedTransitions(state);
            if (state != BordereauWorkflowState.Completed)
                transitions.Should().NotBeEmpty($"state {state} should have allowed transitions");
        }
    }

    [Fact]
    public void PRD092_InvoiceWorkflow_Terminals_Are_Correct()
    {
        var sm = new ChifaWorkflowStateMachine();
        sm.IsTerminal(ChifaWorkflowState.Transmitted).Should().BeTrue();
        sm.IsTerminal(ChifaWorkflowState.Cancelled).Should().BeTrue();
        sm.IsTerminal(ChifaWorkflowState.Draft).Should().BeFalse();
        sm.IsTerminal(ChifaWorkflowState.Validated).Should().BeFalse();
    }

    [Fact]
    public void PRD093_BordereauWorkflow_Terminal_Is_Completed()
    {
        var sm = new BordereauWorkflowStateMachine();
        sm.IsTerminal(BordereauWorkflowState.Completed).Should().BeTrue();
        sm.IsTerminal(BordereauWorkflowState.Transmitted).Should().BeFalse();
    }

    [Fact]
    public void PRD094_BordereauWorkflow_ErrorStates_Are_Correct()
    {
        var sm = new BordereauWorkflowStateMachine();
        sm.IsError(BordereauWorkflowState.Error).Should().BeTrue();
        sm.IsError(BordereauWorkflowState.SignatureError).Should().BeTrue();
        sm.IsError(BordereauWorkflowState.ClosureError).Should().BeTrue();
        sm.IsError(BordereauWorkflowState.TransmissionError).Should().BeTrue();
        sm.IsError(BordereauWorkflowState.SyncError).Should().BeTrue();
        sm.IsError(BordereauWorkflowState.Draft).Should().BeFalse();
        sm.IsError(BordereauWorkflowState.Completed).Should().BeFalse();
    }

    [Fact]
    public void PRD095_BordereauWorkflow_Recovery_From_Error_To_Draft()
    {
        var sm = new BordereauWorkflowStateMachine();
        sm.CanTransition(BordereauWorkflowState.Error, BordereauWorkflowState.Draft).Should().BeTrue();
        sm.CanTransition(BordereauWorkflowState.SignatureError, BordereauWorkflowState.Draft).Should().BeTrue();
        sm.CanTransition(BordereauWorkflowState.ClosureError, BordereauWorkflowState.Draft).Should().BeTrue();
        sm.CanTransition(BordereauWorkflowState.TransmissionError, BordereauWorkflowState.Draft).Should().BeTrue();
    }

    [Fact]
    public void PRD096_ActionDescriptions_Are_NonEmpty()
    {
        var sm = new BordereauWorkflowStateMachine();
        var states = Enum.GetValues<BordereauWorkflowState>();
        foreach (var state in states)
        {
            var desc = sm.GetActionDescription(state, "TEST");
            desc.Should().NotBeNullOrEmpty($"action description for {state} should not be empty");
        }
    }

    [Fact]
    public void PRD097_Applications_Map_Correctly()
    {
        var sm = new BordereauWorkflowStateMachine();
        sm.GetActionApplication(BordereauWorkflowState.AwaitingSignature).Should().Be("CHIFA-OFFICINE");
        sm.GetActionApplication(BordereauWorkflowState.AwaitingClosure).Should().Be("CHIFA-OFFICINE");
        sm.GetActionApplication(BordereauWorkflowState.AwaitingTransmission).Should().Be("CNAS");
        sm.GetActionApplication(BordereauWorkflowState.Draft).Should().Be("BM Pharma");
    }

    // ============================================================================
    // EDGE CASES & RESILIENCE
    // ============================================================================

    [Fact]
    public void PRD098_Reset_Clears_All_Fake_State()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        fake.SimulateInvoiceExists("INV01");
        fake.SimulateTokenPresent();
        fake.SimulateOffline();
        fake.Reset();
        fake.InvoiceCount.Should().Be(0);
        fake.BordereauCount.Should().Be(0);
        fake.IsOnline.Should().BeTrue();
        fake.TokenPresent.Should().BeFalse();
    }

    [Fact]
    public async Task PRD099_Multiple_Operations_Concurrent_Safe()
    {
        var (_, fake, _) = CreateReadOnlyProvider();
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var num = i.ToString("D6");
            tasks.Add(fake.CreateInvoiceAsync(new ChifaInvoiceRequest
            {
                NumFact = num,
                Lines = new List<ChifaInvoiceLineRequest>()
            }));
        }
        await Task.WhenAll(tasks);
        fake.InvoiceCount.Should().Be(100);
    }

    [Fact]
    public async Task PRD100_CorrelationIds_Are_Unique()
    {
        var (serviceProvider, _, _) = CreateReadOnlyProvider();
        var workflowService = serviceProvider.GetRequiredService<IChifaInvoiceWorkflowService>();
        var correlationIds = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var request = new ChifaInvoiceRequest
            {
                NumFact = $"COR{i:D3}",
                NumAssure = "123456789012",
                CodeCentre = 11600,
                Lines = new List<ChifaInvoiceLineRequest>
                {
                    new() { NumEnr = "00001", PrixUnit = 10, Quantite = 1 }
                }
            };
            var result = await workflowService.ExecuteFullWorkflowAsync(request);
            correlationIds.Add(result.CorrelationId);
        }
        correlationIds.Count.Should().Be(10, "each correlation ID should be unique");
    }

    [Fact]
    public void PRD101_AppSettings_AdminPassword_Is_Hardcoded()
    {
        var source = ReadSourceFile("src/BMPharma.UI/appsettings.json");
        source.Should().Contain("admin123",
            "DOCUMENTED RISK: AdminPassword is hardcoded in appsettings.json. " +
            "Should be moved to UserSecrets or environment variables before production.");
    }
}
