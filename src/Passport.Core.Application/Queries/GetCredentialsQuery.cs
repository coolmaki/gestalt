using Supercluster.Lib.Application.Queries;

namespace Passport.Core.Application.Queries;

public sealed record GetCredentialsQuery(string Email) : IQuery<GetCredentialsResult>;