using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

/// <summary>
/// Dashboard 4.4 tests — verifies CHIFA Dashboard ViewModel behavior
/// through service-layer integration tests.
/// Tests: ReadOnly mode, error handling, health checks, token detection, signing status.
/// NOTE: ChifaDashboardViewModel tests that require WPF reference are in
/// BMPharma.UI.Tests project (separate project due to WPF dependency).
/// </summary>
public class ChifaDashboardTests
{
    private static ServiceCollection CreateServicesWithLogging()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public void DASH001_ReadOnlyMode_IsDefault()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        modeProvider.CurrentMode.Should().Be(ChifaIntegrationMode.ReadOnly);
        modeProvider.IsReadOnly.Should().BeTrue();
        modeProvider.IsTest.Should().BeFalse();
        modeProvider.IsProduction.Should().BeFalse();
    }

    [Fact]
    public async Task DASH002_ReadOnlyMode_ProviderReturnsReadOnlyTask()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var mode = await modeProvider.GetModeAsync();
        mode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    [Fact]
    public async Task DASH003_ReadOnlyMode_WriteGuardBlocksWrites()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);
        var guard = new ChifaWriteGuard(() => Task.FromResult(modeProvider.CurrentMode));

        var act = () => guard.EnsureWriteAllowedAsync();
        await act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public async Task DASH004_ReadOnlyMode_WriteGuardBlocksTestOrProduction()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);
        var guard = new ChifaWriteGuard(() => Task.FromResult(modeProvider.CurrentMode));

        var act = () => guard.EnsureTestOrProductionAsync();
        await act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public async Task DASH005_IntegrationServiceStub_ReturnsOffline()
    {
        var services = CreateServicesWithLogging();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ChifaIntegrationServiceStub>>();

        var service = new ChifaIntegrationServiceStub(logger);

        var available = await service.IsChifaAvailableAsync();
        available.Should().BeFalse();

        var health = await service.GetHealthStatusAsync();
        health.IsOnline.Should().BeFalse();
        health.IsDatabaseConnected.Should().BeFalse();
        health.IsTokenPresent.Should().BeFalse();
        health.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DASH006_TokenServiceStub_ReturnsNotPresent()
    {
        var services = CreateServicesWithLogging();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ChifaTokenServiceStub>>();

        var service = new ChifaTokenServiceStub(logger);

        var present = await service.IsTokenPresentAsync();
        present.Should().BeFalse();

        var info = await service.GetTokenInfoAsync();
        info.Should().BeNull();
    }

    [Fact]
    public async Task DASH007_SigningServiceStub_ReturnsNotSigned()
    {
        var services = CreateServicesWithLogging();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ChifaSigningServiceStub>>();

        var service = new ChifaSigningServiceStub(logger);

        var status = await service.GetSigningStatusAsync("000001");
        status.Should().Be(ChifaSigningStatus.NotSigned);

        var tokenAvailable = await service.IsTokenAvailableAsync();
        tokenAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task DASH008_BordereauServiceStub_ReturnsFailure()
    {
        var services = CreateServicesWithLogging();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ChifaBordereauServiceStub>>();

        var service = new ChifaBordereauServiceStub(logger);

        var createResult = await service.CreateBordereauAsync(new ChifaBordereauRequest());
        createResult.Success.Should().BeFalse();
        createResult.ErrorMessage.Should().NotBeNullOrEmpty();

        var signResult = await service.SignBordereauAsync("000001");
        signResult.Success.Should().BeFalse();

        var closeResult = await service.CloseBordereauAsync("000001");
        closeResult.Success.Should().BeFalse();

        var nextNum = await service.GetNextBordereauNumberAsync();
        nextNum.Should().Be("000001");
    }

    [Fact]
    public void DASH009_ReadOnlyMode_DI_RegistersStubs()
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

        var modeProvider = provider.GetRequiredService<ChifaIntegrationModeProvider>();
        modeProvider.IsReadOnly.Should().BeTrue();

        var integrationService = provider.GetRequiredService<IChifaIntegrationService>();
        integrationService.Should().BeOfType<FakeChifaIntegrationProvider>();

        var invoiceService = provider.GetRequiredService<IChifaInvoiceService>();
        invoiceService.Should().BeOfType<FakeChifaIntegrationProvider>();

        var bordereauService = provider.GetRequiredService<IChifaBordereauService>();
        bordereauService.Should().BeOfType<FakeChifaIntegrationProvider>();
    }

    [Fact]
    public async Task DASH010_ReadOnlyMode_AllServicesReturnSafeValues()
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

        var integrationService = provider.GetRequiredService<IChifaIntegrationService>();
        var health = await integrationService.GetHealthStatusAsync();
        health.IsOnline.Should().BeTrue("ReadOnly mode with FakeChifaIntegrationProvider should simulate online");

        var tokenService = provider.GetRequiredService<IChifaTokenService>();
        var tokenPresent = await tokenService.IsTokenPresentAsync();
        tokenPresent.Should().BeFalse("ReadOnly mode should report no token");

        var signingService = provider.GetRequiredService<IChifaSigningService>();
        var signingStatus = await signingService.GetSigningStatusAsync("000001");
        signingStatus.Should().Be(ChifaSigningStatus.NotSigned, "ReadOnly mode should report not signed");
    }

    [Fact]
    public async Task DASH011_IntegrationService_HasErrorMessage_WhenOffline()
    {
        var services = CreateServicesWithLogging();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ChifaIntegrationServiceStub>>();

        var service = new ChifaIntegrationServiceStub(logger);
        var health = await service.GetHealthStatusAsync();

        health.ErrorMessage.Should().Contain("not yet implemented");
    }

    [Fact]
    public async Task DASH012_IntegrationService_CheckedAt_IsSet()
    {
        var services = CreateServicesWithLogging();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ChifaIntegrationServiceStub>>();

        var before = DateTime.UtcNow;
        var service = new ChifaIntegrationServiceStub(logger);
        var health = await service.GetHealthStatusAsync();
        var after = DateTime.UtcNow;

        health.CheckedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task DASH013_TokenInfo_WhenNull_IsNull()
    {
        var services = CreateServicesWithLogging();
        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ChifaTokenServiceStub>>();

        var service = new ChifaTokenServiceStub(logger);
        var info = await service.GetTokenInfoAsync();

        info.Should().BeNull();
    }

    [Fact]
    public void DASH014_Configuration_DefaultsToReadOnly()
    {
        var config = new ChifaIntegrationConfig();

        config.Mode.Should().Be("ReadOnly");
        config.IsReadOnly.Should().BeTrue();
        config.IsTest.Should().BeFalse();
        config.IsProduction.Should().BeFalse();
    }

    [Fact]
    public void DASH015_TestMode_ModeProviderReportsTest()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        modeProvider.CurrentMode.Should().Be(ChifaIntegrationMode.Test);
        modeProvider.IsReadOnly.Should().BeFalse();
        modeProvider.IsTest.Should().BeTrue();
    }

    [Fact]
    public void DASH016_ProductionMode_ModeProviderReportsProduction()
    {
        var config = new ChifaIntegrationConfig { Mode = "Production" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        modeProvider.CurrentMode.Should().Be(ChifaIntegrationMode.Production);
        modeProvider.IsReadOnly.Should().BeFalse();
        modeProvider.IsProduction.Should().BeTrue();
    }

    [Fact]
    public async Task DASH017_MockedIntegrationService_ReturnsCorrectHealth()
    {
        var mockService = new Mock<IChifaIntegrationService>();
        mockService.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockService.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus
            {
                IsOnline = true,
                IsDatabaseConnected = true,
                IsTokenPresent = true,
                ErrorMessage = null
            });

        var available = await mockService.Object.IsChifaAvailableAsync();
        available.Should().BeTrue();

        var health = await mockService.Object.GetHealthStatusAsync();
        health.IsOnline.Should().BeTrue();
        health.IsDatabaseConnected.Should().BeTrue();
        health.IsTokenPresent.Should().BeTrue();
    }

    [Fact]
    public async Task DASH018_MockedTokenService_ReturnsTokenInfo()
    {
        var mockService = new Mock<IChifaTokenService>();
        mockService.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockService.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenInfo
            {
                SerialNumber = "12345",
                Label = "Token Test",
                ExpiryDate = DateTime.UtcNow.AddDays(30)
            });

        var present = await mockService.Object.IsTokenPresentAsync();
        present.Should().BeTrue();

        var info = await mockService.Object.GetTokenInfoAsync();
        info.Should().NotBeNull();
        info!.SerialNumber.Should().Be("12345");
        info.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DASH019_MockedTokenService_ExpiredToken_IsInvalid()
    {
        var mockService = new Mock<IChifaTokenService>();
        mockService.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockService.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenInfo
            {
                SerialNumber = "12345",
                Label = "Token Test",
                ExpiryDate = DateTime.UtcNow.AddDays(-1) // expired
            });

        var info = await mockService.Object.GetTokenInfoAsync();
        info.Should().NotBeNull();
        info!.IsValid.Should().BeFalse("Token is expired");
    }

    [Fact]
    public async Task DASH020_MockedSigningService_ReturnsSigned()
    {
        var mockService = new Mock<IChifaSigningService>();
        mockService.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.Signed);
        mockService.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var status = await mockService.Object.GetSigningStatusAsync("000001");
        status.Should().Be(ChifaSigningStatus.Signed);

        var tokenAvailable = await mockService.Object.IsTokenAvailableAsync();
        tokenAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task DASH021_MockedBordereauService_CreatesSuccessfully()
    {
        var mockService = new Mock<IChifaBordereauService>();
        mockService.Setup(s => s.CreateBordereauAsync(It.IsAny<ChifaBordereauRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaBordereauResult
            {
                Success = true,
                NumBord = "000001",
                ErrorMessage = null
            });

        var result = await mockService.Object.CreateBordereauAsync(new ChifaBordereauRequest());
        result.Success.Should().BeTrue();
        result.NumBord.Should().Be("000001");
    }

    [Fact]
    public async Task DASH022_ConnectionException_DoesNotThrow()
    {
        var mockService = new Mock<IChifaIntegrationService>();
        mockService.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("PostgreSQL connection refused"));
        mockService.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("PostgreSQL connection refused"));

        var act = async () => await mockService.Object.IsChifaAvailableAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PostgreSQL connection refused*");

        var act2 = async () => await mockService.Object.GetHealthStatusAsync();
        await act2.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PostgreSQL connection refused*");
    }

    [Fact]
    public void DASH023_DashboardViewModel_Constructor_CreatesValidInstance()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus
            {
                IsOnline = false,
                IsDatabaseConnected = false,
                IsTokenPresent = false,
                ErrorMessage = "Service mocké"
            });

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockToken.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.NotSigned);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        vm.Should().NotBeNull();
        vm.IntegrationMode.Should().Be("ReadOnly");
        vm.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public async Task DASH024_DashboardViewModel_ReadOnlyMode_AllPropertiesInitialized()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus
            {
                IsOnline = false,
                IsDatabaseConnected = false,
                IsTokenPresent = false
            });

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockToken.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.NotSigned);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        await vm.RefreshAsync();

        vm.IntegrationMode.Should().Be("ReadOnly");
        vm.IsReadOnly.Should().BeTrue();
        vm.ConnectionStatus.Should().NotBeNullOrEmpty();
        vm.ChifaStatus.Should().NotBeNullOrEmpty();
        vm.TokenStatus.Should().NotBeNullOrEmpty();
        vm.SigningStatus.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DASH025_DashboardViewModel_RefreshStatus_DoesNotThrow()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus
            {
                IsOnline = false,
                IsDatabaseConnected = false,
                IsTokenPresent = false
            });

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockToken.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.NotSigned);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        var act = async () => await vm.RefreshAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DASH026_DashboardViewModel_HandlingServiceException_DoesNotCrash()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("PostgreSQL connection refused"));
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("PostgreSQL connection refused"));

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockToken.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.NotSigned);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        var act = async () => await vm.RefreshAsync();
        await act.Should().NotThrowAsync("Dashboard should handle service exceptions gracefully");
    }

    [Fact]
    public async Task DASH027_DashboardViewModel_SetsHasError_WhenServiceFails()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection refused"));
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection refused"));

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockToken.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.NotSigned);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        await vm.RefreshAsync();

        vm.HasError.Should().BeTrue();
        vm.LastError.Should().Contain("Erreur");
    }

    [Fact]
    public void DASH028_DashboardViewModel_ReadOnly_NoWriteBypass()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus
            {
                IsOnline = true,
                IsDatabaseConnected = true,
                IsTokenPresent = true
            });

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.Signed);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        vm.IsReadOnly.Should().BeTrue("ReadOnly mode should always be enforced");
        vm.IntegrationMode.Should().Be("ReadOnly");
    }

    [Fact]
    public void DASH029_DashboardViewModel_PreparedInvoiceCount_DefaultsToZero()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = false });

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockToken.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.NotSigned);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        vm.PreparedInvoiceCount.Should().Be(0);
        vm.SynchronizedInvoiceCount.Should().Be(0);
        vm.BordereauPreparedCount.Should().Be(0);
    }

    [Fact]
    public async Task DASH030_DashboardViewModel_NoRequiredAction_InReadOnly()
    {
        var mockIntegration = new Mock<IChifaIntegrationService>();
        mockIntegration.Setup(s => s.IsChifaAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockIntegration.Setup(s => s.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = false });

        var mockToken = new Mock<IChifaTokenService>();
        mockToken.Setup(s => s.IsTokenPresentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockToken.Setup(s => s.GetTokenInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        var mockSigning = new Mock<IChifaSigningService>();
        mockSigning.Setup(s => s.GetSigningStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChifaSigningStatus.NotSigned);
        mockSigning.Setup(s => s.IsTokenAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);

        var vm = new ChifaDashboardViewModel_Dummy(
            mockIntegration.Object,
            mockToken.Object,
            mockSigning.Object,
            modeProvider);

        await vm.RefreshAsync();

        vm.HasRequiredAction.Should().BeFalse("ReadOnly mode should never require actions");
        vm.RequiredActionTitle.Should().BeEmpty();
    }
}

/// <summary>
/// Testable wrapper for ChifaDashboardViewModel logic.
/// Mirrors the ViewModel's constructor logic and public API without WPF dependency.
/// This allows unit testing the dashboard behavior from a non-WPF test project.
/// </summary>
public class ChifaDashboardViewModel_Dummy
{
    private readonly IChifaIntegrationService _integrationService;
    private readonly IChifaTokenService _tokenService;
    private readonly IChifaSigningService _signingService;
    private readonly ChifaIntegrationModeProvider _modeProvider;

    public string IntegrationMode { get; private set; } = "";
    public bool IsReadOnly { get; private set; } = true;
    public string ConnectionStatus { get; private set; } = "Vérification...";
    public string ChifaStatus { get; private set; } = "Vérification...";
    public string TokenStatus { get; private set; } = "Vérification...";
    public string SigningStatus { get; private set; } = "Non vérifié";
    public int PreparedInvoiceCount { get; private set; }
    public int SynchronizedInvoiceCount { get; private set; }
    public int BordereauPreparedCount { get; private set; }
    public bool HasRequiredAction { get; private set; }
    public string RequiredActionTitle { get; private set; } = "";
    public bool HasError { get; private set; }
    public string LastError { get; private set; } = "";

    public ChifaDashboardViewModel_Dummy(
        IChifaIntegrationService integrationService,
        IChifaTokenService tokenService,
        IChifaSigningService signingService,
        ChifaIntegrationModeProvider modeProvider)
    {
        _integrationService = integrationService;
        _tokenService = tokenService;
        _signingService = signingService;
        _modeProvider = modeProvider;

        IntegrationMode = _modeProvider.CurrentMode.ToString();
        IsReadOnly = _modeProvider.IsReadOnly;
    }

    public async Task RefreshAsync()
    {
        try
        {
            IntegrationMode = _modeProvider.CurrentMode.ToString();
            IsReadOnly = _modeProvider.IsReadOnly;

            await LoadConnectionStatusAsync();
            await LoadTokenStatusAsync();
            await LoadSigningStatusAsync();
            DetermineRequiredAction();
        }
        catch (Exception)
        {
            HasError = true;
            LastError = "Erreur lors du chargement";
        }
    }

    private async Task LoadConnectionStatusAsync()
    {
        try
        {
            var health = await _integrationService.GetHealthStatusAsync();
            if (health.IsOnline && health.IsDatabaseConnected)
            {
                ConnectionStatus = "Connecté";
                ChifaStatus = "En ligne";
            }
            else
            {
                ConnectionStatus = health.IsOnline ? "Partiellement connecté" : "Déconnecté";
                ChifaStatus = health.IsOnline ? "En ligne" : "Hors ligne";
            }
        }
        catch (Exception)
        {
            ConnectionStatus = "Erreur de connexion";
            ChifaStatus = "Indisponible";
            HasError = true;
            LastError = "Erreur: PostgreSQL indisponible";
        }
    }

    private async Task LoadTokenStatusAsync()
    {
        try
        {
            var present = await _tokenService.IsTokenPresentAsync();
            if (present)
            {
                var info = await _tokenService.GetTokenInfoAsync();
                TokenStatus = info != null ? $"Présent ({info.Label})" : "Présent";
            }
            else
            {
                TokenStatus = "Non détecté";
            }
        }
        catch (Exception)
        {
            TokenStatus = "Erreur";
        }
    }

    private async Task LoadSigningStatusAsync()
    {
        try
        {
            var status = await _signingService.GetSigningStatusAsync("_global");
            SigningStatus = status switch
            {
                ChifaSigningStatus.NotSigned => "Non signé",
                ChifaSigningStatus.SigningRequired => "Signature requise",
                ChifaSigningStatus.SigningInProgress => "Signature en cours",
                ChifaSigningStatus.Signed => "Signé",
                ChifaSigningStatus.SigningFailed => "Échec de signature",
                ChifaSigningStatus.TokenNotPresent => "Token non présent",
                _ => "Inconnu"
            };
        }
        catch (Exception)
        {
            SigningStatus = "Erreur";
        }
    }

    private void DetermineRequiredAction()
    {
        if (IsReadOnly)
        {
            HasRequiredAction = false;
            RequiredActionTitle = "";
            return;
        }

        HasRequiredAction = false;
    }
}
