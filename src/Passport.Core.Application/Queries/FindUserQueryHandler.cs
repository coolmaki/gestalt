using Gestalt.Lib.Application.Queries;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Repositories;

namespace Passport.Core.Application.Queries;

internal sealed class FindUserQueryHandler(
    IUserQueryRepository userQueryRepo
) : IQueryHandler<FindUserQuery, FindUserResult>
{
    public async Task<Result<FindUserResult>> HandleAsync(FindUserQuery query, CancellationToken cancellationToken)
    {
        var userOption = await userQueryRepo.FindByEmailAsync(query.Email, cancellationToken);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "No user found with this email.");
        }

        return new FindUserResult(userOption.Value);
    }
}