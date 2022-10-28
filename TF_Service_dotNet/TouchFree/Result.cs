using System;

namespace Ultraleap.TouchFree.Library;

/// <summary>
/// Result that can contain a value or an error
/// </summary>
/// <typeparam name="T">Type of contained value</typeparam>
public abstract record Result<T>
{
    /// <summary>
    /// Type that represents a contained value
    /// </summary>
    public record Some(T Value) : Result<T>
    {
        public static implicit operator Some(T value) => new(value);
    }

    /// <summary>
    /// Type that represents an error
    /// </summary>
    public record Error(string ErrorValue) : Result<T>
    {
        public static implicit operator Error(string error) => new(error);
    }

    public static implicit operator Result<T>(T value) => new Some(value);
    public static implicit operator Result<T>(string error) => new Error(error);

    public bool IsSuccess => !IsError;
    public bool HasValue => this is Some;
    public bool IsError => this is Error;

    public bool TryGetValue(out T value)
    {
        if (this is not Some)
        {
            value = default;
            return false;
        }

        value = ((Some)this).Value;
        return true;
    }

    public bool TryGetError(out string error)
    {
        if (this is not Error)
        {
            error = default;
            return false;
        }

        error = ((Error)this).ErrorValue;
        return true;
    }

    public bool GetValueOrError(out T value, out string error)
    {
        value = HasValue ? ((Some)this).Value : default;
        error = IsError ? ((Error)this).ErrorValue : default;
        return HasValue;
    }

    public Result<T> Match(Action<T> matchFunc, Action<string> matchError)
    {
        if (HasValue) matchFunc(((Some)this).Value);
        if (IsError) matchError(((Error)this).ErrorValue);
        return this;
    }

    public TResult Match<TResult>(Func<T, TResult> matchFunc, Func<string, TResult> matchError) =>
        HasValue
            ? matchFunc(((Some)this).Value)
            : matchError(((Error)this).ErrorValue);
}