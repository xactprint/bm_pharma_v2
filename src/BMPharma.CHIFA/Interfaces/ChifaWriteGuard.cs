using BMPharma.Domain.Enums;

namespace BMPharma.CHIFA.Interfaces;

public class ChifaWriteGuard
{
    private readonly Func<Task<ChifaIntegrationMode>> _getMode;

    public ChifaWriteGuard(Func<Task<ChifaIntegrationMode>> getMode)
    {
        _getMode = getMode;
    }

    public async Task EnsureWriteAllowedAsync()
    {
        var mode = await _getMode();
        if (mode == ChifaIntegrationMode.ReadOnly)
        {
            throw new ChifaWriteBlockedException(
                "CHIFA integration is in ReadOnly mode. " +
                "Change mode in Settings > CHIFA Integration to allow writes.");
        }
    }

    public async Task EnsureTestOrProductionAsync()
    {
        var mode = await _getMode();
        if (mode == ChifaIntegrationMode.ReadOnly)
        {
            throw new ChifaWriteBlockedException(
                "CHIFA integration is in ReadOnly mode. Writes are blocked.");
        }
    }
}

public class ChifaWriteBlockedException : Exception
{
    public ChifaWriteBlockedException(string message) : base(message) { }
}
