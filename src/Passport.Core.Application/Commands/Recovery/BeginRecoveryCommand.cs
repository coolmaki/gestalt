using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands.Recovery;

public sealed record BeginRecoveryCommand(string Email) : ICommand<Unit>;