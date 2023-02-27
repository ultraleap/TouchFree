using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultraleap.TouchFree.Library;

public static class Result
{
    public static Result<Empty> Success { get; } = new();
}

public readonly record struct Empty;

public readonly record struct Error(string Message)
{
    public static Error None { get; } = new();
    
    public Error(string message, IReadOnlyCollection<Error> children) : this(message)
    {
        Message = message;
        Children = children;
        ErrorCount = children.Count > 0 ? children.Sum(c => c.ErrorCount) : 1;
        var errorCount = ErrorCount;

        if (children.Count > 0)
        {
            var builder = new StringBuilder();
            int indentation = 0;

            void AppendError(Error error)
            {
                builder.Append(' ', indentation * 4);
                builder.Append(error.Message);
                if (error.Children.Count > 0)
                {
                    builder.Append($". {errorCount} errors:");
                }
                AppendChildren(error.Children);
            }

            void AppendChildren(IReadOnlyCollection<Error> errors)
            {
                if (errors.Count < 1) return;
                indentation++;
                foreach (var child in errors)
                {
                    builder.AppendLine();
                    AppendError(child);
                }
                indentation--;
            }
            
            AppendError(this);
            MessageTree = builder.ToString();
        }
        else
        {
            MessageTree = Message;
        }
    }

    public IReadOnlyCollection<Error> Children { get; } = Array.Empty<Error>();
    public string MessageTree { get; } = Message;

    private int ErrorCount { get; } = 1;

    // WARNING: These operators are explicit deliberately.
    // Making them implicit causes issues with conversion to/from Error when dealing with Result<string>
    public static explicit operator string(Error error) => error.Message;
    public static explicit operator Error(string error) => new(error);
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

    /// <summary>
    /// Transform Result with a mapping function if it is not an error.
    /// For errors, the error will be propagated and the mapping function will not be called. 
    /// </summary>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapFunc) => IsSuccess ? mapFunc(value) : error;
    
    /// <summary>
    /// Transform Result with a mapping function if it is not an error.
    /// For errors, the error will be propagated and the mapping function will not be called. 
    /// </summary>
    public Result<TResult> Map<TResult>(Func<T, Result<TResult>> mapFunc) => IsSuccess ? mapFunc(value) : error;
}