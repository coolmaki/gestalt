using Gestalt.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Validates the device verification code from the add-passkey flow.
/// On success, returns an <c>AddPasskeyToken</c> that authorizes the caller
/// to proceed to WebAuthn credential registration (Phase B).
/// <para>
/// Phase A (email verification) — step 2 of 4 in the add-passkey flow.
/// </para>
/// </summary>
public sealed record VerifyAddPasskeyCodeCommand(string Email, string Code) : ICommand<VerifyAddPasskeyCodeResult>;