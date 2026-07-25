namespace BMPharma.CHIFA.Interfaces;

public class ChifaIntegrationConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Mode { get; set; } = "ReadOnly";
    public bool IsReadOnly => Mode.Equals("ReadOnly", StringComparison.OrdinalIgnoreCase);
    public bool IsTest => Mode.Equals("Test", StringComparison.OrdinalIgnoreCase);
    public bool IsProduction => Mode.Equals("Production", StringComparison.OrdinalIgnoreCase);
}
