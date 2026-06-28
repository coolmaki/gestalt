using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

public sealed record BeginRegistrationCommand(string Email) : ICommand<BeginRegistrationResult>;