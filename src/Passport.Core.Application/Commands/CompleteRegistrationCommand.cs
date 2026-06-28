using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands;

public sealed record CompleteRegistrationCommand(string Email, string AttestationJson) : ICommand<Unit>;