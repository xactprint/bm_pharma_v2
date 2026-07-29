namespace BMPharma.CHIFA.Services;

public class ChifaConnectionException : Exception
{
    public string Server { get; }
    public ChifaConnectionException(string server, string message, Exception? inner = null)
        : base(message, inner) { Server = server; }
}

public class ChifaValidationException : Exception
{
    public IReadOnlyList<ChifaValidationError> Errors { get; }
    public ChifaValidationException(string message, List<ChifaValidationError>? errors = null)
        : base(message) { Errors = errors ?? new List<ChifaValidationError>(); }
}

public class ChifaValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ChifaWriteException : Exception
{
    public string EntityType { get; }
    public string EntityKey { get; }
    public ChifaWriteException(string entityType, string entityKey, string message, Exception? inner = null)
        : base(message, inner) { EntityType = entityType; EntityKey = entityKey; }
}

public class ChifaConcurrencyException : Exception
{
    public string Resource { get; }
    public ChifaConcurrencyException(string resource, string message, Exception? inner = null)
        : base(message, inner) { Resource = resource; }
}

public class ChifaSynchronizationException : Exception
{
    public string Source { get; }
    public ChifaSynchronizationException(string source, string message, Exception? inner = null)
        : base(message, inner) { Source = source; }
}

public class ChifaVisibilityException : Exception
{
    public string EntityType { get; }
    public string EntityKey { get; }
    public ChifaVisibilityException(string entityType, string entityKey, string message, Exception? inner = null)
        : base(message, inner) { EntityType = entityType; EntityKey = entityKey; }
}
