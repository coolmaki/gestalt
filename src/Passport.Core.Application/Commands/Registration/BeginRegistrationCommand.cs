using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands.Registration;

public sealed record BeginRegistrationCommand(string Email) : ICommand<string>;