using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Primitives;

namespace Passport.Core.Application.Commands;

public sealed record RevokeRefreshTokenCommand(string Email, string TokenHash) : ICommand<Unit>;