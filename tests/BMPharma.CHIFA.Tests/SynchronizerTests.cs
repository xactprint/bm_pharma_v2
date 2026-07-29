using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using BMPharma.Persistence.PostgreSQL.Contexts;

namespace BMPharma.CHIFA.Tests;

public class SynchronizerTests
{
    private readonly Mock<IChifaInvoiceService> _invoiceMock = new();
    private readonly Mock<IChifaBordereauService> _bordereauMock = new();
    private readonly Mock<IChifaIntegrationService> _integrationMock = new();
    private readonly Mock<IBordereauStatusService> _bordereauStatusMock = new();

    [Fact]
    public async Task SYNC001_InvoiceSynchronizer_Available_ReturnsSummary()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sync = new InvoiceSynchronizer(_invoiceMock.Object, _integrationMock.Object,
            Mock.Of<ILogger<InvoiceSynchronizer>>());
        var result = await sync.SynchronizeAsync();
        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SYNC002_InvoiceSynchronizer_NotAvailable_ReturnsError()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sync = new InvoiceSynchronizer(_invoiceMock.Object, _integrationMock.Object,
            Mock.Of<ILogger<InvoiceSynchronizer>>());
        var result = await sync.SynchronizeAsync();
        result.Errors.Should().Contain(e => e.Contains("not available"));
    }

    [Fact]
    public async Task SYNC003_InvoiceSynchronizer_Exception_ReturnsError()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));
        var sync = new InvoiceSynchronizer(_invoiceMock.Object, _integrationMock.Object,
            Mock.Of<ILogger<InvoiceSynchronizer>>());
        var result = await sync.SynchronizeAsync();
        result.Errors.Should().Contain(e => e.Contains("fail"));
    }

    [Fact]
    public async Task SYNC004_BordereauSynchronizer_Available_ReturnsSummary()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sync = new BordereauSynchronizer(_bordereauMock.Object, _integrationMock.Object,
            _bordereauStatusMock.Object, Mock.Of<ILogger<BordereauSynchronizer>>());
        var result = await sync.SynchronizeAsync();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task SYNC005_BordereauSynchronizer_NotAvailable_ReturnsError()
    {
        _integrationMock.Setup(x => x.IsChifaAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var sync = new BordereauSynchronizer(_bordereauMock.Object, _integrationMock.Object,
            _bordereauStatusMock.Object, Mock.Of<ILogger<BordereauSynchronizer>>());
        var result = await sync.SynchronizeAsync();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void SYNC006_ChifaSyncSummary_Defaults()
    {
        var summary = new ChifaSyncSummary();
        summary.InvoicesFound.Should().Be(0);
        summary.InvoicesUpdated.Should().Be(0);
        summary.BordereauxFound.Should().Be(0);
        summary.BordereauxUpdated.Should().Be(0);
        summary.Errors.Should().BeEmpty();
    }

    private static ChifaPostgreSqlContext CreateReadOnlyContext()
    {
        var options = new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ChifaPostgreSqlContext(options);
    }

    [Fact]
    public async Task SYNC007_StatusSynchronizer_LoadInvoice_ReturnsNull_WhenNotFound()
    {
        using var ctx = CreateReadOnlyContext();
        var sync = new StatusSynchronizer(ctx, _integrationMock.Object,
            Mock.Of<ILogger<StatusSynchronizer>>());
        var result = await sync.LoadInvoiceAsync("NONEXISTENT");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SYNC008_StatusSynchronizer_LoadAllInvoices_ReturnsEmpty_WhenNoData()
    {
        using var ctx = CreateReadOnlyContext();
        var sync = new StatusSynchronizer(ctx, _integrationMock.Object,
            Mock.Of<ILogger<StatusSynchronizer>>());
        var result = await sync.LoadAllInvoicesAsync();
        result.Should().BeEmpty();
    }
}
