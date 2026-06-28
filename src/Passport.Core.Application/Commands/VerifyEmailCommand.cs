using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands;

public sealed record VerifyEmailCommand(string Email, string Code) : ICommand<Unit>;