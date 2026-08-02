namespace Passport.Core.Application.Commands;

/// <summary>
/// Result of <see cref="BeginAddPasskeyRegistrationCommand"/>. Contains the
/// WebAuthn <c>optionsJson</c> that the client passes to
/// <c>navigator.credentials.create()</c>.
/// </summary>
public sealed record BeginAddPasskeyRegistrationResult(string OptionsJson);