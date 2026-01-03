namespace mg3.foundry.Features.FoundryCore.Models;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorInfo? Error { get; }

    private Result(bool isSuccess, T? value, ErrorInfo? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string message, string? details = null, int? statusCode = null)
        => new(false, default, new ErrorInfo(message, details, statusCode));
}

/// <summary>
/// Contains detailed error information.
/// </summary>
public record ErrorInfo(string Message, string? Details = null, int? StatusCode = null);
