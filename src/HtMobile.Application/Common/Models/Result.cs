namespace HtMobile.Application.Common.Models;

/// <summary>Kết quả thao tác (tránh ném exception cho luồng nghiệp vụ thường).</summary>
public class Result
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(string error) => new() { Succeeded = false, Error = error };
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static new Result<T> Failure(string error) => new() { Succeeded = false, Error = error };
}
