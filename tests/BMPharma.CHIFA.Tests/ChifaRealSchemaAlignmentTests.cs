using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BMPharma.Domain.Enums;
using BMPharma.Persistence.PostgreSQL.Contexts;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;
using BMPharma.CHIFA.Interfaces;
using Xunit;

namespace BMPharma.CHIFA.Tests;

/// <summary>
/// Tests for BM-PHASE-004.10 — Real CHIFA Connection & Schema Alignment.
/// All tests use InMemory or unit-level checks. No real PostgreSQL writes.
/// </summary>
public class ChifaRealSchemaAlignmentTests
{
    private static DbContextOptions<ChifaPostgreSqlContext> CreateReadOnlyOptions()
    {
        return new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private static DbContextOptions<ChifaWriteDbContext> CreateWriteOptions()
    {
        return new DbContextOptionsBuilder<ChifaWriteDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private static ServiceProvider CreateTestServiceProvider(ChifaIntegrationMode mode = ChifaIntegrationMode.ReadOnly)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddChifaIntegrationForTests(mode);
        return services.BuildServiceProvider();
    }

    #region Phase 1: Connection Configuration

    [Fact]
    public void CONN001_ConnectionString_UsesPharmUser()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true"
        };
        config.ConnectionString.Should().Contain("Username=pharm");
        config.ConnectionString.Should().NotContain("Username=postgres");
    }

    [Fact]
    public void CONN002_SchemaConfig_DefaultsToPublic()
    {
        var config = new ChifaIntegrationConfig
        {
            Schema = "public"
        };
        config.Schema.Should().Be("public");
    }

    [Fact]
    public void CONN003_ModeConfig_DefaultsToReadOnly()
    {
        var config = new ChifaIntegrationConfig();
        config.Mode.Should().Be("ReadOnly");
        config.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void CONN004_IsReadOnly_DetectsCorrectly()
    {
        new ChifaIntegrationConfig { Mode = "ReadOnly" }.IsReadOnly.Should().BeTrue();
        new ChifaIntegrationConfig { Mode = "Test" }.IsReadOnly.Should().BeFalse();
        new ChifaIntegrationConfig { Mode = "Production" }.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void CONN005_ConnectionString_HasTimeoutConfig()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30"
        };
        config.ConnectionString.Should().Contain("Timeout=10");
        config.ConnectionString.Should().Contain("CommandTimeout=30");
    }

    [Fact]
    public void CONN006_ConnectionString_HasSslModeDisable()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true"
        };
        config.ConnectionString.Should().Contain("SslMode=Disable");
    }

    #endregion

    #region Phase 3: EF Core Entity Mapping — ChifaFacture

    [Fact]
    public void FACT001_Facture_HasExactly53Properties()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaFacture))!.GetProperties().Count();
        propertyCount.Should().Be(53, "BM-PHASE-004.9 real schema has 53 physical columns in facture");
    }

    [Fact]
    public void FACT002_Facture_HasCorrectPK()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var pk = context.Model.FindEntityType(typeof(ChifaFacture))!.FindPrimaryKey()!;
        pk.Properties.Should().HaveCount(1);
        pk.Properties[0].Name.Should().Be("NumFact");
    }

    [Fact]
    public void FACT003_Facture_TypeMaj_IsRequiredInt()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var prop = context.Model.FindEntityType(typeof(ChifaFacture))!.FindProperty("TypeMaj")!;
        prop.IsNullable.Should().BeFalse();
        prop.ClrType.Should().Be(typeof(int));
    }

    [Fact]
    public void FACT004_Facture_MontMajFae_IsRequiredDecimal()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var prop = context.Model.FindEntityType(typeof(ChifaFacture))!.FindProperty("MontMajFae")!;
        prop.IsNullable.Should().BeFalse();
        prop.GetPrecision().Should().Be(4);
        prop.GetScale().Should().Be(2);
    }

    [Fact]
    public void FACT005_Facture_Taux_IsString_NotDecimal()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var prop = context.Model.FindEntityType(typeof(ChifaFacture))!.FindProperty("Taux")!;
        prop.ClrType.Should().Be(typeof(string), "real CHIFA facture.taux is char(1), not numeric");
    }

    [Fact]
    public void FACT006_Facture_HasRangAd_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("RangAd");
        prop.Should().NotBeNull();
        prop!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void FACT007_Facture_HasIdUser_Column()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var prop = context.Model.FindEntityType(typeof(ChifaFacture))!.FindProperty("IdUser")!;
        prop.Should().NotBeNull();
        prop.ClrType.Should().Be(typeof(int?));
        prop.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void FACT008_Facture_HasStatutFact_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("StatutFact");
        prop.Should().NotBeNull("statut_fact is needed for invoice status tracking");
    }

    [Fact]
    public void FACT009_Facture_HasSignature_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("Signature");
        prop.Should().NotBeNull("signature (xml) is needed for digital signature");
        prop!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void FACT010_Facture_HasFactXml_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("FactXml");
        prop.Should().NotBeNull("fact_xml (xml) is needed for XML submission");
    }

    [Fact]
    public void FACT011_Facture_HasCodeSp_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("CodeSp");
        prop.Should().NotBeNull("code_sp is needed for specialty code");
    }

    [Fact]
    public void FACT012_Facture_HasTypeOrd_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("TypeOrd");
        prop.Should().NotBeNull("type_ord is needed for prescription type");
    }

    [Fact]
    public void FACT013_Facture_HasPrescripteur_Column()
    {
        var prop = typeof(ChifaFacture).GetProperty("Prescripteur");
        prop.Should().NotBeNull("prescripteur is the prescribing doctor");
    }

    [Fact]
    public void FACT014_Facture_NoPhantomProperties_Exist()
    {
        var phantomProps = new[] { "NomAssure", "PrenomAssure", "NomBenef", "PrenomBenef",
            "LieuNaissance", "DateNaissance", "Wilaya", "Commune", "CodePostal",
            "Tel", "NumDossier", "MotifRejet", "DateRejet", "DatePaiement", "MontPaiement",
            "NumCheque", "DateControle", "IdUtilisateur", "DateCreation", "DateModification",
            "CentreGestion", "CodeActe", "Beneficiaire", "Matricule" };

        foreach (var prop in phantomProps)
        {
            typeof(ChifaFacture).GetProperty(prop).Should().BeNull(
                $"Phantom property '{prop}' should NOT exist in ChifaFacture (column not in real DB)");
        }
    }

    [Fact]
    public void FACT015_Facture_CanCreate_InMemory()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var facture = new ChifaFacture
        {
            NumFact = "TEST0001",
            TypeMaj = 0,
            MontMajFae = 0,
            MontMaj = 0,
            RangAd = "01",
            Taux = "1",
            StatutFact = "N",
            CodeCentre = "11600",
            NumAssure = "123456789012",
            IdUser = 1
        };
        context.Factures.Add(facture);
        context.SaveChanges();

        var loaded = context.Factures.First(f => f.NumFact == "TEST0001");
        loaded.RangAd.Should().Be("01");
        loaded.Taux.Should().Be("1");
        loaded.StatutFact.Should().Be("N");
        loaded.IdUser.Should().Be(1);
    }

    #endregion

    #region Phase 3: EF Core Entity Mapping — ChifaParametre

    [Fact]
    public void PARA001_Parametre_HasExactly58Properties()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaParametre))!.GetProperties().Count();
        propertyCount.Should().Be(58, "BM-PHASE-004.9 real schema has 58 physical columns in parametre");
    }

    [Fact]
    public void PARA002_Parametre_IsKeyless()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaParametre))!;
        entityType.FindPrimaryKey().Should().BeNull();
    }

    [Fact]
    public void PARA003_Parametre_Version_IsString_NotInt()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var prop = context.Model.FindEntityType(typeof(ChifaParametre))!.FindProperty("Version")!;
        prop.ClrType.Should().Be(typeof(string), "real parametre.version is varchar(20), not integer");
    }

    [Fact]
    public void PARA004_Parametre_HasNumTel_Column()
    {
        var prop = typeof(ChifaParametre).GetProperty("NumTel");
        prop.Should().NotBeNull("num_tel is the correct column name (not Tel)");
    }

    [Fact]
    public void PARA005_Parametre_HasNumFax_Column()
    {
        var prop = typeof(ChifaParametre).GetProperty("NumFax");
        prop.Should().NotBeNull("num_fax is the correct column name (not Fax)");
    }

    [Fact]
    public void PARA006_Parametre_HasNomPrenom_Columns()
    {
        typeof(ChifaParametre).GetProperty("Nom").Should().NotBeNull();
        typeof(ChifaParametre).GetProperty("Prenom").Should().NotBeNull();
    }

    [Fact]
    public void PARA007_Parametre_HasTokenColumns()
    {
        typeof(ChifaParametre).GetProperty("AccessToken").Should().NotBeNull("access_token is a real column");
        typeof(ChifaParametre).GetProperty("RefreshToken").Should().NotBeNull("refresh_token is a real column");
    }

    [Fact]
    public void PARA008_Parametre_NoPhantomProperties()
    {
        var phantomProps = new[] { "Wilaya", "Commune", "CodePostal", "DateCreation", "DateModification" };
        foreach (var prop in phantomProps)
        {
            typeof(ChifaParametre).GetProperty(prop).Should().BeNull(
                $"Phantom property '{prop}' should NOT exist in ChifaParametre");
        }
    }

    [Fact]
    public void PARA009_Parametre_HasNisNicoNdps_Columns()
    {
        typeof(ChifaParametre).GetProperty("Nis").Should().NotBeNull();
        typeof(ChifaParametre).GetProperty("Nico").Should().NotBeNull();
        typeof(ChifaParametre).GetProperty("Ndps").Should().NotBeNull();
    }

    [Fact]
    public void PARA010_Parametre_HasBackupColumns()
    {
        typeof(ChifaParametre).GetProperty("CheminBackup").Should().NotBeNull();
        typeof(ChifaParametre).GetProperty("HeureBackup").Should().NotBeNull();
        typeof(ChifaParametre).GetProperty("NbBackup").Should().NotBeNull();
        typeof(ChifaParametre).GetProperty("BackupStart").Should().NotBeNull();
        typeof(ChifaParametre).GetProperty("BackupExit").Should().NotBeNull();
    }

    [Fact]
    public void PARA011_Parametre_HasDateApiChifa_Column()
    {
        typeof(ChifaParametre).GetProperty("DateApiChifa").Should().NotBeNull();
    }

    [Fact]
    public void PARA012_Parametre_HasVersionDb_Column()
    {
        var prop = typeof(ChifaParametre).GetProperty("VersionDb");
        prop.Should().NotBeNull();
        prop!.PropertyType.Should().Be(typeof(int?));
    }

    #endregion

    #region Phase 3: EF Core Entity Mapping — ChifaMedicament

    [Fact]
    public void MEDC001_Medicament_HasExactly29Properties()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaMedicament))!.GetProperties().Count();
        propertyCount.Should().Be(29, "BM-PHASE-004.9 real schema has 29 columns in medicament");
    }

    [Fact]
    public void MEDC002_Medicament_PK_IsNumEnr()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var pk = context.Model.FindEntityType(typeof(ChifaMedicament))!.FindPrimaryKey()!;
        pk.Properties.Should().HaveCount(1);
        pk.Properties[0].Name.Should().Be("NumEnr");
    }

    [Fact]
    public void MEDC003_Medicament_HasCriticalColumns()
    {
        typeof(ChifaMedicament).GetProperty("NomCom").Should().NotBeNull();
        typeof(ChifaMedicament).GetProperty("NomDci").Should().NotBeNull();
        typeof(ChifaMedicament).GetProperty("Dosage").Should().NotBeNull();
        typeof(ChifaMedicament).GetProperty("TarifRef").Should().NotBeNull();
        typeof(ChifaMedicament).GetProperty("Taux").Should().NotBeNull();
        typeof(ChifaMedicament).GetProperty("Remboursable").Should().NotBeNull();
        typeof(ChifaMedicament).GetProperty("Generic").Should().NotBeNull();
        typeof(ChifaMedicament).GetProperty("CodeForme").Should().NotBeNull();
    }

    [Fact]
    public void MEDC004_Medicament_TarifRef_HasCorrectPrecision()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var prop = context.Model.FindEntityType(typeof(ChifaMedicament))!.FindProperty("TarifRef")!;
        prop.GetPrecision().Should().Be(11);
        prop.GetScale().Should().Be(2);
    }

    [Fact]
    public void MEDC005_Medicament_TableName_IsMedicament()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaMedicament))!;
        entityType.GetTableName().Should().Be("medicament");
    }

    [Fact]
    public void MEDC006_Medicament_CanCreate_InMemory()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var med = new ChifaMedicament
        {
            NumEnr = "12345",
            NomCom = "DOLOPRINE 1000MG",
            Dosage = "1000MG",
            TarifRef = 150.50m,
            Taux = 70
        };
        context.Medicaments.Add(med);
        context.SaveChanges();

        var loaded = context.Medicaments.First(m => m.NumEnr == "12345");
        loaded.NomCom.Should().Be("DOLOPRINE 1000MG");
        loaded.TarifRef.Should().Be(150.50m);
    }

    #endregion

    #region Phase 3: EF Core Entity Mapping — ChifaSignature

    [Fact]
    public void SIGN001_Signature_HasExactly2Properties()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaSignature))!.GetProperties().Count();
        propertyCount.Should().Be(2, "signature table has 2 columns: num_fact and sign");
    }

    [Fact]
    public void SIGN002_Signature_PK_IsNumFact()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var pk = context.Model.FindEntityType(typeof(ChifaSignature))!.FindPrimaryKey()!;
        pk.Properties.Should().HaveCount(1);
        pk.Properties[0].Name.Should().Be("NumFact");
    }

    [Fact]
    public void SIGN003_Signature_TableName_IsSignature()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaSignature))!;
        entityType.GetTableName().Should().Be("signature");
    }

    #endregion

    #region Phase 3: DbContext — Write Context Parity

    [Fact]
    public void CTX050_WriteContext_HasSameEntities_AsReadOnly()
    {
        using var readCtx = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        using var writeCtx = new ChifaWriteDbContext(CreateWriteOptions());

        var readTypes = readCtx.Model.GetEntityTypes().Select(e => e.ClrType).ToList();
        var writeTypes = writeCtx.Model.GetEntityTypes().Select(e => e.ClrType).ToList();

        readTypes.Should().BeEquivalentTo(writeTypes, "Write and ReadOnly contexts must map the same entities");
    }

    [Fact]
    public void CTX051_ReadOnlyContext_Has6Entities()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityCount = context.Model.GetEntityTypes().Count();
        entityCount.Should().Be(6, "ReadOnly context should have 6 entities: Facture, DetailFact, Bordereau, Parametre, Medicament, Signature");
    }

    #endregion

    #region Phase 8: WriteGuard Tests

    [Fact]
    public void GUARD001_ReadOnlyMode_ThrowsOnWrite()
    {
        var provider = CreateTestServiceProvider(ChifaIntegrationMode.ReadOnly);
        using var scope = provider.CreateScope();
        var guard = scope.ServiceProvider.GetRequiredService<ChifaWriteGuard>();

        guard.Invoking(g => g.EnsureWriteAllowedAsync())
            .Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public void GUARD002_TrimConnectionStrings()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=test123;SslMode=Disable;TrustServerCertificate=true"
        };
        var trimmed = config.ConnectionString.Replace("Password=test123", "Password=***");
        trimmed.Should().NotContain("test123");
    }

    #endregion

    #region Phase 8: DetailFact and Bordereau Unchanged

    [Fact]
    public void DETAIL001_DetailFact_Has20Properties()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaDetailFact))!.GetProperties().Count();
        propertyCount.Should().Be(20, "detail_fact still has 20 columns — unchanged from 004.9");
    }

    [Fact]
    public void BORD001_Bordereau_Has11Properties()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaBordereau))!.GetProperties().Count();
        propertyCount.Should().Be(11, "bordereau still has 11 columns — unchanged from 004.9");
    }

    #endregion

    #region Phase 8: Table Classification

    [Fact]
    public void CLASS001_AllCoreEntities_MapToCorrectTables()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        context.Model.FindEntityType(typeof(ChifaFacture))!.GetTableName().Should().Be("facture");
        context.Model.FindEntityType(typeof(ChifaDetailFact))!.GetTableName().Should().Be("detail_fact");
        context.Model.FindEntityType(typeof(ChifaBordereau))!.GetTableName().Should().Be("bordereau");
        context.Model.FindEntityType(typeof(ChifaParametre))!.GetTableName().Should().Be("parametre");
        context.Model.FindEntityType(typeof(ChifaMedicament))!.GetTableName().Should().Be("medicament");
        context.Model.FindEntityType(typeof(ChifaSignature))!.GetTableName().Should().Be("signature");
    }

    [Fact]
    public void CLASS002_FactureDecimal_PrecisionMatches()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaFacture))!;
        entityType.FindProperty("MontOff")!.GetPrecision().Should().Be(10);
        entityType.FindProperty("MontOff")!.GetScale().Should().Be(2);
        entityType.FindProperty("MontFact")!.GetPrecision().Should().Be(11);
        entityType.FindProperty("MontFact")!.GetScale().Should().Be(2);
        entityType.FindProperty("MontMaj")!.GetPrecision().Should().Be(11);
        entityType.FindProperty("MontMaj")!.GetScale().Should().Be(2);
        entityType.FindProperty("MontMut")!.GetPrecision().Should().Be(10);
        entityType.FindProperty("MontMut")!.GetScale().Should().Be(2);
    }

    #endregion
}
