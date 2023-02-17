using System;

namespace Ultraleap.TouchFree.Library;

public static class Result
{
    public static Result<Empty> Success { get; } = new();
}

public readonly record struct Empty;

public readonly record struct Error(string Message)
{
    public static Error None { get; } = new();
}

public delegate Result<Empty> ResultPredicate<in T>(T value);

/// <summary>
/// Result that can contain a value or an error
/// </summary>
/// <typeparam name="T">Type of contained value</typeparam>
public readonly record struct Result<T>
{
    public Result(T value)
    {
        this.value = value;
        error = Error.None;
    }

    public Result(Error error)
    {
        value = default;
        this.error = error;
    }

    private readonly T value;
    private readonly Error error;

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);

    public bool IsSuccess => !IsError;
    public bool IsError => error != Error.None;

    public bool TryGetValue(out T value)
    {
        value = this.value;
        return IsSuccess;
    }

    public bool TryGetError(out Error error)
    {
        error = this.error;
        return IsError;
    }

    public Result<T> Match(Action<T> matchFunc, Action<Error> matchError)
    {
        if (IsSuccess) matchFunc(value);
        if (IsError) matchError(error);
        return this;
    }

    public TResult Match<TResult>(Func<T, TResult> matchFunc, Func<Error, TResult> matchError) =>
        IsSuccess
            ? matchFunc(value)
            : matchError(error);

    public Result<TResult> Map<TResult>(Func<T, TResult> mapFunc) => IsSuccess ? mapFunc(value) : error;
    public Result<TResult> Map<TResult>(Func<T, Result<TResult>> mapFunc) => IsSuccess ? mapFunc(value) : error;
}