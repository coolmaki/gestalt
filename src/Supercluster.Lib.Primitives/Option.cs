namespace Supercluster.Lib.Primitives;

public sealed class Option<T> : IEquatable<Option<T>>
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    /// <summary>
    /// Constructs an <see cref="Option{T}"/>. Marked <c>internal</c> rather than
    /// <c>private</c> to allow mocking libraries (NSubstitute, etc.) to create
    /// instances when returning <see cref="Option{T}"/> values from mocked methods.
    /// Callers should always use <see cref="Some"/> or <see cref="None"/>.
    /// </summary>
    internal Option(T value, bool isSome)
    {
        _value = value;
        IsSome = isSome;
    }

    public static Option<T> Some(T value) => new(value, isSome: true);

    public static Option<T> None => new(default!, isSome: false);

    public static implicit operator Option<T>(T value) => Some(value);

    // ------------------------------------------------------------
    // Backing Fields
    // ------------------------------------------------------------

    private readonly T _value;

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public bool IsSome { get; }

    public bool IsNone => !IsSome;

    public T Value => IsSome ? _value : throw new InvalidOperationException("Cannot get value for none option.");

    // ------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------

    public TNext Match<TNext>(Func<T, TNext> onSome, Func<TNext> onNone)
    {
        return IsSome
            ? onSome(_value)
            : onNone();
    }

    public Option<TNext> Map<TNext>(Func<T, TNext> map)
    {
        return IsSome
            ? map(_value)
            : Option<TNext>.None;
    }

    public Option<TNext> Bind<TNext>(Func<T, Option<TNext>> bind)
    {
        return IsSome
            ? bind(_value)
            : Option<TNext>.None;
    }

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(Option<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (IsNone && other.IsNone)
        {
            return true;
        }

        if (IsSome && other.IsSome)
        {
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        return false;
    }

    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    public override int GetHashCode() => IsSome ? _value?.GetHashCode() ?? 0 : 0;
}