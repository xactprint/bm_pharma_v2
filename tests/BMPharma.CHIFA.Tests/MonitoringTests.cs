using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class MonitoringTests
{
    [Fact]
    public void MET001_ChifaMetricsService_RecordIncrements()
    {
        var metrics = new ChifaMetricsService();
        metrics.Record("TestOp", true, 10);
        metrics.Record("TestOp", true, 20);
        metrics.Record("TestOp", false, 5);

        var all = metrics.GetAll();
        all.Should().HaveCount(3);
    }

    [Fact]
    public void MET002_ChifaMetricsService_GetSummary_All()
    {
        var metrics = new ChifaMetricsService();
        metrics.Record("Op1", true, 10);
        metrics.Record("Op1", true, 20);
        metrics.Record("Op1", false, 30);

        var (total, success, failed, avgMs) = metrics.GetSummary("Op1");
        total.Should().Be(3);
        success.Should().Be(2);
        failed.Should().Be(1);
        avgMs.Should().Be(20);
    }

    [Fact]
    public void MET003_ChifaMetricsService_GetSummary_Empty()
    {
        var metrics = new ChifaMetricsService();
        var (total, success, failed, avgMs) = metrics.GetSummary();
        total.Should().Be(0);
        success.Should().Be(0);
        failed.Should().Be(0);
        avgMs.Should().Be(0);
    }

    [Fact]
    public void MET004_ChifaMetricsService_Reset()
    {
        var metrics = new ChifaMetricsService();
        metrics.Record("Op", true, 5);
        metrics.Reset();
        metrics.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void MET005_ChifaMetricsService_SummaryWithNoOperation()
    {
        var metrics = new ChifaMetricsService();
        metrics.Record("A", true, 10);
        metrics.Record("B", false, 5);
        var (total, success, _, _) = metrics.GetSummary();
        total.Should().Be(2);
        success.Should().Be(1);
    }

    [Fact]
    public void CB001_CircuitBreaker_InitialStateClosed()
    {
        var cb = new ChifaCircuitBreaker();
        cb.GetState("test").Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void CB002_CircuitBreaker_OpensAfterThreshold()
    {
        var cb = new ChifaCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 2, OpenTimeoutMs = 30000, SuccessThreshold = 1
        });

        cb.RecordFailure("test");
        cb.GetState("test").Should().Be(CircuitState.Closed);
        cb.RecordFailure("test");
        cb.GetState("test").Should().Be(CircuitState.Open);
    }

    [Fact]
    public void CB003_CircuitBreaker_HalfOpenAfterTimeout()
    {
        var cb = new ChifaCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 1, OpenTimeoutMs = 1, SuccessThreshold = 1
        });

        cb.RecordFailure("test");
        Thread.Sleep(10);
        cb.GetState("test").Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public void CB004_CircuitBreaker_ClosesAfterSuccessThreshold()
    {
        var cb = new ChifaCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 1, OpenTimeoutMs = 1, SuccessThreshold = 1
        });

        cb.RecordFailure("test");
        Thread.Sleep(10);
        cb.RecordSuccess("test");
        cb.GetState("test").Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void CB005_CircuitBreaker_IsOpen_True()
    {
        var cb = new ChifaCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 1, OpenTimeoutMs = 30000, SuccessThreshold = 2
        });
        cb.RecordFailure("test");
        cb.IsOpen("test").Should().BeTrue();
    }

    [Fact]
    public void CB006_CircuitBreaker_IsOpen_False()
    {
        var cb = new ChifaCircuitBreaker();
        cb.IsOpen("test").Should().BeFalse();
    }

    [Fact]
    public void CB007_CircuitBreaker_Reset()
    {
        var cb = new ChifaCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 1, OpenTimeoutMs = 30000, SuccessThreshold = 2
        });
        cb.RecordFailure("test");
        cb.Reset("test");
        cb.IsOpen("test").Should().BeFalse();
        cb.GetState("test").Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void CB008_CircuitBreaker_MultipleKeys()
    {
        var cb = new ChifaCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 1, OpenTimeoutMs = 30000, SuccessThreshold = 1
        });
        cb.RecordFailure("key1");
        cb.IsOpen("key1").Should().BeTrue();
        cb.IsOpen("key2").Should().BeFalse();
    }

    [Fact]
    public void COR001_CorrelationContext_GetOrCreate()
    {
        var ctx = new CorrelationContext();
        var id = ctx.GetOrCreate();
        id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void COR002_CorrelationContext_SameIdOnRepeatCall()
    {
        var ctx = new CorrelationContext();
        var id1 = ctx.GetOrCreate();
        var id2 = ctx.GetOrCreate();
        id2.Should().Be(id1);
    }

    [Fact]
    public void COR003_CorrelationContext_Reset()
    {
        var ctx = new CorrelationContext();
        ctx.GetOrCreate();
        ctx.Reset();
        ctx.Current.Should().BeNull();
    }

    [Fact]
    public void COR004_CorrelationContext_NewIdAfterReset()
    {
        var ctx = new CorrelationContext();
        var id1 = ctx.GetOrCreate();
        ctx.Reset();
        var id2 = ctx.GetOrCreate();
        id2.Should().NotBe(id1);
    }

    [Fact]
    public async Task HLT001_ChifaHealthCheckService_Delegates()
    {
        var integrationMock = new Mock<IChifaIntegrationService>();
        integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChifaHealthStatus { IsOnline = true, IsDatabaseConnected = true });

        var service = new ChifaHealthCheckService(
            integrationMock.Object,
            Mock.Of<IChifaTokenService>(),
            Mock.Of<IChifaSigningService>(),
            Mock.Of<BMPharma.Persistence.PostgreSQL.Contexts.ChifaPostgreSqlContext>(),
            Mock.Of<BMPharma.Persistence.PostgreSQL.Contexts.ChifaWriteDbContext>(),
            Mock.Of<ILogger<ChifaHealthCheckService>>());

        var result = await service.CheckAllAsync();
        result.IsOnline.Should().BeTrue();
        result.IsDatabaseConnected.Should().BeTrue();
    }

    [Fact]
    public async Task HLT002_ChifaHealthCheckService_Exception_ReturnsDefault()
    {
        var integrationMock = new Mock<IChifaIntegrationService>();
        integrationMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("fail"));

        var service = new ChifaHealthCheckService(
            integrationMock.Object,
            Mock.Of<IChifaTokenService>(),
            Mock.Of<IChifaSigningService>(),
            Mock.Of<BMPharma.Persistence.PostgreSQL.Contexts.ChifaPostgreSqlContext>(),
            Mock.Of<BMPharma.Persistence.PostgreSQL.Contexts.ChifaWriteDbContext>(),
            Mock.Of<ILogger<ChifaHealthCheckService>>());

        var result = await service.CheckAllAsync();
        result.IsOnline.Should().BeFalse();
        result.ErrorMessage.Should().Contain("fail");
    }

    [Fact]
    public void MON001_ChifaMonitoringService_RecordOperation()
    {
        var service = new ChifaMonitoringService(
            Mock.Of<ChifaHealthCheckService>(),
            new ChifaMetricsService(),
            new ChifaCircuitBreaker(),
            new CorrelationContext(), 0);

        service.RecordOperation("TestOp");
        service.LastOperation.Should().Be("TestOp");
    }

    [Fact]
    public void MON002_ChifaMonitoringService_RecordSyncResult()
    {
        var service = new ChifaMonitoringService(
            Mock.Of<ChifaHealthCheckService>(),
            new ChifaMetricsService(),
            new ChifaCircuitBreaker(),
            new CorrelationContext(), 0);

        var summary = new ChifaSyncSummary
        {
            InvoicesFound = 10,
            InvoicesUpdated = 5,
            BordereauxFound = 3,
            BordereauxUpdated = 2
        };
        service.RecordSyncResult(summary);
        service.LastSyncResult.Should().Be(summary);
        service.LastOperation.Should().Be("Synchronize");
    }

    [Fact]
    public async Task MON003_ChifaMonitoringService_GetMetrics()
    {
        var metrics = new ChifaMetricsService();
        metrics.Record("Op", true, 10);
        metrics.Record("Op", false, 20);

        var service = new ChifaMonitoringService(
            Mock.Of<ChifaHealthCheckService>(),
            metrics,
            new ChifaCircuitBreaker(),
            new CorrelationContext(), 0);

        var (total, success, failed, _) = service.GetMetrics("Op");
        total.Should().Be(2);
        success.Should().Be(1);
        failed.Should().Be(1);
    }

    [Fact]
    public void MON004_ChifaMonitoringService_ResetCircuitBreaker()
    {
        var cb = new ChifaCircuitBreaker(new CircuitBreakerOptions
        {
            FailureThreshold = 1, OpenTimeoutMs = 30000, SuccessThreshold = 1
        });
        cb.RecordFailure("test");

        var service = new ChifaMonitoringService(
            Mock.Of<ChifaHealthCheckService>(),
            new ChifaMetricsService(),
            cb,
            new CorrelationContext(), 0);

        service.GetCircuitState("test").Should().Be(CircuitState.Open);
        service.ResetCircuitBreaker("test");
        service.GetCircuitState("test").Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task MON005_ChifaMonitoringService_GetLastSyncResult_WhenNull()
    {
        var service = new ChifaMonitoringService(
            Mock.Of<ChifaHealthCheckService>(),
            new ChifaMetricsService(),
            new ChifaCircuitBreaker(),
            new CorrelationContext(), 0);

        var result = await service.GetLastSyncResultAsync();
        result.Should().BeNull();
    }
}
