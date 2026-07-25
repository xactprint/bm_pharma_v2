using FluentAssertions;
using BMPharma.CHIFA.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class StructuredChifaAuditServiceTests
{
    private readonly StructuredChifaAuditService _audit;

    public StructuredChifaAuditServiceTests()
    {
        var logger = new Mock<ILogger<StructuredChifaAuditService>>();
        _audit = new StructuredChifaAuditService(logger.Object);
    }

    [Fact]
    public async Task AUD001_LogOperation_RecordsEntry()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            true, 100, cancellationToken: CancellationToken.None);

        _audit.Entries.Should().HaveCount(1);
        _audit.Entries[0].Operation.Should().Be("TEST_OP");
        _audit.Entries[0].Result.Should().Be("SUCCESS");
    }

    [Fact]
    public async Task AUD002_LogOperation_WithDetails()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            "some details", true, 100, cancellationToken: CancellationToken.None);

        _audit.Entries[0].Details.Should().Be("some details");
    }

    [Fact]
    public async Task AUD003_LogOperation_ErrorResult()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            false, 100, "error message", cancellationToken: CancellationToken.None);

        _audit.Entries[0].Result.Should().Be("ERROR");
        _audit.Entries[0].Error.Should().Be("error message");
    }

    [Fact]
    public async Task AUD004_LogOperation_GeneratesCorrelationId()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            true, 100, cancellationToken: CancellationToken.None);

        _audit.Entries[0].CorrelationId.Should().NotBeNullOrEmpty();
        _audit.Entries[0].CorrelationId.Should().HaveLength(8);
    }

    [Fact]
    public async Task AUD005_LogOperation_SetsTimestamp()
    {
        var before = DateTime.UtcNow;
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            true, 100, cancellationToken: CancellationToken.None);
        var after = DateTime.UtcNow;

        _audit.Entries[0].Timestamp.Should().BeOnOrAfter(before);
        _audit.Entries[0].Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task AUD006_LogOperation_SetsSource()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            true, 100, cancellationToken: CancellationToken.None);

        _audit.Entries[0].Source.Should().Be("BM_PHARMA");
    }

    [Fact]
    public async Task AUD007_LogOperation_SetsDestination()
    {
        await _audit.LogOperationAsync("CHIFA_WRITE", "entity", "key1",
            true, 100, cancellationToken: CancellationToken.None);

        _audit.Entries[0].Destination.Should().Be("CHIFA_POSTGRESQL");
    }

    [Fact]
    public async Task AUD008_LogOperation_DefaultUser()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            true, 100, cancellationToken: CancellationToken.None);

        _audit.Entries[0].UserId.Should().Be("system");
    }

    [Fact]
    public async Task AUD009_LogOperation_CustomUser()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            true, 100, userId: "admin", cancellationToken: CancellationToken.None);

        _audit.Entries[0].UserId.Should().Be("admin");
    }

    [Fact]
    public async Task AUD010_Clear_RemovesAllEntries()
    {
        await _audit.LogOperationAsync("OP1", "e", "k", true, 0, cancellationToken: CancellationToken.None);
        await _audit.LogOperationAsync("OP2", "e", "k", true, 0, cancellationToken: CancellationToken.None);

        _audit.Clear();

        _audit.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task AUD011_MultipleEntries_AreOrdered()
    {
        await _audit.LogOperationAsync("OP1", "e", "k", true, 0, cancellationToken: CancellationToken.None);
        await _audit.LogOperationAsync("OP2", "e", "k", true, 0, cancellationToken: CancellationToken.None);
        await _audit.LogOperationAsync("OP3", "e", "k", true, 0, cancellationToken: CancellationToken.None);

        _audit.Entries.Should().HaveCount(3);
        _audit.Entries[0].Operation.Should().Be("OP1");
        _audit.Entries[2].Operation.Should().Be("OP3");
    }

    [Fact]
    public async Task AUD012_DurationMs_Recorded()
    {
        await _audit.LogOperationAsync("TEST_OP", "entity", "key1",
            true, 42, cancellationToken: CancellationToken.None);

        _audit.Entries[0].DurationMs.Should().Be(42);
    }
}
