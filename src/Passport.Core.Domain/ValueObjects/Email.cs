using Supercluster.Lib.Primitives;

namespace Passport.Core.Domain.ValueObjects;

public sealed record Email
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Error.Validation("email.empty", "Email must not be empty.");
        }

        string normalized = email.Trim().ToLowerInvariant();

        int atIndex = normalized.IndexOf('@');
        if (atIndex < 1)
        {
            return Error.Validation("email.invalid_format", "Email must contain an '@' with a local part.");
        }

        string domain = normalized[(atIndex + 1)..];
        if (domain.Length == 0 || !domain.Contains('.'))
        {
            return Error.Validation("email.invalid_format", "Email must have a valid domain with a TLD.");
        }

        if (normalized.Length > 254)
        {
            return Error.Validation("email.too_long", "Email must not exceed 254 characters.");
        }

        return new Email(normalized);
    }

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public string Value { get; }
}