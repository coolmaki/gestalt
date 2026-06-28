using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Ports;
using Passport.Core.Application.ReadModels;

namespace Passport.Core.Application.Queries;

internal sealed class GetCredentialsQueryHandler : IQueryHandler<GetCredentialsQuery, IReadOnlyList<CredentialInfo>>
{
    private readonly IUserQueryRepository _userQueryRepo;

    public GetCredentialsQueryHandler(IUserQueryRepository userQueryRepo)
    {
        _userQueryRepo = userQueryRepo;
    }

    public async Task<Result<IReadOnlyList<CredentialInfo>>> HandleAsync(GetCredentialsQuery query, CancellationToken ct)
    {
        var credentials = await _userQueryRepo.GetCredentialsAsync(query.Email, ct);
        return Result<IReadOnlyList<CredentialInfo>>.Success(credentials);
    }
}