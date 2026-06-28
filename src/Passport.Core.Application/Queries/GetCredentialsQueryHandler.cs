using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports.Repositories;

namespace Passport.Core.Application.Queries;

internal sealed class GetCredentialsQueryHandler(
    IUserQueryRepository userQueryRepo
) : IQueryHandler<GetCredentialsQuery, GetCredentialsResult>
{
    public async Task<Result<GetCredentialsResult>> HandleAsync(GetCredentialsQuery query, CancellationToken ct)
    {
        var credentials = await userQueryRepo.GetCredentialsAsync(query.Email, ct);
        return new GetCredentialsResult(credentials);
    }
}