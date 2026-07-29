using System.Diagnostics;
using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaIntegrationFacadeTests
{
    private readonly Mock<IChifaIntegrationService> _integrationMock = new();
    private readonly Mock<IChifaInvoiceService> _invoiceMock = new();
    private readonly Mock<IChifaBordereauService> _bordereauMock = new();
    private readonly Mock<IChifaTokenService> _tokenMock = new();
    private readonly Mock<IChifaSigningService> _signingMock = new();
    private readonly Mock<IChifaAuditService> _auditMock = new();
    private readonly Mock<IChifaInvoiceWorkflowService> _workflowMock = new();
    private readonly Mock<IBordereauStatusService> _bordereauStatusMock = new();
    private readonly Mock<IChifaNumberingService> _numberingMock = new();
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly Mock<ILogger<ChifaIntegrationFacade>> _loggerMock = new();
    private readonly ChifaCircuitBreaker _circuitBreaker = new();
    private readonly ChifaMetricsService _metrics = new();
    private readonly CorrelationContext _correlation = new();
    private ChifaIntegrationFacade CreateFacade()
    {
        var config = new ChifaIntegrationConfig { Mode = "ReadOnly" };
        var modeProvider = new ChifaIntegrationModeProvider(config);
        var statusEngine = new StatusEngine(
            _integrationMock.Object, _tokenMock.Object, _signingMock.Object,
            _invoiceMock.Object, _bordereauMock.Object, _circuitBreaker,
            Mock.Of<ILogger<StatusEngine>>());
        var invoiceSync = new InvoiceSynchronizer(
            _invoiceMock.Object, _integrationMock.Object,
            Mock.Of<ILogger<InvoiceSynchronizer>>());
        var bordereauSync = new BordereauSynchronizer(
            _bordereauMock.Object, _integrationMock.Object,
            _bordereauStatusMock.Object, Mock.Of<ILogger<BordereauSynchronizer>>());
        var statusSync = new StatusSynchronizer(
            Mock.Of<BMPharma.Persistence.PostgreSQL.Contexts.ChifaPostgreSqlContext>(),
            _integrationMock.Object, Mock.Of<ILogger<StatusSynchronizer>>());
        var healthCheck = new ChifaHealthCheckService(
            _integrationMock.Object, _tokenMock.Object, _signingMock.Object,
            Mock.Of<BMPharma.Persistence.PostgreSQL.Contexts.ChifaPostgreSqlContext>(),
            Mock.Of<BMPharma.Persistence.PostgreSQL.Contexts.ChifaWriteDbContext>(),
            Mock.Of<ILogger<ChifaHealthCheckService>>());
        var monitoring = new ChifaMonitoringService(healthCheck, _metrics, _circuitBreaker, _correlation, 0);

        return new ChifaIntegrationFacade(
            _integrationMock.Object, _invoiceMock.Object, _bordereauMock.Object,
            _tokenMock.Object, _signingMock.Object, _auditMock.Object,
            _workflowMock.Object, _bordereauStatusMock.Object,
            Mock.Of<ChifaWorkflowStateMachine>(), _numberingMock.Object,
            modeProvider, statusEngine, invoiceSync, bordereauSync, statusSync,
            monitoring, _metrics, _correlation, _loggerMock.Object);
    }

    [Fact]
    public async Task FAC001_IsAvailableAsync_DelegatesToIntegration()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var facade = CreateFacade();
        var result = await facade.IsAvailableAsync();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FAC002_IsAvailableAsync_ReturnsFalseOnException()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));
        var facade = CreateFacade();
        var result = await facade.IsAvailableAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FAC003_GetHealthStatusAsync_Delegates()
    {
        var health = new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = true };
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>())).ReturnsAsync(health);
        var facade = CreateFacade();
        var result = await facade.GetHealthStatusAsync();
        result.IsOnline.Should().BeTrue();
        result.IsDatabaseConnected.Should().BeTrue();
    }

    [Fact]
    public async Task FAC004_GetTokenStatusAsync_TokenPresent()
    {
        _tokenMock.Setup(x => x.IsTokenPresentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tokenMock.Setup(x => x.GetTokenInfoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ChifaTokenInfo { Label = "TestToken", IsValid = true });
        var facade = CreateFacade();
        var (present, label, valid) = await facade.GetTokenStatusAsync();
        present.Should().BeTrue();
        label.Should().Be("TestToken");
        valid.Should().BeTrue();
    }

    [Fact]
    public async Task FAC005_GetTokenStatusAsync_TokenAbsent()
    {
        _tokenMock.Setup(x => x.IsTokenPresentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var facade = CreateFacade();
        var (present, _, _) = await facade.GetTokenStatusAsync();
        present.Should().BeFalse();
    }

    [Fact]
    public async Task FAC006_GetSigningStatusAsync_Delegates()
    {
        _signingMock.Setup(x => x.GetSigningStatusAsync("_global", It.IsAny<CancellationToken>())).ReturnsAsync(ChifaSigningStatus.Signed);
        var facade = CreateFacade();
        var result = await facade.GetSigningStatusAsync();
        result.Should().Be(ChifaSigningStatus.Signed);
    }

    [Fact]
    public async Task FAC007_GetModeAsync_ReadOnly()
    {
        var facade = CreateFacade();
        var mode = await facade.GetModeAsync();
        mode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    [Fact]
    public async Task FAC008_CreateInvoiceAsync_ReadOnly_ReturnsNotAvailable()
    {
        var facade = CreateFacade();
        var request = new ChifaInvoiceRequest { NumFact = "FAC001" };
        var result = await facade.CreateInvoiceAsync(request);
        result.Status.Should().Be(FacadeOperationStatus.NotAvailable);
    }

    [Fact]
    public async Task FAC009_PrepareInvoiceAsync_ReadOnly_ReturnsNotAvailable()
    {
        var facade = CreateFacade();
        var request = new ChifaInvoiceRequest { NumFact = "FAC001" };
        var result = await facade.PrepareInvoiceAsync(request);
        result.Status.Should().Be(FacadeOperationStatus.NotAvailable);
    }

    [Fact]
    public async Task FAC010_ValidateInvoiceAsync_DelegatesToWorkflow()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var modeProvider = new ChifaIntegrationModeProvider(config);
        var facade = CreateFacade();

        var wfResult = new ChifaWorkflowResult { IsSuccess = true, ComputedMontFact = 100 };
        _workflowMock.Setup(x => x.ValidateOnlyAsync(It.IsAny<ChifaInvoiceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wfResult);

        var request = new ChifaInvoiceRequest { NumFact = "FAC001" };
        var result = await facade.ValidateInvoiceAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task FAC011_DeleteDraftAsync_InvoiceNotFound()
    {
        _invoiceMock.Setup(x => x.InvoiceExistsInChifaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var facade = CreateFacade();
        var result = await facade.DeleteDraftAsync("NONEXISTENT");
        result.Status.Should().Be(FacadeOperationStatus.NotFound);
    }

    [Fact]
    public async Task FAC012_GetInvoiceStatusAsync_Exists()
    {
        _invoiceMock.Setup(x => x.InvoiceExistsInChifaAsync("FAC001", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var facade = CreateFacade();
        var result = await facade.GetInvoiceStatusAsync("FAC001");
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ExistsInChifa.Should().BeTrue();
    }

    [Fact]
    public async Task FAC013_GetInvoiceStatusAsync_NotFound()
    {
        _invoiceMock.Setup(x => x.InvoiceExistsInChifaAsync("NOPE", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var facade = CreateFacade();
        var result = await facade.GetInvoiceStatusAsync("NOPE");
        result.IsSuccess.Should().BeTrue();
        result.Data!.ExistsInChifa.Should().BeFalse();
    }

    [Fact]
    public async Task FAC014_GetDashboardOverviewAsync_ReturnsOverview()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = true });
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var facade = CreateFacade();
        var result = await facade.GetDashboardOverviewAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().NotBeNull();
    }

    [Fact]
    public async Task FAC015_RefreshStatusAsync_ReturnsSnapshot()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = true });
        var facade = CreateFacade();
        var result = await facade.RefreshStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task FAC016_CreateInvoiceAsync_Conflict()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var modeProvider = new ChifaIntegrationModeProvider(config);
        _invoiceMock.Setup(x => x.InvoiceExistsInChifaAsync("EXISTING", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var facade = CreateFacade();
        var request = new ChifaInvoiceRequest { NumFact = "EXISTING" };
        var result = await facade.CreateInvoiceAsync(request);
        result.Status.Should().Be(FacadeOperationStatus.Conflict);
    }

    [Fact]
    public async Task FAC017_GetBordereauStatusAsync_Found()
    {
        var bResult = new BordereauWorkflowResult
        {
            NumBord = "B001", IsSuccess = true,
            State = BordereauWorkflowState.Created,
            InvoiceCount = 5
        };
        _bordereauStatusMock.Setup(x => x.GetStatusAsync("B001", It.IsAny<CancellationToken>())).ReturnsAsync(bResult);
        var facade = CreateFacade();
        var result = await facade.GetBordereauStatusAsync("B001");
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.NumBord.Should().Be("B001");
    }

    [Fact]
    public async Task FAC018_GetBordereauStatusAsync_NotFound()
    {
        var bResult = new BordereauWorkflowResult { IsSuccess = false, ErrorMessage = "not found" };
        _bordereauStatusMock.Setup(x => x.GetStatusAsync("B999", It.IsAny<CancellationToken>())).ReturnsAsync(bResult);
        var facade = CreateFacade();
        var result = await facade.GetBordereauStatusAsync("B999");
        result.Status.Should().Be(FacadeOperationStatus.NotFound);
    }

    [Fact]
    public async Task FAC019_ExecuteFullWorkflowAsync_Delegates()
    {
        var config = new ChifaIntegrationConfig { Mode = "Test" };
        var modeProvider = new ChifaIntegrationModeProvider(config);
        var wfResult = new ChifaWorkflowResult { IsSuccess = true, Step = "COMPLETED", ChifaNumFact = "FAC001" };
        _workflowMock.Setup(x => x.ExecuteFullWorkflowAsync(It.IsAny<ChifaInvoiceRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wfResult);
        var facade = CreateFacade();
        var request = new ChifaInvoiceRequest { NumFact = "FAC001" };
        var result = await facade.ExecuteFullWorkflowAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task FAC020_GetAllBordereauxAsync_Delegates()
    {
        var list = new List<BordereauWorkflowSummary>
        {
            new() { NumBord = "B001", InvoiceCount = 3 },
            new() { NumBord = "B002", InvoiceCount = 5 }
        };
        _bordereauStatusMock.Setup(x => x.GetAllBordereaux()).Returns(list);
        var facade = CreateFacade();
        var result = await facade.GetAllBordereauxAsync();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FAC021_GetWorkflowAuditLogAsync_Delegates()
    {
        var entries = new List<ChifaWorkflowAuditEntry>
        {
            new() { Operation = "VALIDATE", Result = "OK" },
            new() { Operation = "CREATE", Result = "OK" }
        };
        _workflowMock.Setup(x => x.GetAuditLog()).Returns(entries);
        var facade = CreateFacade();
        var result = await facade.GetWorkflowAuditLogAsync(10);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FAC022_GetBordereauAuditLogAsync_Delegates()
    {
        var entries = new List<BordereauWorkflowAuditEntry>
        {
            new() { Operation = "CREATE", Result = "OK" }
        };
        _bordereauStatusMock.Setup(x => x.GetAuditLog(null)).Returns(entries);
        var facade = CreateFacade();
        var result = await facade.GetBordereauAuditLogAsync();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task FAC023_CreateBordereauAsync_Delegates()
    {
        var bResult = new BordereauWorkflowResult { IsSuccess = true, NumBord = "B001", InvoiceCount = 3 };
        _bordereauStatusMock.Setup(x => x.CreateBordereauAsync("B001", "11600", It.Is<List<string>>(l => l.Count == 2), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bResult);
        var facade = CreateFacade();
        var result = await facade.CreateBordereauAsync("B001", "11600", new List<string> { "F1", "F2" });
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FAC024_ValidateBordereauAsync_Delegates()
    {
        var bResult = new BordereauWorkflowResult { IsSuccess = true, NumBord = "B001" };
        _bordereauStatusMock.Setup(x => x.ValidateBordereauAsync("B001", It.IsAny<CancellationToken>())).ReturnsAsync(bResult);
        var facade = CreateFacade();
        var result = await facade.ValidateBordereauAsync("B001");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FAC025_SignBordereauAsync_Delegates()
    {
        var bResult = new BordereauWorkflowResult { IsSuccess = true, NumBord = "B001" };
        _bordereauStatusMock.Setup(x => x.SignBordereauAsync("B001", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(bResult);
        var facade = CreateFacade();
        var result = await facade.SignBordereauAsync("B001");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FAC026_CloseBordereauAsync_Delegates()
    {
        var bResult = new BordereauWorkflowResult { IsSuccess = true, NumBord = "B001" };
        _bordereauStatusMock.Setup(x => x.CloseBordereauAsync("B001", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(bResult);
        var facade = CreateFacade();
        var result = await facade.CloseBordereauAsync("B001");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FAC027_TransmitBordereauAsync_Delegates()
    {
        var bResult = new BordereauWorkflowResult { IsSuccess = true, NumBord = "B001" };
        _bordereauStatusMock.Setup(x => x.TransmitBordereauAsync("B001", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(bResult);
        var facade = CreateFacade();
        var result = await facade.TransmitBordereauAsync("B001");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FAC028_SynchronizeAsync_ReturnsSummary()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var facade = CreateFacade();
        var result = await facade.SynchronizeAsync();
        result.Should().NotBeNull();
    }
}
