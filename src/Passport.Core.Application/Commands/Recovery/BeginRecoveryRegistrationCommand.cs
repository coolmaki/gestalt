using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands.Recovery;

public sealed record BeginRecoveryRegistrationCommand(string RecoveryToken) : ICommand<string>;