using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Primitives;

namespace Passport.Core.Application.Commands;

public sealed record VerifyEmailCommand(string Email, string Code) : ICommand<Unit>;