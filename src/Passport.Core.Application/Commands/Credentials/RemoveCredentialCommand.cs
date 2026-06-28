using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Primitives;

namespace Passport.Core.Application.Commands.Credentials;

public sealed record RemoveCredentialCommand(string Email, byte[] CredentialId) : ICommand<Unit>;