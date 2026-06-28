using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

public sealed record BeginRecoveryRegistrationCommand(string RecoveryToken) : ICommand<BeginRecoveryRegistrationResult>;