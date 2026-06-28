using Supercluster.Lib.Application.Commands;

namespace Passport.Core.Application.Commands.Authentication;

public sealed record BeginAuthenticationCommand(string Email) : ICommand<string>;