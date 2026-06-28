using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands.Recovery;

public sealed record VerifyRecoveryCodeCommand(string Email, string Code) : ICommand<string>;