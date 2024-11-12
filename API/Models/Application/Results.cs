using System.Diagnostics.CodeAnalysis;

namespace API.Models.Application;

public class Result<TValue, TError>
{
    private Result(TValue value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(TError error)
    {
        Error = error;
        IsSuccess = false;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public TValue? Value { get; }

    public TError? Error { get; }

    public static Result<TValue, TError> FromSuccess(TValue value) => new(value);
    public static Result<TValue, TError> FromError(TError error) => new(error);

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);
}

public static class EmptyResult
{
    public static EmptyResult<TError> FromSuccess<TError>() => EmptyResult<TError>.FromSuccess();
}

public class EmptyResult<TError>
{
    private EmptyResult()
    {
        IsSuccess = true;
    }

    private EmptyResult(TError error)
    {
        Error = error;
        IsSuccess = false;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public TError? Error { get; }

    public static EmptyResult<TError> FromSuccess() => new();
    public static EmptyResult<TError> FromError(TError error) => new(error);

    public static implicit operator EmptyResult<TError>(TError error) => new(error);
}