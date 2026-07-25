using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaDependencyInjectionTests
{
    private static ServiceCollection CreateServicesWithLogging()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public void DI001_ReadOnly_Mode_Registers_Stubs()
    {
        var services = CreateServicesWithLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CHIFA:Mode"] = "ReadOnly",
                ["CHIFA:ConnectionString"] = "Host=localhost"
            })
            .Build();

        services.AddChifaIntegration(config);
        var provider = services.BuildServiceProvider();

        var invoiceService = provider.GetRequiredService<IChifaInvoiceService>();
        invoiceService.Should().BeOfType<ChifaInvoiceServiceStub>();

        var bordereauService = provider.GetRequiredService<IChifaBordereauService>();
        bordereauService.Should().BeOfType<ChifaBordereauServiceStub>();

        var signingService = provider.GetRequiredService<IChifaSigningService>();
        signingService.Should().BeOfType<ChifaSigningServiceStub>();

        var tokenService = provider.GetRequiredService<IChifaTokenService>();
        tokenService.Should().BeOfType<ChifaTokenServiceStub>();
    }

    [Fact]
    public void DI002_Test_Mode_Registers_Postgres_Services()
    {
        var services = CreateServicesWithLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CHIFA:Mode"] = "Test",
                ["CHIFA:ConnectionString"] = "Host=localhost;Database=CHIFA_OFFICINE;Username=pharm"
            })
            .Build();

        services.AddChifaIntegration(config);
        var provider = services.BuildServiceProvider();

        var invoiceService = provider.GetRequiredService<IChifaInvoiceService>();
        invoiceService.Should().BeOfType<ChifaPostgresInvoiceService>();

        var bordereauService = provider.GetRequiredService<IChifaBordereauService>();
        bordereauService.Should().BeOfType<ChifaPostgresBordereauService>();
    }

    [Fact]
    public void DI003_Production_Mode_Registers_Postgres_Services()
    {
        var services = CreateServicesWithLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CHIFA:Mode"] = "Production",
                ["CHIFA:ConnectionString"] = "Host=localhost;Database=CHIFA_OFFICINE;Username=pharm"
            })
            .Build();

        services.AddChifaIntegration(config);
        var provider = services.BuildServiceProvider();

        var invoiceService = provider.GetRequiredService<IChifaInvoiceService>();
        invoiceService.Should().BeOfType<ChifaPostgresInvoiceService>();
    }

    [Fact]
    public void DI004_ReadOnly_Mode_Provider_Returns_ReadOnly()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var provider = new ChifaIntegrationModeProvider(config);

        provider.CurrentMode.Should().Be(ChifaIntegrationMode.ReadOnly);
        provider.IsReadOnly.Should().BeTrue();
        provider.IsTest.Should().BeFalse();
        provider.IsProduction.Should().BeFalse();
    }

    [Fact]
    public void DI005_Test_Mode_Provider_Returns_Test()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var provider = new ChifaIntegrationModeProvider(config);

        provider.CurrentMode.Should().Be(ChifaIntegrationMode.Test);
        provider.IsTest.Should().BeTrue();
        provider.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void DI006_Production_Mode_Provider_Returns_Production()
    {
        var config = new ChifaIntegrationConfig { Mode = "Production" };
        var provider = new ChifaIntegrationModeProvider(config);

        provider.CurrentMode.Should().Be(ChifaIntegrationMode.Production);
        provider.IsProduction.Should().BeTrue();
    }

    [Fact]
    public void DI007_Default_Mode_Is_ReadOnly()
    {
        var config = new ChifaIntegrationConfig { Mode = "" };
        var provider = new ChifaIntegrationModeProvider(config);

        provider.CurrentMode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    [Fact]
    public void DI008_WriteGuard_Blocks_In_ReadOnly_Mode()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var provider = new ChifaIntegrationModeProvider(config);

        var guard = new ChifaWriteGuard(() => provider.GetModeAsync());
        var act = () => guard.EnsureWriteAllowedAsync();

        act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public void DI009_WriteGuard_Allows_In_Test_Mode()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var provider = new ChifaIntegrationModeProvider(config);

        var guard = new ChifaWriteGuard(() => provider.GetModeAsync());
        var act = async () => await guard.EnsureWriteAllowedAsync();

        act.Should().NotThrowAsync();
    }

    [Fact]
    public void DI010_WriteGuard_Allows_In_Production_Mode()
    {
        var config = new ChifaIntegrationConfig { Mode = "Production" };
        var provider = new ChifaIntegrationModeProvider(config);

        var guard = new ChifaWriteGuard(() => provider.GetModeAsync());
        var act = async () => await guard.EnsureWriteAllowedAsync();

        act.Should().NotThrowAsync();
    }

    [Fact]
    public void DI011_All_Modes_Register_AuditService()
    {
        foreach (var mode in new[] { "ReadOnly", "Test", "Production" })
        {
            var services = CreateServicesWithLogging();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CHIFA:Mode"] = mode,
                    ["CHIFA:ConnectionString"] = "Host=localhost;Database=CHIFA_OFFICINE;Username=pharm"
                })
                .Build();

            services.AddChifaIntegration(config);
            var provider = services.BuildServiceProvider();

            var audit = provider.GetRequiredService<IChifaAuditService>();
            audit.Should().NotBeNull();
        }
    }

    [Fact]
    public void DI012_All_Modes_Register_Validators()
    {
        var services = CreateServicesWithLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CHIFA:Mode"] = "ReadOnly",
                ["CHIFA:ConnectionString"] = "Host=localhost"
            })
            .Build();

        services.AddChifaIntegration(config);
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<ChifaInvoiceValidator>().Should().NotBeNull();
        provider.GetRequiredService<ChifaBordereauValidator>().Should().NotBeNull();
        provider.GetRequiredService<ChifaWriteGuard>().Should().NotBeNull();
        provider.GetRequiredService<ChifaInvoiceMapper>().Should().NotBeNull();
        provider.GetRequiredService<ChifaBordereauMapper>().Should().NotBeNull();
        provider.GetRequiredService<ChifaWorkflowStateMachine>().Should().NotBeNull();
    }

    [Fact]
    public void DI013_ForTests_ReadOnly_Registers_Stubs()
    {
        var services = CreateServicesWithLogging();
        services.AddChifaIntegrationForTests(ChifaIntegrationMode.ReadOnly);
        var provider = services.BuildServiceProvider();

        var invoiceService = provider.GetRequiredService<IChifaInvoiceService>();
        invoiceService.Should().BeOfType<ChifaInvoiceServiceStub>();
    }

    [Fact]
    public void DI014_ForTests_Test_Registers_Postgres_Services()
    {
        var services = CreateServicesWithLogging();
        services.AddChifaIntegrationForTests(ChifaIntegrationMode.Test);
        var provider = services.BuildServiceProvider();

        var invoiceService = provider.GetRequiredService<IChifaInvoiceService>();
        invoiceService.Should().BeOfType<ChifaPostgresInvoiceService>();
    }
}
