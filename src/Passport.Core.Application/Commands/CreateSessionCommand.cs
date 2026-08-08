using Gestalt.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

public sealed record CreateSessionCommand(string Email) : ICommand<SessionResult>;