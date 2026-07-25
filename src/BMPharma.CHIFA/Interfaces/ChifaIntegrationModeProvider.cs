using BMPharma.Domain.Enums;

namespace BMPharma.CHIFA.Interfaces;

public class ChifaIntegrationModeProvider
{
    private readonly ChifaIntegrationConfig _config;

    public ChifaIntegrationModeProvider(ChifaIntegrationConfig config)
    {
        _config = config;
    }

    public ChifaIntegrationMode CurrentMode => _config.Mode.ToLowerInvariant() switch
    {
        "test" => ChifaIntegrationMode.Test,
        "production" => ChifaIntegrationMode.Production,
        _ => ChifaIntegrationMode.ReadOnly
    };

    public bool IsReadOnly => CurrentMode == ChifaIntegrationMode.ReadOnly;
    public bool IsTest => CurrentMode == ChifaIntegrationMode.Test;
    public bool IsProduction => CurrentMode == ChifaIntegrationMode.Production;

    public Task<ChifaIntegrationMode> GetModeAsync() => Task.FromResult(CurrentMode);
}
