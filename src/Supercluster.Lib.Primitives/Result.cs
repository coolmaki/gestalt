namespace Supercluster.Lib.Primitives;

public sealed record Result<T>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    /// <summary>
    /// Constructs a <see cref="Result{T}"/>. Marked <c>internal</c> rather than
    /// <c>private</c> to allow mocking libraries (NSubstitute, etc.) to create
    /// instances when returning <see cref="Result{T}"/> values from mocked methods.
    /// Callers should always use <see cref="Success"/> or <see cref="Failure"/>.
    /// </summary>
    internal Result(T value, Error error, bool isSuccess)
    {
        _value = value;
        _error = error;
        IsSuccess = isSuccess;
    }

    public static Result<T> Success(T value) => new(value, default!, isSuccess: true);

    public static Result<T> Failure(Error error) => new(default!, error, isSuccess: false);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);

    // ------------------------------------------------------------
    // Backing Fields
    // ------------------------------------------------------------

    private readonly T _value;

    private readonly Error _error;

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public bool IsSuccess { get; private init; }

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot get value for error result.");

    public Error Error => IsFailure ? _error : throw new InvalidOperationException("Cannot get error for value result.");

    // ------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------

    public TNext Match<TNext>(Func<T, TNext> onSuccess, Func<Error, TNext> onFailure)
    {
        return IsSuccess
            ? onSuccess(_value)
            : onFailure(_error);
    }

    public Result<TNext> Map<TNext>(Func<T, TNext> map)
    {
        return IsSuccess
            ? map(_value)
            : _error;
    }

    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> bind)
    {
        return IsSuccess
            ? bind(_value)
            : _error;
    }
}