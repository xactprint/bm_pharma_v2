namespace BMPharma.Shared.Results;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(T? value, bool isSuccess, string? error, IReadOnlyList<string>? errors)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? Array.Empty<string>();
    }

    public static Result<T> Success(T value) => new(value, true, null, null);
    public static Result<T> Failure(string error) => new(default, false, error, new[] { error });
    public static Result<T> Failure(IReadOnlyList<string> errors) => new(default, false, null, errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}
