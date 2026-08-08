using Gestalt.Lib.Application.Commands;

namespace Passport.Core.Application.Commands;

public sealed record RefreshAccessTokenCommand(string RefreshToken) : ICommand<SessionResult>;