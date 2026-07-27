namespace BMPharma.CHIFA.Interfaces;

public class ChifaWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long ElapsedMs { get; set; }
    public string? CorrelationId { get; set; }
}

public class ChifaWriteResult<T> : ChifaWriteResult
{
    public T? Data { get; set; }
}

public class ChifaWriteError
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRetryable { get; set; }
}

public class ChifaConcurrencyException : Exception
{
    public string Resource { get; }
    public ChifaConcurrencyException(string resource, string message) : base(message)
    {
        Resource = resource;
    }
}

public class ChifaForeignKeyException : Exception
{
    public string Constraint { get; }
    public ChifaForeignKeyException(string constraint, string message) : base(message)
    {
        Constraint = constraint;
    }
}
