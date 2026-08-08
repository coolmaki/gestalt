using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Primitives;

namespace Passport.Core.Application.Commands;

public sealed record CompleteAuthenticationCommand(string Email, string AssertionJson) : ICommand<SessionResult>;