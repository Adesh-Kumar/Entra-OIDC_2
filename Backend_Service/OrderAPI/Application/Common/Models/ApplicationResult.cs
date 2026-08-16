namespace OrderAPI.Application.Common.Models;

public enum ApplicationResultStatus
{
    Success,
    NotFound,
    Forbidden,
    Failure
}

public sealed class ApplicationResult<T>
{
    public T? Value { get; init; }
    public ApplicationResultStatus Status { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsSuccess => Status == ApplicationResultStatus.Success;

    public static ApplicationResult<T> Success(T value) =>
        new() { Value = value, Status = ApplicationResultStatus.Success };

    public static ApplicationResult<T> NotFound(string? message = null) =>
        new() { Status = ApplicationResultStatus.NotFound, ErrorMessage = message };

    public static ApplicationResult<T> Forbidden(string? message = null) =>
        new() { Status = ApplicationResultStatus.Forbidden, ErrorMessage = message };

    public static ApplicationResult<T> Failure(string message) =>
        new() { Status = ApplicationResultStatus.Failure, ErrorMessage = message };
}

public sealed class ApplicationResult
{
    public ApplicationResultStatus Status { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsSuccess => Status == ApplicationResultStatus.Success;

    public static ApplicationResult Success() =>
        new() { Status = ApplicationResultStatus.Success };

    public static ApplicationResult NotFound(string? message = null) =>
        new() { Status = ApplicationResultStatus.NotFound, ErrorMessage = message };

    public static ApplicationResult Forbidden(string? message = null) =>
        new() { Status = ApplicationResultStatus.Forbidden, ErrorMessage = message };

    public static ApplicationResult Failure(string message) =>
        new() { Status = ApplicationResultStatus.Failure, ErrorMessage = message };
}
