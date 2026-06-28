using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;

namespace Passport.Core.Application.Queries;

internal sealed class FindUserQueryHandler(
    IUserQueryRepository userQueryRepo
) : IQueryHandler<FindUserQuery, FindUserResult>
{
    public async Task<Result<FindUserResult>> HandleAsync(FindUserQuery query, CancellationToken ct)
    {
        var userOption = await userQueryRepo.FindByEmailAsync(query.Email, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "No user found with this email.");
        }

        return new FindUserResult(userOption.Value);
    }
}