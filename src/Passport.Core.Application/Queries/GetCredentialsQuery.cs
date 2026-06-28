using Supercluster.Lib.Application.Queries;
using Passport.Core.Application.ReadModels;

namespace Passport.Core.Application.Queries;

public sealed record GetCredentialsQuery(string Email) : IQuery<IReadOnlyList<CredentialInfo>>;