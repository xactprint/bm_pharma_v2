using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class StatusEngineTests
{
    private readonly Mock<IChifaIntegrationService> _integrationMock = new();
    private readonly Mock<IChifaTokenService> _tokenMock = new();
    private readonly Mock<IChifaSigningService> _signingMock = new();
    private readonly Mock<IChifaInvoiceService> _invoiceMock = new();
    private readonly Mock<IChifaBordereauService> _bordereauMock = new();
    private readonly ChifaCircuitBreaker _circuitBreaker = new(new CircuitBreakerOptions
    {
        FailureThreshold = 1, OpenTimeoutMs = 30000, SuccessThreshold = 1
    });

    private StatusEngine CreateEngine() => new(
        _integrationMock.Object, _tokenMock.Object, _signingMock.Object,
        _invoiceMock.Object, _bordereauMock.Object, _circuitBreaker,
        Mock.Of<ILogger<StatusEngine>>());

    [Fact]
    public async Task ST001_Evaluate_Connected()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = true });
        var engine = CreateEngine();
        var result = await engine.EvaluateAsync();
        result.Technical.Should().Be(TechnicalStatus.Connected);
    }

    [Fact]
    public async Task ST002_Evaluate_Degraded_OnlineOnly()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = false });
        var engine = CreateEngine();
        var result = await engine.EvaluateAsync();
        result.Technical.Should().Be(TechnicalStatus.Degraded);
    }

    [Fact]
    public async Task ST003_Evaluate_Degraded_DbOnly()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = false, IsDatabaseConnected = true });
        var engine = CreateEngine();
        var result = await engine.EvaluateAsync();
        result.Technical.Should().Be(TechnicalStatus.Degraded);
    }

    [Fact]
    public async Task ST004_Evaluate_Disconnected()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = false, IsDatabaseConnected = false });
        var engine = CreateEngine();
        var result = await engine.EvaluateAsync();
        result.Technical.Should().Be(TechnicalStatus.Disconnected);
    }

    [Fact]
    public async Task ST005_Evaluate_Exception_CircuitBreakerOpens()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("timeout"));
        var engine = CreateEngine();
        var result = await engine.EvaluateAsync();
        result.Technical.Should().Be(TechnicalStatus.Disconnected);
        result.ErrorMessage.Should().Contain("timeout");
    }

    [Fact]
    public async Task ST006_Evaluate_CircuitBreakerOpen_ReturnsDisconnected()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("fail"));
        var engine = CreateEngine();

        await engine.EvaluateAsync();
        var result = await engine.EvaluateAsync();
        result.Technical.Should().Be(TechnicalStatus.Disconnected);
        result.ErrorMessage.Should().Contain("Circuit breaker is open");
    }

    [Fact]
    public async Task ST007_Evaluate_HasTimestamp()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = true });
        var engine = CreateEngine();
        var result = await engine.EvaluateAsync();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ST008_Evaluate_HasDuration()
    {
        _integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = true });
        var engine = CreateEngine();
        var result = await engine.EvaluateAsync();
        result.DurationMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void ST009_StatusSnapshot_IsReady_True()
    {
        var snap = new ChifaStatusSnapshot
        {
            Technical = TechnicalStatus.Connected,
            Business = BusinessStatus.Persisted,
            Visibility = VisibilityStatus.VisibleInFacture
        };
        snap.IsReady.Should().BeTrue();
    }

    [Fact]
    public void ST010_StatusSnapshot_IsReady_False_Technical()
    {
        var snap = new ChifaStatusSnapshot
        {
            Technical = TechnicalStatus.Disconnected,
            Business = BusinessStatus.Persisted,
            Visibility = VisibilityStatus.VisibleInFacture
        };
        snap.IsReady.Should().BeFalse();
    }

    [Fact]
    public void ST011_StatusSnapshot_IsReady_False_Business()
    {
        var snap = new ChifaStatusSnapshot
        {
            Technical = TechnicalStatus.Connected,
            Business = BusinessStatus.Draft,
            Visibility = VisibilityStatus.VisibleInFacture
        };
        snap.IsReady.Should().BeFalse();
    }

    [Fact]
    public void ST012_StatusSnapshot_IsReady_False_Visibility()
    {
        var snap = new ChifaStatusSnapshot
        {
            Technical = TechnicalStatus.Connected,
            Business = BusinessStatus.Persisted,
            Visibility = VisibilityStatus.NotVisible
        };
        snap.IsReady.Should().BeFalse();
    }

    [Fact]
    public void ST013_StatusSnapshot_Defaults()
    {
        var snap = new ChifaStatusSnapshot();
        snap.Technical.Should().Be(TechnicalStatus.Unknown);
        snap.Business.Should().Be(BusinessStatus.Unknown);
        snap.Visibility.Should().Be(VisibilityStatus.Unknown);
        snap.IsReady.Should().BeFalse();
    }
}
