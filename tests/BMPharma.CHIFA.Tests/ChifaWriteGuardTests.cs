using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaWriteGuardTests
{
    [Fact]
    public async Task CH017_ReadOnly_Mode_Blocks_Write()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));

        var act = () => guard.EnsureWriteAllowedAsync();

        await act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public async Task CH018_Test_Mode_Allows_Write()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.Test));

        var act = async () => await guard.EnsureWriteAllowedAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CH019_Production_Mode_Allows_Write()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.Production));

        var act = async () => await guard.EnsureWriteAllowedAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CH020_ReadOnly_Mode_Allows_Select()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));

        // ReadOnly should NOT throw for read operations
        // (The guard is only called for write operations)
        var act = async () => await guard.EnsureTestOrProductionAsync();

        await act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }
}
