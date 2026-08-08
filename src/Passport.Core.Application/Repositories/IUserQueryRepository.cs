using Gestalt.Lib.Primitives;
using Passport.Core.Application.ReadModels;

namespace Passport.Core.Application.Repositories;

public interface IUserQueryRepository
{
    /// <summary>
    /// Finds a user by email. Returns <see cref="Option{T}.None"/> if not found.
    /// Uses raw SQL via Dapper.
    /// </summary>
    Task<Option<UserReadModel>> FindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all passkey credentials for a user. Uses raw SQL via Dapper.
    /// </summary>
    Task<IReadOnlyList<CredentialInfo>> GetCredentialsAsync(string email, CancellationToken cancellationToken);
}