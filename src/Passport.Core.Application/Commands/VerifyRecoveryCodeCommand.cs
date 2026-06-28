using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

public sealed record VerifyRecoveryCodeCommand(string Email, string Code) : ICommand<VerifyRecoveryCodeResult>;