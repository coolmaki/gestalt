using Gestalt.Lib.Application.Queries;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Repositories;

namespace Passport.Core.Application.Queries;

internal sealed class GetCredentialsQueryHandler(
    IUserQueryRepository userQueryRepo
) : IQueryHandler<GetCredentialsQuery, GetCredentialsResult>
{
    public async Task<Result<GetCredentialsResult>> HandleAsync(GetCredentialsQuery query, CancellationToken cancellationToken)
    {
        var credentials = await userQueryRepo.GetCredentialsAsync(query.Email, cancellationToken);
        return new GetCredentialsResult(credentials);
    }
}