namespace FAM.Domain.Common.Base;

/// <summary>
/// Represents the result of an operation without a value.
/// Used for operations that only indicate success or failure.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error that occurred (null if successful)
    /// </summary>
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>
    /// Creates a failed result with an error code
    /// </summary>
    public static Result Failure(string errorCode)
    {
        return new Result(false, new Error(errorCode));
    }

    /// <summary>
    /// Creates a failed result with an error code and details
    /// </summary>
    public static Result Failure(string errorCode, IDictionary<string, object> details)
    {
        return new Result(false, new Error(errorCode, details));
    }

    /// <summary>
    /// Creates a failed result from an Error object
    /// </summary>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Creates a typed successful result
    /// </summary>
    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value, true, null);
    }

    /// <summary>
    /// Creates a typed failed result
    /// </summary>
    public static Result<T> Failure<T>(string errorCode)
    {
        return new Result<T>(default!, false, new Error(errorCode));
    }

    /// <summary>
    /// Creates a typed failed result with details
    /// </summary>
    public static Result<T> Failure<T>(string errorCode, IDictionary<string, object> details)
    {
        return new Result<T>(default!, false, new Error(errorCode, details));
    }

    /// <summary>
    /// Creates a typed failed result from an Error object
    /// </summary>
    public static Result<T> Failure<T>(Error error)
    {
        return new Result<T>(default!, false, error);
    }
}

/// <summary>
/// Represents the result of an operation with a value.
/// Used for operations that return a value on success.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// The value returned by the operation (only available if successful)
    /// </summary>
    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access value of a failed result");

            return _value!;
        }
    }

    internal Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Implicitly convert value to Result
    /// </summary>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    /// <summary>
    /// Implicitly convert Error to failed Result
    /// </summary>
    public static implicit operator Result<T>(Error error)
    {
        return Failure<T>(error);
    }
}
