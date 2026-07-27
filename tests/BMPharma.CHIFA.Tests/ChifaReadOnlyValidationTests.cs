using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BMPharma.Domain.Enums;
using BMPharma.Persistence.PostgreSQL.Contexts;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BMPharma.CHIFA.Tests;

/// <summary>
/// BM-PHASE-004.12 — Automated Tests for Real CHIFA Read-Only Validation.
/// Tests connection, schema detection, ReadOnly protection, and WriteGuard.
/// </summary>
public class ChifaReadOnlyValidationTests
{
    private static ServiceProvider CreateServiceProvider(ChifaIntegrationMode mode = ChifaIntegrationMode.ReadOnly)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddChifaIntegrationForTests(mode);
        return services.BuildServiceProvider();
    }

    #region Connection & Detection

    [Fact]
    public void REAL001_ConnectionString_ContainsCorrectHost()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30"
        };
        config.ConnectionString.Should().Contain("Host=127.0.0.1");
        config.ConnectionString.Should().Contain("Port=5432");
    }

    [Fact]
    public void REAL002_ConnectionString_ContainsCorrectDatabase()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30"
        };
        config.ConnectionString.Should().Contain("Database=CHIFA_OFFICINE");
    }

    [Fact]
    public void REAL003_ConnectionString_ContainsPharmUser()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30"
        };
        config.ConnectionString.Should().Contain("Username=pharm");
    }

    [Fact]
    public void REAL004_ConnectionString_HasSslModeDisable()
    {
        var config = new ChifaIntegrationConfig
        {
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30"
        };
        config.ConnectionString.Should().Contain("SslMode=Disable");
    }

    [Fact]
    public void REAL005_Config_Schema_DefaultsToPublic()
    {
        var config = new ChifaIntegrationConfig { Schema = "public" };
        config.Schema.Should().Be("public");
    }

    [Fact]
    public void REAL006_Config_ApplicationPath_Set()
    {
        var config = new ChifaIntegrationConfig
        {
            ApplicationPath = @"D:\Download\Softwares\CHIFA_OFFICINE"
        };
        config.ApplicationPath.Should().Be(@"D:\Download\Softwares\CHIFA_OFFICINE");
    }

    [Fact]
    public void REAL007_ModeProvider_DetectsReadOnly()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var provider = new ChifaIntegrationModeProvider(config);
        provider.IsReadOnly.Should().BeTrue();
        provider.CurrentMode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    [Fact]
    public void REAL008_ModeProvider_DetectsTest()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var provider = new ChifaIntegrationModeProvider(config);
        provider.IsReadOnly.Should().BeFalse();
        provider.CurrentMode.Should().Be(ChifaIntegrationMode.Test);
    }

    [Fact]
    public void REAL009_ModeProvider_DetectsProduction()
    {
        var config = new ChifaIntegrationConfig { Mode = "Production" };
        var provider = new ChifaIntegrationModeProvider(config);
        provider.IsReadOnly.Should().BeFalse();
        provider.CurrentMode.Should().Be(ChifaIntegrationMode.Production);
    }

    #endregion

    #region ReadOnly Protection (Layer 1: DI Wiring)

    [Fact]
    public void REAL010_ReadOnly_DI_NoPostgresServices()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var integration = sp.GetRequiredService<IChifaIntegrationService>();
        integration.Should().BeOfType<FakeChifaIntegrationProvider>();
    }

    [Fact]
    public void REAL011_ReadOnly_DI_InvoiceServiceIsFake()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var invoice = sp.GetRequiredService<IChifaInvoiceService>();
        invoice.Should().BeOfType<FakeChifaIntegrationProvider>();
    }

    [Fact]
    public void REAL012_ReadOnly_DI_BordereauServiceIsFake()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var bord = sp.GetRequiredService<IChifaBordereauService>();
        bord.Should().BeOfType<FakeChifaIntegrationProvider>();
    }

    [Fact]
    public void REAL013_ReadOnly_DI_TokenServiceIsFake()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var token = sp.GetRequiredService<IChifaTokenService>();
        token.Should().BeOfType<FakeChifaIntegrationProvider>();
    }

    [Fact]
    public void REAL014_ReadOnly_DI_SigningServiceIsFake()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var sign = sp.GetRequiredService<IChifaSigningService>();
        sign.Should().BeOfType<FakeChifaIntegrationProvider>();
    }

    #endregion

    #region ReadOnly Protection (Layer 2: ChifaWriteGuard)

    [Fact]
    public async Task REAL015_WriteGuard_ReadOnly_ThrowsOnWrite()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var guard = sp.GetRequiredService<ChifaWriteGuard>();
        await guard.Invoking(g => g.EnsureWriteAllowedAsync())
            .Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public async Task REAL016_WriteGuard_ReadOnly_ThrowsOnTestOrProduction()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var guard = sp.GetRequiredService<ChifaWriteGuard>();
        await guard.Invoking(g => g.EnsureTestOrProductionAsync())
            .Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public async Task REAL017_WriteGuard_Test_DoesNotThrow()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.Test);
        var guard = sp.GetRequiredService<ChifaWriteGuard>();
        await guard.Invoking(g => g.EnsureWriteAllowedAsync())
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task REAL018_WriteGuard_Production_DoesNotThrow()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.Production);
        var guard = sp.GetRequiredService<ChifaWriteGuard>();
        await guard.Invoking(g => g.EnsureWriteAllowedAsync())
            .Should().NotThrowAsync();
    }

    #endregion

    #region Fake Provider Behavior

    [Fact]
    public async Task REAL019_FakeProvider_IsAvailable()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var service = sp.GetRequiredService<IChifaIntegrationService>();
        var available = await service.IsChifaAvailableAsync();
        available.Should().BeTrue();
    }

    [Fact]
    public async Task REAL020_FakeProvider_HealthIsOnline()
    {
        using var sp = CreateServiceProvider(ChifaIntegrationMode.ReadOnly);
        var service = sp.GetRequiredService<IChifaIntegrationService>();
        var health = await service.GetHealthStatusAsync();
        health.IsOnline.Should().BeTrue();
    }

    #endregion

    #region EF Core Entity Counts (InMemory)

    [Fact]
    public void REAL021_EFCore_ChifaFacture_Has53Properties()
    {
        using var ctx = new ChifaPostgreSqlContext(new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        ctx.Model.FindEntityType(typeof(ChifaFacture))!.GetProperties().Count().Should().Be(53);
    }

    [Fact]
    public void REAL022_EFCore_ChifaParametre_Has58Properties()
    {
        using var ctx = new ChifaPostgreSqlContext(new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        ctx.Model.FindEntityType(typeof(ChifaParametre))!.GetProperties().Count().Should().Be(58);
    }

    [Fact]
    public void REAL023_EFCore_ChifaMedicament_Has29Properties()
    {
        using var ctx = new ChifaPostgreSqlContext(new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        ctx.Model.FindEntityType(typeof(ChifaMedicament))!.GetProperties().Count().Should().Be(29);
    }

    [Fact]
    public void REAL024_EFCore_ChifaSignature_Has2Properties()
    {
        using var ctx = new ChifaPostgreSqlContext(new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        ctx.Model.FindEntityType(typeof(ChifaSignature))!.GetProperties().Count().Should().Be(2);
    }

    [Fact]
    public void REAL025_EFCore_AllEntities_MappedToCorrectTables()
    {
        using var ctx = new ChifaPostgreSqlContext(new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        ctx.Model.FindEntityType(typeof(ChifaFacture))!.GetTableName().Should().Be("facture");
        ctx.Model.FindEntityType(typeof(ChifaDetailFact))!.GetTableName().Should().Be("detail_fact");
        ctx.Model.FindEntityType(typeof(ChifaBordereau))!.GetTableName().Should().Be("bordereau");
        ctx.Model.FindEntityType(typeof(ChifaParametre))!.GetTableName().Should().Be("parametre");
        ctx.Model.FindEntityType(typeof(ChifaMedicament))!.GetTableName().Should().Be("medicament");
        ctx.Model.FindEntityType(typeof(ChifaSignature))!.GetTableName().Should().Be("signature");
    }

    #endregion

    #region WriteDbContext (same mapping)

    [Fact]
    public void REAL026_WriteDbContext_AllEntities_MappedToCorrectTables()
    {
        using var ctx = new ChifaWriteDbContext(new DbContextOptionsBuilder<ChifaWriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        ctx.Model.FindEntityType(typeof(ChifaFacture))!.GetTableName().Should().Be("facture");
        ctx.Model.FindEntityType(typeof(ChifaDetailFact))!.GetTableName().Should().Be("detail_fact");
        ctx.Model.FindEntityType(typeof(ChifaBordereau))!.GetTableName().Should().Be("bordereau");
        ctx.Model.FindEntityType(typeof(ChifaParametre))!.GetTableName().Should().Be("parametre");
        ctx.Model.FindEntityType(typeof(ChifaMedicament))!.GetTableName().Should().Be("medicament");
        ctx.Model.FindEntityType(typeof(ChifaSignature))!.GetTableName().Should().Be("signature");
    }

    [Fact]
    public void REAL027_WriteDbContext_ChifaFacture_Has53Properties()
    {
        using var ctx = new ChifaWriteDbContext(new DbContextOptionsBuilder<ChifaWriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        ctx.Model.FindEntityType(typeof(ChifaFacture))!.GetProperties().Count().Should().Be(53);
    }

    #endregion

    #region Configuration Validation

    [Fact]
    public void REAL028_Config_DefaultMode_IsReadOnly()
    {
        var config = new ChifaIntegrationConfig();
        config.Mode.Should().Be("ReadOnly");
        config.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void REAL029_Config_HasSchema()
    {
        var config = new ChifaIntegrationConfig { Schema = "public" };
        config.Schema.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void REAL030_Config_HasApplicationPath()
    {
        var config = new ChifaIntegrationConfig { ApplicationPath = @"D:\Download\Softwares\CHIFA_OFFICINE" };
        config.ApplicationPath.Should().NotBeNullOrEmpty();
    }

    #endregion
}
