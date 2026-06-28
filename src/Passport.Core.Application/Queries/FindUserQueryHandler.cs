using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Application.ReadModels;

namespace Passport.Core.Application.Queries;

internal sealed class FindUserQueryHandler : IQueryHandler<FindUserQuery, UserReadModel>
{
    private readonly IUserQueryRepository _userQueryRepo;

    public FindUserQueryHandler(IUserQueryRepository userQueryRepo)
    {
        _userQueryRepo = userQueryRepo;
    }

    public async Task<Result<UserReadModel>> HandleAsync(FindUserQuery query, CancellationToken ct)
    {
        var userOption = await _userQueryRepo.FindByEmailAsync(query.Email, ct);
        if (userOption.IsNone)
        {
            return Error.NotFound("user.not_found", "No user found with this email.");
        }

        return userOption.Value;
    }
}