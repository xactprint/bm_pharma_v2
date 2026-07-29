using System.Diagnostics;

namespace BMPharma.CHIFA.Services;

public class CorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();

    public string? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }

    public string GetOrCreate()
    {
        if (_current.Value == null)
            _current.Value = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..12];
        return _current.Value;
    }

    public void Reset() => _current.Value = null;
}
