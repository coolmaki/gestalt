namespace Supercluster.Lib.Primitives;

public sealed record Error
{
    // ------------------------------------------------------------
    // Constructors & Factories
    // ------------------------------------------------------------

    internal Error(ErrorType type, string code, string description)
    {
        Type = type;
        Code = code;
        Description = description;
    }

    /// <summary>
    /// Input failed validation rules — malformed data, missing required fields,
    /// values out of range, business invariants violated by the caller's input.<br/><br/>
    /// Maps to HTTP 400 Bad Request.<br/><br/>
    /// Examples:<br/>
    /// - "Email is not a valid format"<br/>
    /// - "Amount must be positive"<br/>
    /// - "Order must contain at least one line item"
    /// </summary>
    public static Error Validation(string code, string description) => new(ErrorType.Validation, code, description);

    /// <summary>
    /// The requested resource or entity does not exist.<br/><br/>
    /// Maps to HTTP 404 Not Found.<br/><br/>
    /// Examples:<br/>
    /// - "Customer with ID 123 not found"<br/>
    /// - "No order matches that reference"<br/><br/>
    /// Use this when absence is a legitimate outcome the caller should handle —
    /// not when something that *should* exist is mysteriously missing (that's a bug).
    /// </summary>
    public static Error NotFound(string code, string description) => new(ErrorType.NotFound, code, description);

    /// <summary>
    /// The operation conflicts with the current state of the system.
    /// The request is well-formed but can't be applied right now.<br/><br/>
    /// Maps to HTTP 409 Conflict.<br/><br/>
    /// Examples:<br/>
    /// - "Email already registered"<br/>
    /// - "Order already shipped and cannot be cancelled"<br/>
    /// - "Concurrency conflict — entity was modified by another user"<br/>
    /// - "Insufficient stock to fulfill order"
    /// </summary>
    public static Error Conflict(string code, string description) => new(ErrorType.Conflict, code, description);

    /// <summary>
    /// The caller is not permitted to perform this operation.
    /// Covers both authentication failures (no/invalid identity) and authorization
    /// failures (valid identity but insufficient permissions). If you need to
    /// distinguish, split into Unauthenticated (401) and Forbidden (403).<br/><br/>
    /// Maps to HTTP 401 or 403.<br/><br/>
    /// Examples:<br/>
    /// - "Must be logged in"<br/>
    /// - "Only the order owner can cancel this order"<br/>
    /// - "Admin role required"
    /// </summary>
    public static Error Unauthorized(string code, string description) => new(ErrorType.Unauthorized, code, description);

    /// <summary>
    /// A failure that wasn't anticipated by the domain — typically used as a
    /// fallback when wrapping infrastructure exceptions into a Result at a boundary.<br/><br/>
    /// Maps to HTTP 500 Internal Server Error.<br/><br/>
    /// Examples:<br/>
    /// - "Database connection failed"<br/>
    /// - "Third-party API returned malformed response"<br/><br/>
    /// Prefer letting these remain as exceptions when possible; only use this variant
    /// when you have a specific reason to convert an exception into a Result.
    /// </summary>
    public static Error Unexpected(string code, string description) => new(ErrorType.Unexpected, code, description);

    // ------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------

    public ErrorType Type { get; private init; }

    public string Code { get; private init; }

    public string Description { get; private init; }
}