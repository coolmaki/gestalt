using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands;

public sealed record CompleteAuthenticationCommand(string Email, string AssertionJson) : ICommand<Unit>;