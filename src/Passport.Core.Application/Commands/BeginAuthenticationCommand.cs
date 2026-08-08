using Gestalt.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

public sealed record BeginAuthenticationCommand(string Email) : ICommand<BeginAuthenticationResult>;