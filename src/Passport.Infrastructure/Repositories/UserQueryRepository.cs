using System.Data.Common;
using Dapper;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;

namespace Passport.Infrastructure.Repositories;

internal sealed class UserQueryRepository(DbConnection connection) : IUserQueryRepository
{
    public async Task<Option<UserReadModel>> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT email AS Email,
                   email_verified AS EmailVerified,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM users
            WHERE email = @Email
            """;

        var user = await connection.QueryFirstOrDefaultAsync<UserReadModel>(sql, new { Email = email });

        return user is null ? Option<UserReadModel>.None : user;
    }

    public async Task<IReadOnlyList<CredentialInfo>> GetCredentialsAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.credential_id AS CredentialId,
                   p.device_name AS DeviceName,
                   p.created_at AS CreatedAt
            FROM passkey_credentials p
            INNER JOIN users u ON u.id = p.user_id
            WHERE u.email = @Email
            ORDER BY p.created_at DESC
            """;

        var credentials = await connection.QueryAsync<CredentialInfo>(sql, new { Email = email });

        return credentials.AsList();
    }
}