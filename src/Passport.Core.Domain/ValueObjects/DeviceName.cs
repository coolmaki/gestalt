using Supercluster.Lib.Primitives;

namespace Passport.Core.Domain.ValueObjects;

public sealed record DeviceName : IEquatable<DeviceName>
{
    private const int MaxLength = 100;

    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private DeviceName(string value)
    {
        Value = value;
    }

    public static Result<DeviceName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("device_name.empty", "Device name must not be empty.");
        }

        string trimmed = name.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Error.Validation("device_name.too_long", $"Device name must not exceed {MaxLength} characters.");
        }

        if (trimmed.Any(c => char.IsControl(c)))
        {
            return Error.Validation("device_name.invalid_chars", "Device name must not contain control characters.");
        }

        return new DeviceName(trimmed);
    }

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public string Value { get; }

    // ------------------------------------------------------------
    // Equality
    // ------------------------------------------------------------

    public bool Equals(DeviceName? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();
}